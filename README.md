## HagarinetaChat

## Description
HagarinetaChat is a chat application designed for communication between users. The project comprises several components:

- **ChatClient**: The client-side application for user interaction.
- **ChatServer**: The server-side application managing message routing and user connections.
- **ChatCommon**: Shared libraries and resources utilized by both the client and server.
- **CliClient**: A command-line interface version of the chat client.

## Features
- Real-time messaging between users.
- Support for CLI client.
- Scalable server architecture to handle numerous simultaneous connections.

## Getting Started

## Installation

1. **Clone the repository**:
 
   git clone https://github.com/hagaroffer/HagarinetaChat.git

2. **Navigate to the project directory**:
   cd HagarinetaChat

3. **Restore dependencies**:
   dotnet restore

## Usage

1. **Build the solution**:
 
   dotnet build

2. **Run the server**:
   Navigate to the `ChatServer` directory and start the server:
   cd ChatServer
   dotnet run

3. **Run the client**:
   In a new terminal, navigate to the `CliClient` directory and start the client:
   cd CliClient
   dotnet run

4. **Connect and chat**:
   - Use the HELP command to get familier with the commands.
   - Start sending messages to other connected users.

 ## main design of the application:

    Thread Management
        - Each client is handled using threads to ensure concurrency.
        - A `Dictionary` is used to maintain connected users, synchronized with locks to ensure thread safety.
        - The `QueueMessagesSender` class processes a shared queue to manage outgoing messages efficiently.

    Real-time messaging is implemented using TCP protocol.

    The shared 'ChatCommon' library:
        * Ensures consistency between the clients and server.
        * Reduce code duplication and errors.
        * suply configurarion file ('appsettings.json') for updating the `IPAddress` and `Port` values as needed.

    Server key Components:

        * ChatServer:
            Listens for incoming client connections on a specific IP address and port.
            Establishes a persistent communication channel.
            Spawns a new thread for each connected client to handle communication concurrently (SingleConnectionListener).
            Tracks active client connections in a central dictionary.
            Spawns a new thread for sending messgaes (QueueMessagesSender).

        * SingleConnectionListener:
            A dedicated thread for each client.
            Creates new client.
            Reading incoming messages from the client and storing them in a central messages queue.

        * QueueMessagesSender:
            Responsible for managing outgoing messages to clients. Its primary role is to ensure reliable, orderly delivery of messages.
            It continuously dequeues messages and sends them to the respective client via its NetworkStream.


    Client key Components:

        * IChatClient:
            Provides users with an interface to connect to the chat server, and send/receive messages.

        * ChatClient:
            Initiates a new TcpClient and establishes connection to the server.
            Sends messages to the server.
            Spawns a new thread for listening to incoming messages (ClientListener).

        * ClientListener:
            Incoming messages from the server are processed and displayed, allowing users to view messages.
