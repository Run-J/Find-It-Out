//*******************************************************************************************************************************/
// project name: GameClient
// project description: 
//      -- This is the client side of find out word game.
//      -- The client connects to the game server, interacts with it, and handles game-related activities.
//      -- This includes sending key game data, receiving game status from server, updates UI, and managing current client state.
// date: 11/23/2024

// file name: MainWindow.xaml.cs
// file description: 
//      -- This file contains the logic for the WPF client application.
//      -- It handles user interactions, server communication, and background tasks such as listening for server updates.
//      -- Implements features such as connecting to the server, submitting guesses, handling server messages,
//      -- and gracefully managing application shutdown.
//*******************************************************************************************************************************/


using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;

namespace GameClient
{
    public partial class MainWindow : Window
    {
        // player interaction
        private TcpClient Client { get; set; }
        private NetworkStream Stream { get; set; }
        private string IpAddress { get; set; }
        private int Port { get; set; }
        private string PlayerName { get; set; }
        private int RemainingTime { get; set; }
        private DispatcherTimer timer;


        // server part
        private string SessionID { get; set; } = "0";
        private string GameString { get; set; }
        private string RemainingWords { get; set; }
        private string GameMessage { get; set; }
        private string GameEnd { get; set; }


        // listner part
        private string ListenerID { get; set; } 
        private TcpClient ListenerClient { get; set; }
        private NetworkStream ListenerStream { get; set; }
        private bool ListenerActive { get; set; } = false;



        public MainWindow()
        {
            InitializeComponent();
        }


