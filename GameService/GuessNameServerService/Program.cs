// Check config file before install service
// file name: Program.cs
// file description: Main for service

using System.ServiceProcess;

namespace GuessWordServerService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new GuessWordServerService() // Correct service class name
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
