using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;


namespace FlowCAD_desktop
{
    public partial class MainForm : Form
    {

        private dynamic acadApp = null;
        private dynamic acadDoc = null;
        private bool isConnected = false;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Update UI to show not connected initially
            UpdateConnectionStatus(false);
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // Try to get a running instance of AutoCAD
                try
                {
                    acadApp = Marshal.GetActiveObject("AutoCAD.Application");
                }
                catch
                {
                    // If no running instance, create a new one
                    Type acType = Type.GetTypeFromProgID("AutoCAD.Application");
                    acadApp = Activator.CreateInstance(acType);
                    acadApp.Visible = true;
                }

                // Check if there's an active document
                if (acadApp.ActiveDocument != null)
                {
                    acadDoc = acadApp.ActiveDocument;
                    isConnected = true;
                    UpdateConnectionStatus(true);
                    MessageBox.Show("Successfully connected to AutoCAD!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Please open AutoCAD and create a new drawing before connecting.",
                        "No Active Document", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    isConnected = false;
                    UpdateConnectionStatus(false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to AutoCAD: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                isConnected = false;
                UpdateConnectionStatus(false);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void UpdateConnectionStatus(bool connected)
        {
            if (connected)
            {
                statusLabel.Text = "Connected to AutoCAD";
                btnConnect.Enabled = false;
                btnStartAutomation.Enabled = true;  // Enable automation button when connected
            }
            else
            {
                statusLabel.Text = "Not connected to AutoCAD";
                btnConnect.Enabled = true;
                btnStartAutomation.Enabled = false;  // Disable automation button when not connected
                tabSystemCommands.Enabled = false;  // Disable the tab when disconnected
            }
        }

        private void OpenExistingDrawing()
        {
            try
            {
                // First make sure we're connected to AutoCAD
                if (acadApp == null)
                {
                    MessageBox.Show("Not connected to AutoCAD yet.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create an OpenFileDialog to let the user select a drawing file
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "AutoCAD Drawings (*.dwg)|*.dwg|All files (*.*)|*.*",
                    Title = "Open AutoCAD Drawing"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the path of the selected file
                    string filePath = openFileDialog.FileName;

                    // Open the selected drawing
                    acadDoc = acadApp.Documents.Open(filePath);

                    isConnected = true;
                    UpdateConnectionStatus(true);
                    MessageBox.Show($"Successfully opened drawing: {Path.GetFileName(filePath)}",
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening drawing: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenDrawing_Click(object sender, EventArgs e)
        {
            OpenExistingDrawing();
        }

        private void btnStartAutomation_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we're connected
                if (acadDoc == null || !isConnected)
                {
                    MessageBox.Show("Please connect to AutoCAD first.",
                        "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Path to your automation script (.dll or .NET assembly)
                string scriptPath = Path.Combine(Application.StartupPath, @"..\FlowCAD-core.dll");
                scriptPath = Path.GetFullPath(scriptPath);

                // You might want to allow the user to select the script file
                // Or store the path in a configuration file

                // Make sure the file exists
                if (!File.Exists(scriptPath))
                {
                    MessageBox.Show($"Automation script not found at: {scriptPath}",
                        "File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Execute the NETLOAD command with the script path
                acadDoc.SendCommand($"_NETLOAD \"{scriptPath}\" \n");
                tabSystemCommands.Enabled = true;
                btnBacaSec.Enabled = true;

                MessageBox.Show("Automation script loaded successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading automation script: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCreateMuayeneBacasi_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we're connected
                if (acadDoc == null || !isConnected)
                {
                    MessageBox.Show("Please connect to AutoCAD first.",
                        "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if(!(lblMode.Text == "Mode: MB"))
                {
                    MessageBox.Show("Incorrect mode for command, execution failed.",
                        "Mode Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Execute the custom command
                acadDoc.SendCommand("_CREATEMUAYENEBACASI \n");

                MessageBox.Show("Command executed successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCreateIzahatCemberi_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we're connected
                if (acadDoc == null || !isConnected)
                {
                    MessageBox.Show("Please connect to AutoCAD first.",
                        "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!(lblMode.Text == "Mode: KKY"))
                {
                    MessageBox.Show("Incorrect mode for command, execution failed.",
                        "Mode Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Execute the custom command
                acadDoc.SendCommand("_CREATEIZAHATCEMBERI \n");

                MessageBox.Show("Command executed successfully!",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnChangeMode_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we're connected
                if (acadDoc == null || !isConnected)
                {
                    MessageBox.Show("Please connect to AutoCAD first.",
                        "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get the selected mode
                string selectedMode = cboModes.SelectedItem.ToString();
                this.lblMode.Text = "Mode: " + selectedMode;

                // Execute the CHANGEMODE command with the selected mode
                acadDoc.SendCommand($"_CHANGEMODE {selectedMode} \n");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing mode: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAssignKapakKot_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we're connected
                if (acadDoc == null || !isConnected)
                {
                    MessageBox.Show("Please connect to AutoCAD first.",
                        "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!(lblMode.Text == "Mode: FMKNA"))
                {
                    MessageBox.Show("Incorrect mode for command, execution failed.",
                        "Mode Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Execute the custom command
                acadDoc.SendCommand("_ASSIGNKAPAKKOT \n");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAssignAkarKot_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we're connected
                if (acadDoc == null || !isConnected)
                {
                    MessageBox.Show("Please connect to AutoCAD first.",
                        "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!(lblMode.Text == "Mode: AKARKOT"))
                {
                    MessageBox.Show("Incorrect mode for command, execution failed.",
                        "Mode Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Execute the custom command
                acadDoc.SendCommand("_ASSIGNAKARKOT \n");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDrawDimensions_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we're connected
                if (acadDoc == null || !isConnected)
                {
                    MessageBox.Show("Please connect to AutoCAD first.",
                        "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!(lblMode.Text == "Mode: DRAW"))
                {
                    MessageBox.Show("Incorrect mode for command, execution failed.",
                        "Mode Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Execute the custom command
                acadDoc.SendCommand("_DRAWDIMENSIONS \n");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCalculateSystem_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if we're connected
                if (acadDoc == null || !isConnected)
                {
                    MessageBox.Show("Please connect to AutoCAD first.",
                        "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!(lblMode.Text == "Mode: CALCULATE"))
                {
                    MessageBox.Show("Incorrect mode for command, execution failed.",
                        "Mode Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Execute the custom command
                acadDoc.SendCommand("_CALCULATESYSTEM \n");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing command: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBacaSec_Click(object sender, EventArgs e)
        {
            if (acadDoc == null || !isConnected)
            {
                MessageBox.Show("Please connect to AutoCAD first.",
                    "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Show waiting message
                Label waitingLabel = new Label();
                waitingLabel.Text = "Wait for the AutoCAD setup...";
                waitingLabel.ForeColor = Color.Red;
                waitingLabel.Font = new Font(waitingLabel.Font, FontStyle.Bold);
                waitingLabel.AutoSize = true;
                waitingLabel.Location = new Point(btnBacaSec.Location.X, btnBacaSec.Location.Y + btnBacaSec.Height - 10);
                waitingLabel.Name = "waitingLabel";
                this.Controls.Add(waitingLabel);

                // Refresh UI to show the label immediately
                this.Refresh();

                // Make sure AutoCAD is active and ready
                acadApp.Visible = true;
                System.Threading.Thread.Sleep(1500);

                // Remove waiting message
                this.Controls.Remove(waitingLabel);
                waitingLabel.Dispose();

                // Use SendCommand instead of PostCommand for better synchronization
                acadDoc.SendCommand("BOYPROFIL\n");
                btnOtomasyonuCalistir.Enabled = true;
            }
            catch (Exception ex)
            {
                // Make sure to remove waiting label if error occurs
                Control waitingLabel = this.Controls.Find("waitingLabel", false).FirstOrDefault();
                if (waitingLabel != null)
                {
                    this.Controls.Remove(waitingLabel);
                    waitingLabel.Dispose();
                }

                MessageBox.Show($"Error: {ex.Message}", "Command Failed");
            }
        }

        private void btnOtomasyonuCalistir_Click(object sender, EventArgs e)
        {
            try
            {
                // Path to your automation executable
                string exePath = Path.GetFullPath(@"..\BoyProfilUIAutomation.exe");

                if (!File.Exists(exePath))
                {
                    MessageBox.Show($"Automation executable not found:\n{exePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exePath,
                    WorkingDirectory = Path.GetDirectoryName(exePath), // set correct working dir
                    UseShellExecute = false // direct process spawn without shell
                };
                // Start the process
                System.Diagnostics.Process.Start(startInfo);

                // Optionally, notify the user
                MessageBox.Show("Automation started successfully.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Disable button
                btnOtomasyonuCalistir.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to start automation.\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tabConnection_Click(object sender, EventArgs e)
        {

        }

        private void btnRevert_click(object sender, EventArgs e)
        {
            try
            {
                if (acadDoc == null || !isConnected)
                {
                    MessageBox.Show("Please connect to AutoCAD first.",
                        "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                acadDoc.SendCommand("_REVERT \n");
                MessageBox.Show("REVERT command sent to AutoCAD.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending REVERT command: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnManuel_click(object sender, EventArgs e)
        {
            this.Refresh();

            // Make sure AutoCAD is active and ready
            acadApp.Visible = true;
            System.Threading.Thread.Sleep(1500);
            acadDoc.SendCommand("_MANUELDIMENSION \n");
        }

        private void btnAnaHatAta_click(object sender, EventArgs e)
        {
            if (acadDoc == null || !isConnected)
            {
                MessageBox.Show("Please connect to AutoCAD first.", "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                // Make sure AutoCAD is active and ready
                this.Refresh();
                acadApp.Visible = true;
                System.Threading.Thread.Sleep(2000); // Allow AutoCAD to process

                // Use SendCommand instead of PostCommands for better synchronization
                acadDoc.SendCommand("_DEFINEMAINHAT \n");

                MessageBox.Show("Command sent successfully.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // Make sure to remove waiting label if error occurs
                Control waitingLabel = this.Controls.Find("waitingLabel", false).FirstOrDefault();
                if (waitingLabel != null)
                {
                    this.Controls.Remove(waitingLabel);
                    waitingLabel.Dispose();
                }

                MessageBox.Show($"Error: {ex.Message}", "Command Failed");
            }
        }
    }
}