        // Method name: ConnectButton_Click
        // Parameters: object sender, RoutedEventArgs e
        // Return: void
        // Description:
        //      -- Handles the connect button click event.
        //      -- Validates inputs, establishes a connection with the server,
        //      -- initializes the game state, and starts the server listener.
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SessionID == "0")
                {
                    // Check if all inputs are provided
                    if (string.IsNullOrWhiteSpace(IpAddressTextBox.Text))
                        throw new ArgumentException("IP Address cannot be empty.");

                    if (string.IsNullOrWhiteSpace(PortTextBox.Text))
                        throw new ArgumentException("Port cannot be empty.");

                    if (string.IsNullOrWhiteSpace(PlayerNameTextBox.Text))
                        throw new ArgumentException("Player name cannot be empty.");

                    if (string.IsNullOrWhiteSpace(GameTimeTextBox.Text))
                        throw new ArgumentException("Game time cannot be empty.");

                    // Retrieving the game info
                    IpAddress = IpAddressTextBox.Text;
                    Port = int.Parse(PortTextBox.Text);
                    PlayerName = PlayerNameTextBox.Text;

                    // InitializeTimer
                    if (int.TryParse(GameTimeTextBox.Text, out int remainingTime) && remainingTime < 0)
                        throw new ArgumentException("Timer must be integer and greather than 0");
                    RemainingTime = remainingTime;

                    // Initialize TCP client
                    ConnectToServer();

                    // Send player details
                    string message = $"SessionID={SessionID}";
                    SendData(message);

                    // Receive initial game data
                    string serverResponse = ReceiveData();
                    ParseServerData(serverResponse);


                    // Update UI
                    GameStringTextBlock.Text = $"Game String: {GameString}";
                    RemainingWordsTextBlock.Text = $"Remaining Words: {RemainingWords}";
                    ServerMessageTextBlock.Text = $"Game Messages: {PlayerName}, make your guess!";
                    InitializeTimer(); // InitializeTimer after connect server successfully

                    MessageBox.Show("Connected Successfully!");

                    // Start Server Listener
                    await StartListenerAsync(); // non-blocking current thread
                }
                CloseClient();
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show($"Input Error: {argEx.Message}", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (FormatException formatEx)
            {
                MessageBox.Show($"Invalid Format: {formatEx.Message}", "Format Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (SocketException socketEx)
            {
                MessageBox.Show($"Network Error: Could not connect to the server. Details: {socketEx.Message}", "Network Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException invalidOpEx)
            {
                MessageBox.Show($"Invalid Operation: {invalidOpEx.Message}", "Invalid Operation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Method name: InitializeTimer
        // Parameters: None
        // Return: void
        // Description: Initializes a DispatcherTimer to handle game countdown and updates the remaining time.
        private void InitializeTimer()
        {
            // Initialize the timer
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += TimerTick;
            timer.Start();
        }


        // Method name: TimerTick
        // Parameters: object sender, EventArgs e
        // Return: void
        // Description:
        //      -- Handles each tick of the timer.
        //      -- Decrements the remaining time, checks for timeout, and notifies the server if time is up.
        private void TimerTick(object sender, EventArgs e)
        {
            RemainingTime--;

            if (RemainingTime > 0)
            {
                TimeLeftBlock.Text = $"{RemainingTime}";
            }
            else
            {
                timer.Stop();
                TimeLeftBlock.Text = "Time's up! Game Over.";

                NotifyServerTimeUp();

                // receive server response
                string message = ReceiveData();
                ParseServerData(message);

                CloseClient();

                if (GameEnd == "yes")
                {
                    RestartGame();
                }
            }
        }


        // Method name: NotifyServerTimeUp
        // Parameters: None
        // Return: void
        // Description: Notifies the server when the game timer runs out.
        private void NotifyServerTimeUp()
        {
            try
            {
                ConnectToServer();
                string message = $"SessionID={SessionID};ListenerID={ListenerID};TimeUp=Yes";
                SendData(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to notify the server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Method name: StartListenerAsync
        // Parameters: None
        // Return: Task
        // Description: Establishes a special listener connection with the server and starts monitoring messages.
        private async Task StartListenerAsync()
        {
            try
            {
                ListenerClient = new TcpClient();
                ListenerClient.Connect(IpAddress, Port);
                ListenerStream = ListenerClient.GetStream();
                string message = $"SessionID=ClientListener"; // tall the server this is ClientListener
                SendListenerData(message);
                ListenerActive = true;

                // Run the monitoring loop
                await ListenForMessages(); // non-block UI thread and listen in background
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        $"Error in listener: {ex.Message}",
                        "Listener Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });
                StopListener();
            }
        }


        // Method name: ListenForMessages
        // Parameters: None
        // Return: Task
        // Description: Continuously listens for messages from the server and processes shutdown messages if received.
        private async Task ListenForMessages()
        {
            try
            {
                while (ListenerActive && ListenerClient.Connected)
                {
                    string serverResponse = await ReceiveListenerDataAsync();
                    if (!string.IsNullOrEmpty(serverResponse))
                    {
                        ParseServerData(serverResponse);

                        // Check for shutdown message
                        if (GameEnd == "ServerShutDown")
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (timer != null)
                                {
                                    timer.Stop();  // Pause the timer
                                }

                                MessageBox.Show(
                                    "The server is shutting down. The game will close.",
                                    "Server Shutdown",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                Application.Current.Shutdown();
                            });
                            StopListener();
                            break;
                        }
                    }

                    // Small delay to avoid busy waiting
                    await Task.Delay(100);
                }
            }
            catch (Exception) // dont care this exception now, bcoz this exception happen when server delete this listener
            {                   // dont want to print out or show message box about this exception.
                                // this can be written in client log file as a log entry, but it is future thing.
                StopListener();
            }
        }


        // Method name: SendListenerData
        // Parameters: string message
        // Return: void
        // Description: Sends a message to the server using the listener connection(ListenerStream) instead of client stream.
        private void SendListenerData(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            ListenerStream.Write(data, 0, data.Length);
        }


        // Method name: ReceiveListenerDataAsync
        // Parameters: None
        // Return: Task<string> -- Received message as a string
        // Description: Receives data asynchronously from the server using the listener connection.
        private async Task<string> ReceiveListenerDataAsync()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await ListenerStream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0)
            {
                throw new IOException("Server closed the listener connection.");
            }

            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }


        // Method name: StopListener
        // Parameters: None
        // Return: void
        // Description: Stops the listener by closing the listener stream and client, and cleaning up resources.
        private void StopListener()
        {
            ListenerActive = false;

            if (ListenerStream != null)
            {
                ListenerStream.Close();
                ListenerStream = null;
            }

            if (ListenerClient != null)
            {
                ListenerClient.Close();
                ListenerClient = null;
            }
        }


        // Method name: IsClientConnected
        // Parameters: None
        // Return: bool -- True if the client is connected, false otherwise
        // Description: Checks whether the client connection is active by polling the socket status.
        private bool IsClientConnected()
        {
            try
            {
                if (Client == null || !Client.Connected)
                    return false;

                // socket check
                if (Client.Client.Poll(0, SelectMode.SelectRead) && Client.Client.Available == 0)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }


        // Method name: ConnectToServer
        // Parameters: None
        // Return: void
        // Description: Establishes a connection to the server using a TcpClient and initializes the NetworkStream.
        private void ConnectToServer()
        {
            // Close and dispose of the current Client, if any
            CloseClient();

            // Create a new TCP Client and establish connection
            Client = new TcpClient();

            if (IpAddress == null || Port == 0)
            {
                throw new ArgumentException("IP address and Port are required.");
            }

            try
            {
                Client.Connect(IpAddress, Port);
                Stream = Client.GetStream();
            }
            catch
            {
                throw;
            }
        }


        // Method name: CloseClient
        // Parameters: None
        // Return: void
        // Description: Closes the client connection and releases resources.
        private void CloseClient()
        {
            if (IsClientConnected())
            {
                // close the Stream if it's open
                if (Stream != null)
                {
                    Stream.Close();
                    Stream = null;
                }
                // Close the Client connection
                Client.Close();
                Client = null;
            }
        }


        // Method name: SendGuessButton_Click
        // Parameters: object sender, RoutedEventArgs e
        // Return: void
        // Description:
        //      -- Handles the guess submission button click event. Sends the player's guess to the server and processes the response.
        //      -- Send Button, click -> connect server -> get feedback from server -> update UI
        private void SendGuessButton_Click(object sender, RoutedEventArgs e)
        {
            try 
            {
                ConnectToServer();
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show($"Connection Error: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Unable to connect to the server: {ex.Message}\nPlease try again later.",
                "Connection Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
                return;
            }
          
            string guess = GuessTextBox.Text;
            string message = $"SessionID={SessionID};EndGame=No;Guess={guess}";
            SendData(message);
            GuessTextBox.Text = "";

            string serverResponse = ReceiveData();
            ParseServerData(serverResponse);
            ServerMessageTextBlock.Text = $"Game Messages: {GameMessage}";
            RemainingWordsTextBlock.Text = $"Remaining Words: {RemainingWords}";

            CloseClient();

            if (GameEnd == "yes")
            {
                timer.Stop();
                RestartGame();
            }
        }

        // Method name: RestartGame
        // Parameters: None
        // Return: void
        // Description: Restarts the game by resetting the state and reconnecting to the server, or exits the application if declined.
        private void RestartGame()
        {
            var result = MessageBox.Show(
                GameMessage,
                "Game Over",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // reset the game state and reconnect
                SessionID = "0"; // clear session ID for a new game
                GameEnd = "no";
                ConnectButton_Click(null, null); // Reconnect and start a new game
            }
            else if (result == MessageBoxResult.No)
            {
                // exit the application
                Application.Current.Shutdown();
            }
        }

        // Method name: QuitGameButton_Click
        // Parameters: object sender, RoutedEventArgs e
        // Return: void
        // Description: Handles the quit button click event. Notifies the server, stops the game, and closes the application.
        private void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (SessionID != "0")
            {
                try
                {
                    ConnectToServer();
                }
                catch (ArgumentException ex)
                {
                    MessageBox.Show($"Connection Error: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                catch (SocketException ex)
                {
                    MessageBox.Show($"Unable to connect to the server: {ex.Message}\nPlease try again later.",
                    "Connection Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                    var confirmExitServerDown = MessageBox.Show(
                    "Do you want to exit?",
                    "Confirm Quit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                    if (confirmExitServerDown == MessageBoxResult.Yes)
                    {
                        MessageBox.Show("You have quit the game.");
                        CloseClient();
                        Application.Current.Shutdown();
                    }
                    return;
                }

                string message = $"SessionID={SessionID};EndGame=Yes";
                SendData(message); // tell the server user want to quit

                string serverResponse = ReceiveData();
                ParseServerData(serverResponse);
                ServerMessageTextBlock.Text = $"Game Messages: {GameMessage}"; // get server message back
                
                var result = MessageBox.Show(   // shows user confirmation quit message sent from server 
                    GameMessage,
                    "Confirm Quit",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) // if user chose yes (confirm quit)
                {
                    timer.Stop();
                    string quitMessage = $"SessionID={SessionID};ListenerID={ListenerID};EndGame=Yes";
                    SendData(message);
                    MessageBox.Show("You have quit the game.");
                    CloseClient();
                    Application.Current.Shutdown();
                }
            }
            else
            {
                CloseClient();
                Application.Current.Shutdown();
            }
        }


        // Method name: SendData
        // Parameters: string message
        // Return: void
        // Description: Translate string to byte; Sends a message to the server using the main client connection.
        private void SendData(string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            Stream.Write(data, 0, data.Length);
        }


        // Method name: ReceiveData
        // Parameters: None
        // Return: string -- Received message as a string
        // Description: Receives data from the server using the main client connection; Translate byte to string
        private string ReceiveData()
        {
            byte[] buffer = new byte[1024];
            int bytesRead = Stream.Read(buffer, 0, buffer.Length);
            return Encoding.ASCII.GetString(buffer, 0, bytesRead);
        }


        // Method name: ParseServerData
        // Parameters: string data
        // Return: void
        // Description: Parses the server response string into key-value pairs and updates game state attributes accordingly.
        private void ParseServerData(string data)
        {
            // split the data by ";" to get each key-value pair. eg data format: "SessionID=10111;EndGame=No;Guess=Apple;"
            string[] pairs = data.Split(';');

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
                            SessionID = value;
                            break;
                        case "ListenerID":
                            ListenerID = value;
                            break;
                        case "GameString":
                            GameString = value;
                            break;
                        case "RemainingWords":
                            RemainingWords = value;
                            break;
                        case "GameMessage":
                            GameMessage = value;
                            break;
                        case "GameEnd":
                            GameEnd = value;
                            break;
                    }
                }
            }
        }
    }
}
