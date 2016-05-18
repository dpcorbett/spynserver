namespace WFASpyn
{
    partial class frmMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.button3 = new System.Windows.Forms.Button();
            this.txtTestRequest = new System.Windows.Forms.TextBox();
            this.btnTestRequest = new System.Windows.Forms.Button();
            this.txtMaster = new System.Windows.Forms.TextBox();
            this.txtSatellite = new System.Windows.Forms.TextBox();
            this.btnSync = new System.Windows.Forms.Button();
            this.fbdRoot = new System.Windows.Forms.FolderBrowserDialog();
            this.btnNew = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(582, 250);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(78, 81);
            this.button3.TabIndex = 2;
            this.button3.Text = "Debug Server";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // txtTestRequest
            // 
            this.txtTestRequest.Location = new System.Drawing.Point(44, 28);
            this.txtTestRequest.Name = "txtTestRequest";
            this.txtTestRequest.Size = new System.Drawing.Size(100, 20);
            this.txtTestRequest.TabIndex = 3;
            // 
            // btnTestRequest
            // 
            this.btnTestRequest.Location = new System.Drawing.Point(44, 68);
            this.btnTestRequest.Name = "btnTestRequest";
            this.btnTestRequest.Size = new System.Drawing.Size(100, 68);
            this.btnTestRequest.TabIndex = 4;
            this.btnTestRequest.Text = "Test Request";
            this.btnTestRequest.UseVisualStyleBackColor = true;
            this.btnTestRequest.Click += new System.EventHandler(this.btnTestRequest_Click);
            // 
            // txtMaster
            // 
            this.txtMaster.Location = new System.Drawing.Point(84, 210);
            this.txtMaster.Name = "txtMaster";
            this.txtMaster.Size = new System.Drawing.Size(313, 20);
            this.txtMaster.TabIndex = 5;
            this.txtMaster.Click += new System.EventHandler(this.txtMaster_Click);
            // 
            // txtSatellite
            // 
            this.txtSatellite.Location = new System.Drawing.Point(84, 250);
            this.txtSatellite.Name = "txtSatellite";
            this.txtSatellite.Size = new System.Drawing.Size(313, 20);
            this.txtSatellite.TabIndex = 6;
            this.txtSatellite.Click += new System.EventHandler(this.txtDest_Click);
            // 
            // btnSync
            // 
            this.btnSync.Location = new System.Drawing.Point(208, 279);
            this.btnSync.Name = "btnSync";
            this.btnSync.Size = new System.Drawing.Size(75, 23);
            this.btnSync.TabIndex = 7;
            this.btnSync.Text = "Sync";
            this.btnSync.UseVisualStyleBackColor = true;
            this.btnSync.Click += new System.EventHandler(this.btnSync_Click);
            // 
            // btnNew
            // 
            this.btnNew.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNew.Location = new System.Drawing.Point(237, 28);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(266, 147);
            this.btnNew.TabIndex = 8;
            this.btnNew.Text = "WPF";
            this.btnNew.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 374);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.btnSync);
            this.Controls.Add(this.txtSatellite);
            this.Controls.Add(this.txtMaster);
            this.Controls.Add(this.btnTestRequest);
            this.Controls.Add(this.txtTestRequest);
            this.Controls.Add(this.button3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.Text = "SPYN";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.TextBox txtTestRequest;
        private System.Windows.Forms.Button btnTestRequest;
        private System.Windows.Forms.TextBox txtMaster;
        private System.Windows.Forms.TextBox txtSatellite;
        private System.Windows.Forms.Button btnSync;
        private System.Windows.Forms.FolderBrowserDialog fbdRoot;
        private System.Windows.Forms.Button btnNew;
    }
}

