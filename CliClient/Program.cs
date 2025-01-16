using ChatClient;
using System.Text.RegularExpressions;

public class Program
{
    private static ChatClient.ChatClient _chatClient;

    static void Main(string[] args)
    {
        Console.WriteLine(    @"=========  Welcome to Hagarineta's Chat  ========="
        +Environment.NewLine + "==================================================");

        _chatClient = ChatClient.ChatClient.Instance;

        while (true)
        {
            var input = Console.ReadLine();
            List<string> strings = ParseInput(input);

            string command = strings[0].ToLower();

            switch (command)
            {
                case "connect":
                    HandleConnectionRequest(strings[1]);
                    break;
                case "send":
                    if (strings.Count() == 3)
                    {
                        HandleSendPrivateRequest(strings[1], strings[2], "");
                    }
                    else if (strings.Count() == 4)
                    {
                        HandleSendPrivateRequest(strings[1], strings[2], strings[3]);
                    }
                    break;
                case "sendbroadcast":
                    if (strings.Count() == 2)
                    {
                        HandleSendBroadcastRequest(strings[1], "");
                    }
                    else if (strings.Count() == 3)
                    {
                        HandleSendBroadcastRequest(strings[1], strings[2]);
                    }
                    break;
                case "getconnectedclients":
                    HandleGetConnectedClientsRequest();
                    break;
                case "help":
                    ShowHelp();
                    break;
                default:
                    Console.WriteLine("First argument is not a familier command");
                    break;
            }
        }
    }

    private static void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine();
        Console.WriteLine("connect <username>");
        Console.WriteLine("\tEstablishes a connection to the specified server.");
        Console.WriteLine("\tExample: connect MyUsername");
        Console.WriteLine();

        Console.WriteLine("send <recipientUsername> <message> [optional filePathToSend]");
        Console.WriteLine("\tSends a private message to the specified recipient.");
        Console.WriteLine("\tOptionally, you can add a file path.");
        Console.WriteLine("\tExample: send \"john Hello!\"");
        Console.WriteLine("\tExample with flag: send \"john Hello!\" C:\\Users\\YourUsername\\Documents\\example.txt");
        Console.WriteLine();

        Console.WriteLine("sendbroadcast <message> [optional filePathToSend]");
        Console.WriteLine("\tSends a broadcast message to all connected clients.");
        Console.WriteLine("\tOptionally, you can add a file path.");
        Console.WriteLine("\tExample: sendbroadcast \"Hello everyone!\"");
        Console.WriteLine("\tExample with flag: sendbroadcast \"Hello everyone!\" C:\\Users\\YourUsername\\Documents\\example.txt");
        Console.WriteLine();

        Console.WriteLine("getconnectedclients");
        Console.WriteLine("\tRetrieves a list of currently connected clients.");
        Console.WriteLine("\tExample: getconnectedclients");
        Console.WriteLine();

        Console.WriteLine("help");
        Console.WriteLine("\tDisplays this help message.");
        Console.WriteLine("\tExample: help");
    }

    private static List<string> ParseInput(string input)
    {
        List<string> result = new List<string>();

        // Regex to match either quoted strings or non-quoted words
        string pattern = @"(?:\""(.*?)\"")|([^""\s]+)";

        MatchCollection matches = Regex.Matches(input, pattern);

        foreach (Match match in matches)
        {
            if (match.Groups[1].Success)
            {
                result.Add(match.Groups[1].Value);
            }
            else if (match.Groups[2].Success)
            {
                result.Add(match.Groups[2].Value);
            }
        }

        return result;
    }

    private static void HandleConnectionRequest(string userName)
    {
        //Thread thread = new Thread(() => _chatClient.StartClient(userName));
        //thread.Start();
        _chatClient.StartClient(userName); 
    }

    private static void HandleSendPrivateRequest(string destinationUsername, string message, string filePath)
    {
        _chatClient.SendPrivateMessage(destinationUsername, message, filePath);
    }

    private static void HandleSendBroadcastRequest(string message, string filePath)
    {
        _chatClient.SendBroadcastMessage(message, filePath);
    }

    private static void HandleGetConnectedClientsRequest()
    {
        var connectedClients = _chatClient.GetConnectedClients();
        if (connectedClients != null)
        {
            string connectedClientsMessageToShow = string.Format("Currently connected clients: {0}", string.Join(", ", connectedClients));
            Console.WriteLine(connectedClientsMessageToShow);
        }
    }
}