namespace FlowCAD_desktop
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnConnect = new System.Windows.Forms.Button();
            this.statusLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnOpenDrawing = new System.Windows.Forms.Button();
            this.btnStartAutomation = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabConnection = new System.Windows.Forms.TabPage();
            this.tabSystemCommands = new System.Windows.Forms.TabPage();
            this.btnManuelDimension = new System.Windows.Forms.Button();
            this.btnRevert = new System.Windows.Forms.Button();
            this.btnCreateMuayeneBacasi = new System.Windows.Forms.Button();
            this.btnCreateIzahatCemberi = new System.Windows.Forms.Button();
            this.btnChangeMode = new System.Windows.Forms.Button();
            this.btnAssignKapakKot = new System.Windows.Forms.Button();
            this.btnAssignAkarKot = new System.Windows.Forms.Button();
            this.btnDrawDimension = new System.Windows.Forms.Button();
            this.btnDoCalculation = new System.Windows.Forms.Button();
            this.lblMode = new System.Windows.Forms.Label();
            this.cboModes = new System.Windows.Forms.ComboBox();
            this.boyProfilTab = new System.Windows.Forms.TabPage();
            this.lblBoyProfilInfo = new System.Windows.Forms.Label();
            this.btnBacaSec = new System.Windows.Forms.Button();
            this.btnOtomasyonuCalistir = new System.Windows.Forms.Button();
            this.tabControl.SuspendLayout();
            this.tabConnection.SuspendLayout();
            this.tabSystemCommands.SuspendLayout();
            this.boyProfilTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(6, 78);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(144, 37);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect to AutoCAD";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(26, 218);
            this.statusLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(137, 13);
            this.statusLabel.TabIndex = 2;
            this.statusLabel.Text = "Not connected to AutoCAD";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(25, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "AutoCAD Connect";
            // 
            // btnOpenDrawing
            // 
            this.btnOpenDrawing.Location = new System.Drawing.Point(156, 78);
            this.btnOpenDrawing.Name = "btnOpenDrawing";
            this.btnOpenDrawing.Size = new System.Drawing.Size(144, 37);
            this.btnOpenDrawing.TabIndex = 4;
            this.btnOpenDrawing.Text = "Open Drawing";
            this.btnOpenDrawing.UseVisualStyleBackColor = true;
            this.btnOpenDrawing.Click += new System.EventHandler(this.btnOpenDrawing_Click);
            // 
            // btnStartAutomation
            // 
            this.btnStartAutomation.Enabled = false;
            this.btnStartAutomation.Location = new System.Drawing.Point(6, 151);
            this.btnStartAutomation.Name = "btnStartAutomation";
            this.btnStartAutomation.Size = new System.Drawing.Size(294, 37);
            this.btnStartAutomation.TabIndex = 5;
            this.btnStartAutomation.Text = "Start Automation";
            this.btnStartAutomation.UseVisualStyleBackColor = true;
            this.btnStartAutomation.Click += new System.EventHandler(this.btnStartAutomation_Click);
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabConnection);
            this.tabControl.Controls.Add(this.tabSystemCommands);
            this.tabControl.Controls.Add(this.boyProfilTab);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(333, 436);
            this.tabControl.TabIndex = 6;
            // 
            // tabConnection
            // 
            this.tabConnection.Controls.Add(this.statusLabel);
            this.tabConnection.Controls.Add(this.label1);
            this.tabConnection.Controls.Add(this.btnConnect);
            this.tabConnection.Controls.Add(this.btnOpenDrawing);
            this.tabConnection.Controls.Add(this.btnStartAutomation);
            this.tabConnection.Location = new System.Drawing.Point(4, 22);
            this.tabConnection.Name = "tabConnection";
            this.tabConnection.Padding = new System.Windows.Forms.Padding(3);
            this.tabConnection.Size = new System.Drawing.Size(325, 410);
            this.tabConnection.TabIndex = 0;
            this.tabConnection.Text = "Connection";
            this.tabConnection.UseVisualStyleBackColor = true;
            this.tabConnection.Click += new System.EventHandler(this.tabConnection_Click);
            // 
            // tabSystemCommands
            // 
            this.tabSystemCommands.Controls.Add(this.btnManuelDimension);
            this.tabSystemCommands.Controls.Add(this.btnRevert);
            this.tabSystemCommands.Controls.Add(this.btnCreateMuayeneBacasi);
            this.tabSystemCommands.Controls.Add(this.btnCreateIzahatCemberi);
            this.tabSystemCommands.Controls.Add(this.btnChangeMode);
            this.tabSystemCommands.Controls.Add(this.btnAssignKapakKot);
            this.tabSystemCommands.Controls.Add(this.btnAssignAkarKot);
            this.tabSystemCommands.Controls.Add(this.btnDrawDimension);
            this.tabSystemCommands.Controls.Add(this.btnDoCalculation);
            this.tabSystemCommands.Controls.Add(this.lblMode);
            this.tabSystemCommands.Controls.Add(this.cboModes);
            this.tabSystemCommands.Enabled = false;
            this.tabSystemCommands.Location = new System.Drawing.Point(4, 22);
            this.tabSystemCommands.Name = "tabSystemCommands";
            this.tabSystemCommands.Padding = new System.Windows.Forms.Padding(3);
            this.tabSystemCommands.Size = new System.Drawing.Size(325, 410);
            this.tabSystemCommands.TabIndex = 1;
            this.tabSystemCommands.Text = "Sistem Oluştur";
            this.tabSystemCommands.UseVisualStyleBackColor = true;
            // 
            // btnManuelDimension
            // 
            this.btnManuelDimension.BackColor = System.Drawing.Color.Transparent;
            this.btnManuelDimension.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnManuelDimension.Location = new System.Drawing.Point(108, 177);
            this.btnManuelDimension.Name = "btnManuelDimension";
            this.btnManuelDimension.Size = new System.Drawing.Size(109, 38);
            this.btnManuelDimension.TabIndex = 5;
            this.btnManuelDimension.Text = "Manuel dimension";
            this.btnManuelDimension.UseVisualStyleBackColor = false;
            this.btnManuelDimension.Click += new System.EventHandler(this.btnManuel_click);
            // 
            // btnRevert
            // 
            this.btnRevert.Location = new System.Drawing.Point(6, 265);
            this.btnRevert.Name = "btnRevert";
            this.btnRevert.Size = new System.Drawing.Size(211, 38);
            this.btnRevert.TabIndex = 4;
            this.btnRevert.Text = "Hesaplamaları Geri Al";
            this.btnRevert.UseVisualStyleBackColor = true;
            this.btnRevert.Click += new System.EventHandler(this.btnRevert_click);
            // 
            // btnCreateMuayeneBacasi
            // 
            this.btnCreateMuayeneBacasi.Location = new System.Drawing.Point(6, 6);
            this.btnCreateMuayeneBacasi.Name = "btnCreateMuayeneBacasi";
            this.btnCreateMuayeneBacasi.Size = new System.Drawing.Size(211, 37);
            this.btnCreateMuayeneBacasi.TabIndex = 0;
            this.btnCreateMuayeneBacasi.Text = "Muayene Bacası Yarat";
            this.btnCreateMuayeneBacasi.UseVisualStyleBackColor = true;
            this.btnCreateMuayeneBacasi.Click += new System.EventHandler(this.btnCreateMuayeneBacasi_Click);
            // 
            // btnCreateIzahatCemberi
            // 
            this.btnCreateIzahatCemberi.Location = new System.Drawing.Point(6, 49);
            this.btnCreateIzahatCemberi.Name = "btnCreateIzahatCemberi";
            this.btnCreateIzahatCemberi.Size = new System.Drawing.Size(211, 37);
            this.btnCreateIzahatCemberi.TabIndex = 0;
            this.btnCreateIzahatCemberi.Text = "Izahat Cemberi Yarat";
            this.btnCreateIzahatCemberi.UseVisualStyleBackColor = true;
            this.btnCreateIzahatCemberi.Click += new System.EventHandler(this.btnCreateIzahatCemberi_Click);
            // 
            // btnChangeMode
            // 
            this.btnChangeMode.Location = new System.Drawing.Point(6, 326);
            this.btnChangeMode.Name = "btnChangeMode";
            this.btnChangeMode.Size = new System.Drawing.Size(109, 37);
            this.btnChangeMode.TabIndex = 0;
            this.btnChangeMode.Text = "Mod değiştir";
            this.btnChangeMode.UseVisualStyleBackColor = true;
            this.btnChangeMode.Click += new System.EventHandler(this.btnChangeMode_Click);
            // 
            // btnAssignKapakKot
            // 
            this.btnAssignKapakKot.Location = new System.Drawing.Point(6, 92);
            this.btnAssignKapakKot.Name = "btnAssignKapakKot";
            this.btnAssignKapakKot.Size = new System.Drawing.Size(211, 37);
            this.btnAssignKapakKot.TabIndex = 0;
            this.btnAssignKapakKot.Text = "Kapak Kot Ata";
            this.btnAssignKapakKot.UseVisualStyleBackColor = true;
            this.btnAssignKapakKot.Click += new System.EventHandler(this.btnAssignKapakKot_Click);
            // 
            // btnAssignAkarKot
            // 
            this.btnAssignAkarKot.Location = new System.Drawing.Point(3, 135);
            this.btnAssignAkarKot.Name = "btnAssignAkarKot";
            this.btnAssignAkarKot.Size = new System.Drawing.Size(214, 37);
            this.btnAssignAkarKot.TabIndex = 0;
            this.btnAssignAkarKot.Text = "Akar Kot Ata";
            this.btnAssignAkarKot.UseVisualStyleBackColor = true;
            this.btnAssignAkarKot.Click += new System.EventHandler(this.btnAssignAkarKot_Click);
            // 
            // btnDrawDimension
            // 
            this.btnDrawDimension.Location = new System.Drawing.Point(6, 178);
            this.btnDrawDimension.Name = "btnDrawDimension";
            this.btnDrawDimension.Size = new System.Drawing.Size(96, 37);
            this.btnDrawDimension.TabIndex = 0;
            this.btnDrawDimension.Text = "Dimension çiz";
            this.btnDrawDimension.UseVisualStyleBackColor = true;
            this.btnDrawDimension.Click += new System.EventHandler(this.btnDrawDimensions_Click);
            // 
            // btnDoCalculation
            // 
            this.btnDoCalculation.Location = new System.Drawing.Point(6, 221);
            this.btnDoCalculation.Name = "btnDoCalculation";
            this.btnDoCalculation.Size = new System.Drawing.Size(211, 38);
            this.btnDoCalculation.TabIndex = 0;
            this.btnDoCalculation.Text = "Sistemi Hesapla";
            this.btnDoCalculation.UseVisualStyleBackColor = true;
            this.btnDoCalculation.Click += new System.EventHandler(this.btnCalculateSystem_Click);
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Location = new System.Drawing.Point(3, 377);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(56, 13);
            this.lblMode.TabIndex = 1;
            this.lblMode.Text = "Mode: MB";
            // 
            // cboModes
            // 
            this.cboModes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboModes.FormattingEnabled = true;
            this.cboModes.Items.AddRange(new object[] {
            "MB",
            "KKY",
            "FMKNA",
            "AKARKOT",
            "DRAW",
            "CALCULATE",
            "REVERT"});
            this.cboModes.Location = new System.Drawing.Point(131, 335);
            this.cboModes.Name = "cboModes";
            this.cboModes.Size = new System.Drawing.Size(75, 21);
            this.cboModes.TabIndex = 3;
            // 
            // boyProfilTab
            // 
            this.boyProfilTab.Controls.Add(this.lblBoyProfilInfo);
            this.boyProfilTab.Controls.Add(this.btnBacaSec);
            this.boyProfilTab.Controls.Add(this.btnOtomasyonuCalistir);
            this.boyProfilTab.Location = new System.Drawing.Point(4, 22);
            this.boyProfilTab.Name = "boyProfilTab";
            this.boyProfilTab.Padding = new System.Windows.Forms.Padding(3);
            this.boyProfilTab.Size = new System.Drawing.Size(325, 410);
            this.boyProfilTab.TabIndex = 2;
            this.boyProfilTab.Text = "BoyProfil";
            this.boyProfilTab.UseVisualStyleBackColor = true;
            // 
            // lblBoyProfilInfo
            // 
            this.lblBoyProfilInfo.AutoSize = true;
            this.lblBoyProfilInfo.Location = new System.Drawing.Point(6, 14);
            this.lblBoyProfilInfo.Name = "lblBoyProfilInfo";
            this.lblBoyProfilInfo.Size = new System.Drawing.Size(299, 13);
            this.lblBoyProfilInfo.TabIndex = 0;
            this.lblBoyProfilInfo.Text = "Boy profil çıkarmak istediğiniz hattan bir muayene bacası seçin";
            // 
            // btnBacaSec
            // 
            this.btnBacaSec.Enabled = false;
            this.btnBacaSec.Location = new System.Drawing.Point(9, 49);
            this.btnBacaSec.Name = "btnBacaSec";
            this.btnBacaSec.Size = new System.Drawing.Size(120, 30);
            this.btnBacaSec.TabIndex = 1;
            this.btnBacaSec.Text = "baca seç";
            this.btnBacaSec.UseVisualStyleBackColor = true;
            this.btnBacaSec.Click += new System.EventHandler(this.btnBacaSec_Click);
            // 
            // btnOtomasyonuCalistir
            // 
            this.btnOtomasyonuCalistir.Enabled = false;
            this.btnOtomasyonuCalistir.Location = new System.Drawing.Point(165, 49);
            this.btnOtomasyonuCalistir.Name = "btnOtomasyonuCalistir";
            this.btnOtomasyonuCalistir.Size = new System.Drawing.Size(140, 30);
            this.btnOtomasyonuCalistir.TabIndex = 2;
            this.btnOtomasyonuCalistir.Text = "otomasyonu çalıştır";
            this.btnOtomasyonuCalistir.UseVisualStyleBackColor = true;
            this.btnOtomasyonuCalistir.Click += new System.EventHandler(this.btnOtomasyonuCalistir_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(376, 460);
            this.Controls.Add(this.tabControl);
            this.Name = "MainForm";
            this.Text = "FlowCAD";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl.ResumeLayout(false);
            this.tabConnection.ResumeLayout(false);
            this.tabConnection.PerformLayout();
            this.tabSystemCommands.ResumeLayout(false);
            this.tabSystemCommands.PerformLayout();
            this.boyProfilTab.ResumeLayout(false);
            this.boyProfilTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnOpenDrawing;
        private System.Windows.Forms.Button btnStartAutomation;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabConnection;
        private System.Windows.Forms.TabPage tabSystemCommands;
        private System.Windows.Forms.Button btnCreateMuayeneBacasi;
        private System.Windows.Forms.Button btnCreateIzahatCemberi;
        private System.Windows.Forms.Button btnChangeMode;
        private System.Windows.Forms.Button btnAssignKapakKot;
        private System.Windows.Forms.Button btnAssignAkarKot;
        private System.Windows.Forms.Button btnDrawDimension;
        private System.Windows.Forms.Button btnDoCalculation;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.ComboBox cboModes;
        private System.Windows.Forms.TabPage boyProfilTab;
        private System.Windows.Forms.Label lblBoyProfilInfo;
        private System.Windows.Forms.Button btnBacaSec;
        private System.Windows.Forms.Button btnOtomasyonuCalistir;
        private System.Windows.Forms.Button btnRevert;
        private System.Windows.Forms.Button btnManuelDimension;
    }
}