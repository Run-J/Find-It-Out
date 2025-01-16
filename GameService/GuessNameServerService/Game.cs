// file name: Game.cs
// file description: 
//      -- This file contains the implementation of the `Game` class, which manages the core logic for 
//      -- individual game sessions.
//      -- It interacts with a `SessionManager` to manage game states and processes
//      -- client requests such as starting a new game, making guesses, and quitting the game.

using System.Net.Sockets;
using System.Text;


namespace GuessWordServerService
{
    internal class Game
    {
        // attributes 
        private TcpClient client; // client connection for this game session
        private SessionManager sessionManager; // shared session manager for all game sessions; all games has only one SessionManager
        private NetworkStream stream; // stream for communication with the current client

        private Session currentSession; // current session instance for this game
        private string incomingData; // store incoming data sent by client
        private string endGame; // indicating whether or not player want to quit the game; sent by client
        private string incomingSessionID; // sessionID sent by the client
        private string incomingSessionListenerID; // ListnerID sent by certain client
        private string guess; // the player guess sent by the client
        private string timeUp; // this will be informed by client


        // contructor
        internal Game(TcpClient client, SessionManager sessionManager)
        {
            this.client = client;
            this.sessionManager = sessionManager;
            stream = client.GetStream();

            incomingData = string.Empty;
            endGame = "no";
            incomingSessionID = string.Empty;
            guess = string.Empty;
            timeUp = "no";
        }


        // methods

        // Method name: GameStart
        // Parameters: None
        // Return: void 
        // Description:
        //      -- Check if this is new session;
        //      -- Get data from client -> parse data -> check if this is new session ->
        //      -- If not exist, creat a new session (session constructor will do initialization of the game properties job),
        //      -- store it, send SessionID, GameString, RemainingWords back to client
        //      -- If exist, process this incoming game request
        internal void GameStart()
        {
            // Receive and parse incomingData
            incomingData = ReceiveData();
            ParseIncomingData(incomingData);

            // Check if this session already exists
            currentSession = sessionManager.GetSession(incomingSessionID);
            if (currentSession == null) // Session not exist, means this is new client
            {
                // Create a new session and Store(Add) this new session
                Session newSession = new Session(client);

                if (incomingSessionID == "ClientListener") // if comingSession is ClientListner, then dont initialize it game datas
                {
                    sessionManager.AddSession(newSession);
                    string messageToClientForListener = $"ListenerID={newSession.SessionID}";
                    SendData(messageToClientForListener);
                    return; // and also no message back when initialize ClientListener. Only store it in sessionManager
                }
                newSession.InitGameSession();
                sessionManager.AddSession(newSession);

                // Send initial game data(80charString, remaining words, SessionID) to the client
                string message = $"SessionID={newSession.SessionID};GameString={newSession.GameString};RemainingWords={newSession.RemainingWords}";
                SendData(message);
            }
            else // Session already exist 
            {
                ProcessGame(); // Process this incoming game request
            }

        }


        // Method name: ProcessGame
        // Parameters: None
        // Return: void 
        // Description:
        //      -- Restore the session (restore game properties status),
        //      -- and check guess word, want to quit game etc., send suitable response back to client
        internal void ProcessGame()
        {
            // Check incoming data if game times up?
            if (timeUp.ToLower() == "yes") // times up
            {
                // Remove this session and session's listener
                sessionManager.RemoveSession(incomingSessionID);
                sessionManager.RemoveSession(incomingSessionListenerID);
                string completionMessage = "GameMessage=Times Up. Do you want to have a new game try again?;GameEnd=yes";
                SendData(completionMessage);
            }
            else if (endGame.ToLower() == "yes") // If user want to quit the game
            {
                // Ask player if they really want to quit game
                string confirmation = "GameMessage=Do you really want to exit current game??";
                SendData(confirmation);
            }
            else if (endGame.ToLower() == "confirmed")  // if player confirmed really want to quit game
            {
                // Delete this session and this session listener by SessionManager.RemoveSession()
                sessionManager.RemoveSession(incomingSessionID);
                sessionManager.RemoveSession(incomingSessionListenerID);
            }
            else // If user dont want to quit the game also not time'up
            {
                // Check if guessed correctly
                if (currentSession.WordList.Contains(guess))
                {
                    // Correct Guessed: Update the game state
                    currentSession.WordList.Remove(guess); // delete this word for prevent player guess same word two times;
                    currentSession.RemainingWords--; // currentSession.remainingWord - 1;

                    // Check if the game is won
                    if (currentSession.RemainingWords == 0)
                    {
                        // for there, server send EndGame=yes to client. 
                        // purpose of this is give client a signal of current game is finish
                        // so client can request new sessionID, game data if player want to play again.
                        string completionMessage =
                            "GameMessage = Correct! Congratulations! You have found the all the words! " +
                            "Do you want have a new game?;GameEnd=yes";
                        SendData(completionMessage);

                        // Remove this session and its listener
                        sessionManager.RemoveSession(incomingSessionID);
                        sessionManager.RemoveSession(incomingSessionListenerID);
                    }
                    else
                    {
                        // Send Correct Message + remainingWord
                        string successMessage = $"GameMessage=Correct!;RemainingWords={currentSession.RemainingWords}";
                        SendData(successMessage);
                    }
                    // Update the session state
                    sessionManager.UpdateSession(currentSession);
                }
                else // if guessed wrong
                {
                    // SendData(failure Message + remainingWord)
                    string failureMessage = $"GameMessage=Wrong guess!;RemainingWords={currentSession.RemainingWords}";
                    SendData(failureMessage);
                }

            }

            //client.Close(); // close current client connection
        }



        // Method name: ParseIncomingData
        // Parameters: None
        // Return: void
        // Description:
        //      -- Analyze the data sent from the client and parse out the keywords to update them to the attributes.
        internal void ParseIncomingData(string incomingData)
        {
            // split the incomingData by ";" to get each key-value pair. eg incomingData format: "SessionID=10111;EndGame=No;Guess=Apple;"
            string[] pairs = incomingData.Split(';');

            foreach (string pair in pairs)
            {
                // split each pair by '='
                string[] keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = keyValue[0].Trim();
                    string value = keyValue[1].Trim();

                    // assign values to attributes base on key
                    switch (key)
                    {
                        case "SessionID":
                            incomingSessionID = value;
                            break;
                        case "ListenerID":
                            incomingSessionListenerID = value;
                            break;
                        case "EndGame":
                            endGame = value;
                            break;
                        case "Guess":
                            guess = value;
                            break;
                        case "TimeUp":
                            timeUp = value;
                            break;
                    }
                }
                else
                {
                    string failureMessage = $"GameMessage=Incoming data has format issue key-value, check the imcoming data format";
                    SendData(failureMessage);
                }
            }
        }


        // Method name: ReceiveData
        // Parameters: None
        // Return: string
        // Description:
        //      -- This method reads data from NetWorkingStream and stores it in buffer array of type byte.
        //      -- It then converts this buffer array to type of string, and return it.
        internal string ReceiveData()
        {
            byte[] buffer = new byte[1024];
            int byteRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, byteRead);
        }


        // Method name: SendData
        // Parameters: string
        // Return: void
        // Description:
        //      -- This method sends a string message over the NetworkStream.
        //      -- The input message is converted to a byte array using ASCII encoding.
        //      -- The stream's Write method is then used to send the byte array to the connected endpoint.
        internal void SendData(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }
}
