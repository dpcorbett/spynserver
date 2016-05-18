namespace WSVSpyn
{
    partial class ProjectInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.spiSpynServer = new System.ServiceProcess.ServiceProcessInstaller();
            this.sinSpynServer = new System.ServiceProcess.ServiceInstaller();
            // 
            // spiSpynServer
            // 
            this.spiSpynServer.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.spiSpynServer.Password = null;
            this.spiSpynServer.Username = null;
            // 
            // sinSpynServer
            // 
            this.sinSpynServer.ServiceName = "SpynServer";
            this.sinSpynServer.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.spiSpynServer,
            this.sinSpynServer});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller spiSpynServer;
        private System.ServiceProcess.ServiceInstaller sinSpynServer;
    }
}