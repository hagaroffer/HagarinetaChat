using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows;

namespace Client
{
    public partial class MainWindow : Window
    {
        // ViewModel for Data Binding
        public ChatViewModel ViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new ChatViewModel();
            DataContext = ViewModel; // Bind the ViewModel to the UI
        }

        // Handle Send Button Click
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var message = MessageTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                var targetUser = ActiveUsersListBox.SelectedItem as User;
                if (targetUser != null)
                {
                    // Send message to the selected user
                    ViewModel.SendMessage(message, targetUser);
                }
                else
                {
                    // Broadcast message to all users
                    ViewModel.BroadcastMessage(message);
                }
                MessageTextBox.Clear();
            }
        }

        // Handle Login Button Click
        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(username))
            {
                ViewModel.ConnectClient(username);
                ConnectButton.IsEnabled = false;
                UsernameTextBox.IsEnabled = false;
            }
        }

        // Handle Refresh Button Click
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RefreshClientsList();
            //var username = UsernameTextBox.Text.Trim();
            //if (!string.IsNullOrEmpty(username))
            //{
            //    bool connectioniSucceed = ViewModel.ConnectClient(username);
            //    if (!connectioniSucceed)
            //    {
            //        MessageBox.Show(string.Format("Connection of user {0} failed", username));
            //    }

            //    ConnectButton.IsEnabled = false;
            //    UsernameTextBox.IsEnabled = false;
            //}
        }

        // Handle Send Broadcast Button Click
        private void SendBroadcastButton_Click(object sender, RoutedEventArgs e)
        {
            //var message = MessageTextBox.Text.Trim();
            //if (!string.IsNullOrEmpty(message))
            //{
            //    var targetUser = ActiveUsersListBox.SelectedItem as User;
            //    if (targetUser != null)
            //    {
            //        // Send message to the selected user
            //        ViewModel.SendMessage(message, targetUser);
            //    }
            //    else
            //    {
            //        // Broadcast message to all users
            //        ViewModel.BroadcastMessage(message);
            //    }
            //    MessageTextBox.Clear();
            //}
        }
    }

    // User Model
    public class User
    {
        public string Username { get; set; }
    }

    // Message Model

    public class Message
    {
        public string Sender { get; set; }
        public string Content { get; set; }
    }

    //public class MSG
    //{
    //    get { return _ _person; }
    //    set
    //    {
    //        _person = value;
    //        RaisePropertyChanged("Model");  //<- this should tell the view to update
    //    }
    //}

    // ViewModel
    public class ChatViewModel
    {
        public ChatClient.ChatClient _chatClient;

        public ObservableCollection<User> ActiveUsers { get; set; }
        public ObservableCollection<Message> Messages { get; set; }

        public ChatViewModel()
        {
            _chatClient = ChatClient.ChatClient.Instance;

            // Initialize with mock data
            ActiveUsers = new ObservableCollection<User>
            {
            };

            Messages = new ObservableCollection<Message>();
        }

        // Send message to a specific user
        public void SendMessage(string message, User targetUser)
        {
            Messages.Add(new Message
            {
                Sender = "Me -> " + targetUser.Username,
                Content = message
            });

            // Integrate backend logic here
        }

        // Broadcast message to all users
        public void BroadcastMessage(string message)
        {
            Messages.Add(new Message
            {
                Sender = "Me -> Broadcast",
                Content = message
            });

            // Integrate backend logic here
        }

        public void ConnectClient(string username)
        {
            _chatClient.StartClient(username);

            RefreshClientsList();
        }

        public void RefreshClientsList()
        {
            var connectedClients = _chatClient.GetConnectedClients();
            if (connectedClients != null)
            {
                foreach (var client in connectedClients)
                {
                    ActiveUsers.Clear();
                    ActiveUsers.Add(new User() { Username = client });
                }
            }
        }
    }
}
