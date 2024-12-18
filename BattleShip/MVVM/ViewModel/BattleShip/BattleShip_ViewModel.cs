﻿using BattleShip.MVVM.View.BattleShip;
using BattleShipServer;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Input;
using WPF_App.Core;
using WPF_App.MVVM.Model;
using WPF_App.MVVM.View;

namespace WPF_App.MVVM.ViewModel
{
    internal class BattleShip_ViewModel : ObservableObject
    {
        private object _currentView; public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }
        private bool isConnected; public bool IsConnected
        {
            get => isConnected;
            set { isConnected = value; OnPropertyChanged(); }
        }
        private string _messageToSend; public string MessageToSend
        {
            get => _messageToSend;
            set { _messageToSend = value; OnPropertyChanged(); }
        }
        public ObservableCollection<Message> ChatMessages { get; set; }

        private Player _player;
        private Client _client;

        public ICommand SendMessageCommand { get; }

        public BattleShip_ViewModel()
        {
            var connectionView = new BattleShip_Connection_View();
            var vm = connectionView.DataContext as BattleShip_Connection_ViewModel;
            vm.ConnectedToServer += ConnectionView_ConnectedToServer;

            _player = new Player("Me");

            this.CurrentView = connectionView;

            ChatMessages = new ObservableCollection<Model.Message>();

            StartServer();

            SendMessageCommand = new RelayCommand(
                _ => SendChatMessage(),
                _ => IsConnected && !string.IsNullOrEmpty(MessageToSend));
        }


        private void StartServer()
        {
            Server server = new Server();
            try
            {
                _ = server.StartServer();
            }
            catch (Exception)
            {
                Console.WriteLine("Server already running");
            }
        }

        private void SendChatMessage()
        {
            if (string.IsNullOrEmpty(MessageToSend)) return;

            // Send to server
            _client.SendChatMessage(MessageToSend);
            MessageToSend = string.Empty;
        }

        private void ConnectionView_ConnectedToServer(Client client)
        {
            _client = client;
            _client.ShipPlacing += Client_ShipPlacing;
            _client.MessageReceived += Client_MessageReceived;
            _client.NewGame += Client_NewGame;
            IsConnected = true;

            this.CurrentView = new BattleShip_Loading_View();
        }
        private void Client_MessageReceived(string arg1, MessageSenderType arg2)
        {
            Message message = new Message(arg1) 
            { 
                IsMine = arg2 == MessageSenderType.Player,
                IsServer = arg2 == MessageSenderType.Server
            };

            App.Current.Dispatcher.Invoke(() =>
            {
                ChatMessages.Add(message);
            });
        }

        private void Client_ShipPlacing()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var shipPlacementView = new BattleShip_ShipPlacement_View();
                var vm = shipPlacementView.DataContext as BattleShip_ShipPlacement_ViewModel;
                vm.PlayerReady += ShipPlacement_PlayerReady;
                vm.SetClient(_client);
                vm.SetPlayer(_player);

                this.CurrentView = shipPlacementView;
            });
        }

        private void Client_NewGame()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                _player.Reset();
                this.CurrentView = new BattleShip_Loading_View();
            });
        }


        private void ShipPlacement_PlayerReady()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var gameView = new BattleShip_Game_View();
                var vm = gameView.DataContext as BattleShip_Game_ViewModel;
                vm.SetClient(_client);
                vm.SetPlayer(_player);

                this.CurrentView = gameView;
            });
        }
    }
}
