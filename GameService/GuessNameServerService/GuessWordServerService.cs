// file name: GuessWordServerService.cs
// file description: This file is for implementing service behavior.

using System;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Configuration;

namespace GuessWordServerService
{
    public partial class GuessWordServerService : ServiceBase
    {
        // attrubute
        private Server server;


        // constructor
        public GuessWordServerService()
        {
            InitializeComponent();
            CanPauseAndContinue = false; // our server don't need pause and continue
        }


        // methods

        // method name: OnStart
        // parameter: string[] args
        // return value: void
        // description: start game server.
        protected override void OnStart(string[] args)
        {
            // Log
            Logger.Log("Service is starting...");
            try
            {
                // Get our game serverIP, and port from config file
                string serverIP = ConfigurationManager.AppSettings["ServerIP"];
                int port = int.Parse(ConfigurationManager.AppSettings["ServerPort"]);

                // Start our game server
                server = new Server(serverIP, port);

                // Start server in another thread, dont block current thread
                Task.Run(() => server.StartServer());

                // Log
                Logger.Log("Service started successfully.");
            }
            catch (Exception ex)
            {
                Logger.Log("Error during service start.", ex);
            }
        }


        // method name: OnStop
        // parameter: NONE
        // return value: void
        // description: shutdown game server
        protected override void OnStop()
        {
            Logger.Log("Service is stopping...");
            try
            {
                // if our server is running, then involk shutdown method.
                if (server != null)
                {
                    server.ShutDown();
                }

                // log 
                Logger.Log("Service stopped successfully.\n\n");
            }
            catch (Exception ex)
            {
                Logger.Log("Error during service stop.", ex);
            }
        }
    }
}
