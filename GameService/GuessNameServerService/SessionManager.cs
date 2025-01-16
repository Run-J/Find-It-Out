// file name: SessionManager.cs
// file description: 
//      This file contains the implementation of the `SessionManager` class, which is responsible for managing 
//      all active game sessions. It provides methods to add, retrieve, update, and remove sessions, as well 
//      as retrieve a list of all active session IDs.

using System.Collections.Generic;
using System.Linq;

namespace GuessWordServerService
{
    internal class SessionManager
    {
        // attributes
        private Dictionary<string, Session> sessions = new Dictionary<string, Session>(); // store sessions


        // methods

        // Method name: GetSession
        // Parameters: string sessionID
        // Return: Session
        // Description:
        //      Retrieves a session associated with the specified session ID. 
        //      Returns the session if it exists, or `null` if no session is found with the given ID.
        internal Session GetSession(string sessionID)
        {
            if (sessions.ContainsKey(sessionID))
            {
                return sessions[sessionID];
            }
            else
            {
                return null;
            }
        }


        // Method name: AddSession
        // Parameters: Session session
        // Return: void
        // Description:
        //      Adds a new session to the session manager. 
        //      The session is stored in the `sessions` dictionary, using its unique session ID as the key.
        internal void AddSession(Session session) 
        {
            sessions[session.SessionID] = session;
        }


        // Method name: RemoveSession
        // Parameters: string sessionID
        // Return: void
        // Description:
        //      Removes the session associated with the specified session ID from the session manager.
        //      If no session exists with the given ID, the method does nothing.
        internal void RemoveSession(string sessionID)
        {
            sessions.Remove(sessionID);
        }


        // Method name: UpdateSession
        // Parameters: Session session
        // Return: void
        // Description:
        //      Updates the session data for the specified session ID. 
        //      If the session ID already exists, its data is replaced with the new session data.
        internal void UpdateSession(Session session)
        {
            sessions[session.SessionID] = session;
        }


        // Method name: GetAllSessionIDs
        // Parameters: None
        // Return: List<string>
        // Description:
        //      Retrieves a list of all active session IDs currently managed by the session manager.
        internal List<string> GetAllSessionIDs()
        {
            return sessions.Keys.ToList();
        }
    }
}
