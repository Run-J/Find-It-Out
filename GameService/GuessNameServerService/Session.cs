// file name: Session.cs
// file description: 
//      This file contains the implementation of the `Session` class, which manages the state of an individual game session.
//      A session is associated with a unique ID and stores game-related data, such as the 80-character string, the list of 
//      words to guess, and the client connection.

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;

namespace GuessWordServerService
{
    internal class Session
    {
        // attributes
        private string gameString; // character string for hiding answer words
        private int remainingWords; // number of words that exist in the character string
        private List<string> wordList; // answer words list
        private string sessionID;
        TcpClient client;
        private static string testsFolderPath = ConfigurationManager.AppSettings["TestsFolderPath"];


        private static string[] fileNames =
       {
            Path.Combine(testsFolderPath, "test1.txt"),
            Path.Combine(testsFolderPath, "test2.txt"),
            Path.Combine(testsFolderPath, "test3.txt"),
            Path.Combine(testsFolderPath, "test4.txt"),
            Path.Combine(testsFolderPath, "test5.txt")
        };


        // properties
        #region properties
        internal string GameString 
        { 
            get
            {
                return gameString;
            }
            set
            {
                gameString = value;
            }
        }

        internal int RemainingWords
        {
            get
            {
                return remainingWords;
            }
            set
            {
                remainingWords = value;
            }
        }

        internal List<string> WordList
        {
            get
            {
                return wordList;
            }
            set
            {
                wordList = value;
            }
        }

        internal string SessionID
        {
            get
            { 
                return sessionID; 
            }
        }

        internal TcpClient Client
        {
            get 
            { 
                return client; 
            }
        }
        #endregion


        // contructor
        // Description:
        //      Initializes a new game session by assigning a unique session ID, loading game data from a file, 
        //      and associating the session with a client connection.
        internal Session(TcpClient client)
        {
            sessionID = Guid.NewGuid().ToString();
            this.client = client;
        }


        // methods

        // Method name: PickRandomFile
        // Parameters: string[] fileNames -- a string array including all of .txt filename
        // Return: string
        // Description:
        //      Selects a random file name from the provided array of file names.
        private static string PickRandomFile(string[] fileNames)
        {
            Random random = new Random();
            int index = random.Next(fileNames.Length);
            string filePath = fileNames[index];
            return filePath;
        }


        // Method name: InitGameSession
        // Parameters: None
        // Return: void
        // Description:
        //      Initializes the game session by loading data from a randomly selected file.
        //      The file must follow a specific format:
        //          - Line 1: 80-character string.
        //          - Line 2: Number of words.
        //          - Subsequent lines: List of valid words.
        //      Validates the file format and populates the game properties.
        internal void InitGameSession()
        {
            string selectedFile = PickRandomFile(fileNames);

            /*string selectedFile = "tests/test6.txt";*/ // uncommont can make game only have two words; use for test win situation

            string[] lines = File.ReadAllLines(selectedFile);
            if (lines.Length < 2)
            {
                throw new InvalidDataException("File format is invalid: no 80 char string or number of words");
            }
            gameString = lines[0]; // First line: 80-character string
            remainingWords = int.Parse(lines[1]); // Second line: number of words
            wordList = new List<string>(lines.Skip(2)); // Remaining lines: word list

            // Validate the data
            if (gameString.Length != 80)
            {
                throw new InvalidDataException("File format is invalid: first line must be 80 characters.");
            }
            if (wordList.Count != remainingWords)
            {
                throw new InvalidDataException("File format is invalid: word count does not match.");
            }
        }
    }
}
