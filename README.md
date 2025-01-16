# Find It Out

## Overview
**Find It Out** is a multi-client game where the server runs as a Windows service, and clients connect to it using the server's IP address and the appropriate port.

- **Objective**: Players uncover hidden secrets within a text.
- **Winning Condition**: Discover all secrets before the timer expires.
- Players can choose to retry the game or quit when the timer runs out.
- Done in partnership with Kirill Bukhteev.

---

## Server-Client Design

The **Find It Out** system is built using a server-client architecture with TCP as the transport protocol.
<div align="center">
  <img src="https://github.com/user-attachments/assets/4002061c-6101-4968-a50e-19e4a38cd3fc" alt="FindItOutDesign">
</div>

### Features:
1. **Custom TCP Message Protocol**:
   - Messages are structured with key-value pairs (e.g., `sessionID=qwer-2j3i-oij9-skp5; remainingword=15; content=qwioeprqwerioqiowperuqwpoieru`).
   - Both the server and clients parse messages to execute logic and send feedback.

2. **Server Workflow**:
   - Uses multithreading to handle messages from multiple clients simultaneously.
   - Initializes game data, processes game logic, and stores client information in a custom session list.
   - Sends notifications to all connected clients when the server is stopped.
   - Logs exceptions and server status into a log file for troubleshooting.

3. **Client Workflow**:
   - Includes an asynchronous listener to monitor server status.
   - Notifies players if the server shuts down, prompting them to reconnect or quit.
   - Handles real-time game interactions and feedback.
---

## Find It Out Game Demonstration

### 1. Game Process
In this demonstration:
- The server is hosted on an Azure virtual machine.
- Three clients connect to the server from local machines.

**Key Highlights**:
- Clients connect successfully and play simultaneously without blocking each other.
- Players are prompted to retry or quit when the timer expires.
- Players can quit at any time during the game.
- Notifications are sent to clients if the server shuts down.

<div align="center">
  <img src="https://github.com/Run-J/Find-It-Out/blob/master/FindItOutGameDemo/FindItOutDemo.gif" alt="FindItOutDemo">
</div>

---

### 2. Install/Uninstall and Logs

#### 2.1 Installing the Service
Use the command prompt or PowerShell to install the game server program as a Windows service. After successful installation, the service appears in the Windows Services list.

<div align="center">
  <img src="https://github.com/user-attachments/assets/08f2a367-b9c2-48cd-a5a7-667a06ad874b" alt="FindItOutInstall">
</div>

#### 2.2 Uninstalling the Service
To uninstall the service, use the same command with the uninstall option. After successful uninstallation, the service no longer appears in the Windows Services list.

<div align="center">
  <img src="https://github.com/user-attachments/assets/fd354b7d-bd74-4091-8894-f3ae52d24b78" alt="FindItOutUninstall">
</div>

#### 2.3 Checking the Server Log
The server log provides information about the server's status and any exceptions encountered.

<div align="center">
  <img src="https://github.com/user-attachments/assets/1d0c7be7-56b7-4604-848e-24e3e45607a8" alt="FindItOutLog" width="500">
</div>

---

### 3. Firewall Configuration on Azure and Windows
To allow players to connect to the service, configure firewalls on both Azure and Windows to permit TCP connections on port 13000.

#### Windows Firewall
<div align="center">
  <img src="https://github.com/user-attachments/assets/15a0ae08-c0c3-4b37-a13e-0b24bb636404" alt="FindItOutWindowsFirewall">
</div>

#### Azure Firewall
<div align="center">
  <img src="https://github.com/user-attachments/assets/4d233ccf-543b-428a-8316-5e50e78c42cb" alt="FindItOutAzureFirewall" width="500">
</div>

---

## Technology Stack

- **Frontend**:
  - WPF for client interface.
  - Asynchronous programming for real-time communication.
- **Backend**:
  - C# and .NET Framework for server logic.
  - Multithreading for concurrent client management.
- **Database**:
  - Local text file for game data storage.
- **Cloud Services**:
  - Azure Virtual Machine for hosting the server.
