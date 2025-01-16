namespace GuessWordServerService
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
            this.GuessWordGameServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.GuessWordGameServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // GuessWordGameServiceProcessInstaller
            // 
            this.GuessWordGameServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.GuessWordGameServiceProcessInstaller.Password = null;
            this.GuessWordGameServiceProcessInstaller.Username = null;
            this.GuessWordGameServiceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.GuessWordGameServiceProcessInstaller_AfterInstall);
            // 
            // GuessWordGameServiceInstaller
            // 
            this.GuessWordGameServiceInstaller.ServiceName = "Guess Word GameService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.GuessWordGameServiceProcessInstaller,
            this.GuessWordGameServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller GuessWordGameServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller GuessWordGameServiceInstaller;
    }
}