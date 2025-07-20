using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace FlowCAD_core
{
    internal class BoyProfilUiAutomation
    {
        public static void StartBoyProfilMakro()
        {
            // Show file dialog to select the DVB file
            string dvbPath = null;
            using (var openFileDialog = new System.Windows.Forms.OpenFileDialog())
            {
                openFileDialog.Title = "Select DVB Macro File";
                openFileDialog.Filter = "AutoCAD VBA Macro (*.dvb)|*.dvb";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    dvbPath = openFileDialog.FileName;
                }
                else
                {
                    // User cancelled
                    Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\nMacro loading cancelled.");
                    return;
                }
            }

            dynamic acadApp = Marshal.GetActiveObject("AutoCAD.Application");
            acadApp.LoadDVB(dvbPath);

            try
            {
                acadApp.RunMacro("Module1.profil_makrosu");
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to run the macro. Please check the macro name and path.", ex);
            }
        }

        public static BoyProfilCircle MapInternalCircleToBoyProfilCircle(Circle circle, bool isLastCircle)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            BoyProfilCircle boyProfilCircle = new BoyProfilCircle();
            TypedValue[] circleValues = Utils.fetchXrecordOfObject(circle.ObjectId).Data.AsArray();

            // set baca no
            boyProfilCircle.BacaNo = circleValues[7].Value.ToString();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // set kapak kot
                string izahatCemberiHandleString = circleValues[9].Value.ToString();
                ObjectId izahatCemberiObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(izahatCemberiHandleString, 16)), 0);
                Circle izahatCemberi = (Circle)tr.GetObject(izahatCemberiObjectId, OpenMode.ForRead);
                TypedValue[] izahatCemberiData = ((Xrecord)Utils.fetchXrecordOfObject(izahatCemberiObjectId)).Data.AsArray();
                string textObjectKkHandleString = izahatCemberiData[7].Value.ToString();
                ObjectId textObjectKkObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(textObjectKkHandleString, 16)), 0);
                DBText kapakKotuText = (DBText)tr.GetObject(textObjectKkObjectId, OpenMode.ForRead);
                string kapakKotuTextOpeningParanthesisRemoved = kapakKotuText.TextString.Replace("(", "");
                string kapakKotuTextClosingParanthesisRemoved = kapakKotuTextOpeningParanthesisRemoved.Replace(")", "");
                boyProfilCircle.KapakKot = kapakKotuTextClosingParanthesisRemoved;

                // set giriş kot
                bool isMultiInDimension = false;

                string dimInHandleString = circleValues[15].Value.ToString();
                if (dimInHandleString == "-1")
                {
                    boyProfilCircle.GirişKot = "-1";
                }
                else if(dimInHandleString.Contains("%"))
                {
                    isMultiInDimension = true;
                }
                else
                {
                    ObjectId dimInObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(dimInHandleString, 16)), 0);
                    TypedValue[] dimInData = ((Xrecord)Utils.fetchXrecordOfObject(dimInObjectId)).Data.AsArray();
                    string textObjectGirisHandleString = dimInData[9].Value.ToString();
                    ObjectId textObjectGirisObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(textObjectGirisHandleString, 16)), 0);
                    DBText girisKotText = (DBText)tr.GetObject(textObjectGirisObjectId, OpenMode.ForRead);
                    boyProfilCircle.GirişKot = girisKotText.TextString;
                }


                // set akar kot and cap

                string dimOutHandleString = circleValues[13].Value.ToString();
                if (dimOutHandleString == "-1")
                {
                    boyProfilCircle.AkarKot = "-1";
                    boyProfilCircle.Cap = 0;
                }
                else
                {
                    ObjectId dimOutObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(dimOutHandleString, 16)), 0);
                    AlignedDimension circleOutDimension = (AlignedDimension)tr.GetObject(dimOutObjectId, OpenMode.ForRead);
                    TypedValue[] dimOutData = ((Xrecord)Utils.fetchXrecordOfObject(dimOutObjectId)).Data.AsArray();
                    string textObjectAkarHandleString = dimOutData[7].Value.ToString();
                    ObjectId textObjectAkarObjectId = db.GetObjectId(false, new Handle(Convert.ToInt64(textObjectAkarHandleString, 16)), 0);
                    DBText akarKotText = (DBText)tr.GetObject(textObjectAkarObjectId, OpenMode.ForRead);
                    boyProfilCircle.AkarKot = akarKotText.TextString;
                    if(isMultiInDimension)
                    {
                        boyProfilCircle.GirişKot = akarKotText.TextString;
                    }
                    if(isLastCircle)
                    {
                        boyProfilCircle.Cap = 0;
                    }
                    else
                    {
                        boyProfilCircle.Cap = MapDimensionCapInfoToCapInt(circleOutDimension.DimensionText);
                    }
                       
                }

                tr.Commit();
            }

            // set circle location

            boyProfilCircle.XKoordinat = circle.Center.X;
            boyProfilCircle.YKoordinat = circle.Center.Y;

            ed.WriteMessage("Circle mapped to BoyProfilCircle successfully.\n");

            return boyProfilCircle;
        }

        public static void SerializeAndSaveBoyProfilCirclesFile(List<BoyProfilCircle> circles)
        {
            Document doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;

            try
            {
                // Get current user's Desktop directory
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // Search for any directory under Desktop containing "FlowCAD"
                string[] flowCadDirs = Directory.GetDirectories(desktopPath, "*FlowCAD-distro*", SearchOption.TopDirectoryOnly);

                if (flowCadDirs.Length == 0)
                {
                    ed.WriteMessage("\nError: No directory containing 'FlowCAD' found on Desktop.");
                    return;
                }

                // Pick the first matching FlowCAD directory
                string flowCadDir = flowCadDirs[0];

                // Build path for hat.json inside that directory
                string fullPath = Path.Combine(flowCadDir, "hat.json");

                // Serialize and save JSON
                string json = JsonConvert.SerializeObject(circles, Formatting.Indented);
                File.WriteAllText(fullPath, json);

                ed.WriteMessage($"\nFile saved successfully at: {fullPath}");
            }
            catch (Exception ex)
            {
                ed.WriteMessage("\nError saving file: " + ex.Message);
                ed.WriteMessage("\nStack Trace: " + ex.StackTrace);
            }
        }

        private static int MapDimensionCapInfoToCapInt(string dimensionText)
        {
            if (dimensionText.Contains("200"))
            {
                return 200;
            }
            else if (dimensionText.Contains("300"))
            {
                return 300;
            }
            else if (dimensionText.Contains("400"))
            {
                return 400;
            }
            else if (dimensionText.Contains("500"))
            {
                return 500;
            }
            else if (dimensionText.Contains("600"))
            {
                return 600;
            }
            else if (dimensionText.Contains("700"))
            {
                return 700;
            }
            else if (dimensionText.Contains("800"))
            {
                return 800;
            }
            else
            {
                throw new ArgumentException($"Unknown dimension text: {dimensionText}");
            }

        }
    }

}
