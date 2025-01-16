// file name: Listner.cs
// file description:
//      -- This file contains the implementation of the `Server` class, which is responsible for
//      -- managing the game server, handling client connections, and coordinating game sessions.
//      -- It includes methods to start the server, handle client connections, and shut down the server gracefully.

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GuessWordServerService
{
    internal class Server
    {
        // attributes
        private TcpListener server; // game server
        private SessionManager sessionManager;
        internal bool isShuttingDown; // track shutdown status


        // constructor
        internal Server(string serverIP, Int32 port)
        {
            IPAddress iPAddress = IPAddress.Parse(serverIP);
            server = new TcpListener(iPAddress, port); // Initialize a new instance of TcpListener class that listen for incoming connection attemps
            sessionManager = new SessionManager();
            isShuttingDown = false;
        }


        // Method name: StartServer
        // Parameters: None
        // Return: void
        // Description: 
        //      Starts the game server and begins listening for incoming client connections.
        //      For each client connection, it creates a new task (thread) to handle the client's game session.
        //      The server continues to listen until it is shut down.
        internal void StartServer()
        {
            try 
            {
                // Start the server/Listening start
                server.Start();
                Logger.Log("Game server is starting...");

                // Enter the listening loop
                while (!isShuttingDown)
                {
                    Logger.Log("Game Service Waiting for a connection...");
                    // Accept a pending connect request; AcceptTcpClient() method return a TcpClient used to send/receive the data
                    TcpClient client = server.AcceptTcpClient();

                    Logger.Log("Client Connected!");

                    // Start a new Task(thread) for new game
                    Task.Run(() => HandleClient(client));

                }
            }
            catch (SocketException ex)
            {
                Logger.Log("\nSocketException (might catch this when close server, just because cut waiting accept connection):", ex);
            }
            catch (Exception ex)
            {
                Logger.Log("\n\nSomething happen in Server process", ex);
            }

        }


        // Method name: HandleClient
        // Parameters: TcpClient client
        // Return: void
        // Description:
        //      This method is responsible for handling an individual client's game session.
        //      It creates a new game instance, processes game logic,
        //      and ensures the client connection is closed after the finish game process.
        private void HandleClient(TcpClient client)
        {
            try
            {
                Game game = new Game(client, sessionManager);
                game.GameStart(); // process game logic
            }
            catch (Exception ex)
            {
                Logger.Log("\n\nSomething happen when process game logic:", ex);
            }
            
        }


        // Method name: ShutDown
        // Parameters: None
        // Return: void
        // Description: Notify all TcpClient(by sessionID) before shutdown the server. 
        internal void ShutDown()
        {
            isShuttingDown = true;

            foreach (var sessionID in sessionManager.GetAllSessionIDs())
            {
                NotifyClientShutdown(sessionID);
            }

            server.Stop(); // shut down the server
            Logger.Log("Game Server is shut down");
        }


        // Method name: NotifyClientShutdown
        // Parameters: string -- sessionID
        // Return: void
        // Description: send shutting down message to all TcpClient
        private void NotifyClientShutdown(string sessionID)
        {
            Session session = sessionManager.GetSession(sessionID);
            if (session != null)
            {
                TcpClient client = session.Client;
                if (client != null && client.Connected)
                {
                    SendShutdownMessage(client);
                }
            }
        }


        // Method name: SendShutdownMessage
        // Parameters: TcpClient client
        // Return: void
        // Description: Send shutdown message to the client
        private void SendShutdownMessage(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                string shutdownMessage = "GameMessage=Server is shutting down;GameEnd=ServerShutDown";
                byte[] data = Encoding.ASCII.GetBytes(shutdownMessage);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Logger.Log("\n\nError sending shutdown message:", ex);
            }
            finally
            {
                client.Close();
            }
        }
    }
}
