using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

namespace BoyProfilUIAutomation
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;


        [STAThread]
        static void Main()
        {
            List<BoyProfilCircle> circles = BoyProfilCircleJsonMapper.retrieveBoyProfilCircles();
            AutomationElement macroWindow = SetMacroWindowForeground();

            // macro window set to foreground, moving to typing values logic

            FillBacaNoKapakKotGirisKot(circles);

            // jump to AKARKOT first input field and fill the fields 

            System.Windows.Forms.MessageBox.Show("Lütfen ilk AKAR KOT text alanına tıklayın.");
            UserFocusAndSetToTextFields(OperationType.AKARKOT, circles);


            // jump to Y KOORDINAT first input field and fill the fields


            System.Windows.Forms.MessageBox.Show("Lütfen ilk Y KOORDINAT text alanına tıklayın.");
            UserFocusAndSetToTextFields(OperationType.KOORDINAT, circles);


            System.Windows.Forms.MessageBox.Show("Automation completed successfully.");
        }

        private static void FillBacaNoKapakKotGirisKot(List<BoyProfilCircle> circles)
        {
            int TAB_COUNT_36 = 36; // Number of tabs to send
            int TAB_COUNT_18 = 16; // Number of tabs to send
            Console.WriteLine("Typing values...");

            // jump out button keys
            for (int i = 0; i<TAB_COUNT_36; i++)
            {
                SendKeys.SendWait("{TAB}");
            }

            //set values to baca no text field
            for (int i = 0; i<= TAB_COUNT_18; i++)
            {
                if(i < circles.Count)
                {
                    SendMessageWithKeys(circles[i].BacaNo);
                }
                
                Thread.Sleep(5);
                SendKeys.SendWait("{TAB}");
            }

            // jump to next colon
            SendKeys.SendWait("{TAB}");


            //set values to kapak kot text field
            for (int i = 0; i<=TAB_COUNT_18; i++)
            {
                if (i < circles.Count)
                {
                    SendMessageWithKeys(circles[i].KapakKot);
                }
                SendKeys.SendWait("{TAB}");
            }


            // jump to next colon
            SendKeys.SendWait("{TAB}");
            SendKeys.SendWait("{TAB}");


            // jump out button keys
            for (int i = 0; i<TAB_COUNT_18; i++)
            {
                SendKeys.SendWait("{TAB}");
            }


            //set values to giriş kot text field
            for (int i = 0; i<TAB_COUNT_18; i++)
            {
                if (i < circles.Count && circles[i].GirişKot != circles[i].AkarKot)
                {
                    SendMessageWithKeys(circles[i].GirişKot);
                }
                SendKeys.SendWait("{TAB}");
            }

        }

        private static AutomationElement SetMacroWindowForeground()
        {
            string macroWindowTitle = "PROFİL ÇİZİMİ - FERHAT COŞKUN - WWW.PRATIKTE.COM";
            Console.WriteLine("Scanning top-level windows...\n");

            var allWindows = AutomationElement.RootElement.FindAll(TreeScope.Children, Condition.TrueCondition);
            AutomationElement acadWindow = null;

            foreach (AutomationElement window in allWindows)
            {
                string name = window.Current.Name;
                string className = window.Current.ClassName;

                Console.WriteLine($"Found Window: Title = '{name}', Class = '{className}'");

                // Try to find AutoCAD or Autodesk window
                if ((name != null && name.ToLower().Contains("autocad")) ||(name != null && name.ToLower().Contains("autodesk")))
                {
                    acadWindow = window;
                    Console.WriteLine("\n>> Likely AutoCAD Window Found!");
                }
            }

            if (acadWindow == null)
            {
                MessageBox.Show("AutoCAD main window not found.");
                return null;
            }

            Console.WriteLine("Focusing AutoCAD...");
            SetForegroundWindow((IntPtr)acadWindow.Current.NativeWindowHandle);
            Thread.Sleep(500);  // Let AutoCAD fully get focus

            Console.WriteLine("Searching for macro window...");
            AutomationElement macroWindow = null;
            for (int i = 0; i < 20 && macroWindow == null; i++)
            {
                macroWindow = acadWindow.FindFirst(TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.NameProperty, macroWindowTitle));
                Thread.Sleep(300);
            }

            if (macroWindow == null)
            {
                MessageBox.Show("Macro window not found inside AutoCAD.");
                return null;
            }

            Console.WriteLine("Focusing macro window...");
            SetForegroundWindow((IntPtr)macroWindow.Current.NativeWindowHandle);
            Thread.Sleep(500);

            return macroWindow;
        }

        private static System.Drawing.Point WaitForLeftClick()
        {
            System.Drawing.Point clickedPoint = new System.Drawing.Point();
            bool clicked = false;

            using (var listener = new MouseClickListener())
            {
                listener.LeftClicked += (s, e) =>
                {
                    clickedPoint = new System.Drawing.Point(e.Location.X, e.Location.Y);
                    clicked = true;
                };

                while (!clicked)
                {
                    Thread.Sleep(100);
                    Application.DoEvents();
                }
            }

            return clickedPoint;
        }

        private static void UserFocusAndSetToTextFields(OperationType operationType, List<BoyProfilCircle> circles)
        {
            // Wait for the user to click
            Console.WriteLine("Waiting for mouse click...");
            System.Drawing.Point clickedPoint = WaitForLeftClick();
            System.Windows.Point uiPoint = new System.Windows.Point(clickedPoint.X, clickedPoint.Y);

            Console.WriteLine($"Clicked at: {clickedPoint.X}, {clickedPoint.Y}");

            // Get the UI element under clicked point
            AutomationElement clickedElement = AutomationElement.FromPoint(uiPoint);

            if (clickedElement == null)
            {
                MessageBox.Show("Tıklanan yerde bir öğe bulunamadı.");
                return;
            }

            if (clickedElement.Current.ControlType == ControlType.Edit)
            {
                if (clickedElement.TryGetCurrentPattern(ValuePattern.Pattern, out object patternObj))
                {
                    var pattern = (ValuePattern)patternObj;
                    if (!pattern.Current.IsReadOnly)
                    {
                        try
                        {
                            System.Windows.Rect rect = clickedElement.Current.BoundingRectangle;
                            int centerX = (int)(rect.Left + rect.Width / 2);
                            int centerY = (int)(rect.Top + rect.Height / 2);

                            // Move mouse and click to set focus manually
                            Cursor.Position = new System.Drawing.Point(centerX, centerY);
                            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, centerX, centerY, 0, 0);

                            // Small delay to allow UI to process the focus
                            Thread.Sleep(200);

                            // Now try to set the value for AKAR KOT

                            switch (operationType)
                            {
                                case OperationType.AKARKOT:
                                    DoAkarkotOperation(circles);
                                    break;
                                case OperationType.KOORDINAT:
                                    DoKoordinatOperation(circles);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null);
                            }

                            MessageBox.Show("Değer başarıyla yazıldı.");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Yazılamadı: " + ex.Message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Bu öğeye değer atanamıyor.");
                }
            }
            else
            {
                MessageBox.Show("Tıklanan öğe bir text alanı değil.");
            }
        }

        private static void DoAkarkotOperation(List<BoyProfilCircle> circles)
        {
            SendMessageWithKeys(circles[0].AkarKot);


            for (int i = 0; i<=18; i++)
            {
                SendKeys.SendWait("{TAB}");
                Thread.Sleep(5);
            }

            for (int i = 0; i<16; i++)
            {
                if(i < circles.Count -2)
                {
                    if (circles[i+2].AkarKot != "-1")
                    {
                        SendMessageWithKeys(circles[i+2].AkarKot);
                    }
                    else
                    {
                        SendMessageWithKeys(circles[i+2].GirişKot);
                    }
                }
                Thread.Sleep(5);
                SendKeys.SendWait("{TAB}");

            }

            for (int i = 0; i<16; i++)
            {
                if(i < circles.Count)
                {
                    SendMessageWithKeys(circles[i].Cap);
                }
                SendKeys.SendWait("{TAB}");
            }

        }

        private static void DoKoordinatOperation(List<BoyProfilCircle> circles)
        {
            for (int i = 0; i<15; i++)
            {
                if(i < circles.Count)
                {
                    SendMessageWithKeys(circles[i].YKoordinat);
                    SendKeys.SendWait("{TAB}");
                    SendMessageWithKeys(circles[i].XKoordinat);
                }
                SendKeys.SendWait("{TAB}");
            }
        }

        private static void SendMessageWithKeys(string message)
        {
            foreach (char c in message)
            {
                SendKeys.SendWait(c.ToString());
                Thread.Sleep(5); // Small delay to ensure each key is processed
            }
        }

        private static void SendMessageWithKeys(int message)
        {
            // Convert the integer to a string and send each character
            if(message == 0)
            {
                return;
            }
            string messageStr = message.ToString();
            foreach (char c in messageStr)
            {
                SendKeys.SendWait(c.ToString());
                Thread.Sleep(5); // Small delay to ensure each key is processed
            }
        }

        private static void SendMessageWithKeys(double message)
        {
            // Convert the integer to a string and send each character
            string messageStr = message.ToString("F2");
            foreach (char c in messageStr)
            {
                SendKeys.SendWait(c.ToString());
                Thread.Sleep(5); // Small delay to ensure each key is processed
            }
        }
    }
}
