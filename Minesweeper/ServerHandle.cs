using GameServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minesweeper
{
    class ServerHandle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            string _username = _packet.ReadString();
            if (_username.Length > 12)
            {
                Server.Clients[_fromClient].Tcp.Disconnect();
                return;
            }
            int boardX = 10; //Temp
            int boardY = 10; //Temp

            if (_clientIdCheck != _fromClient)
            {
                Console.WriteLine("What the fuck guys");
                return;
            }

            Console.WriteLine($"{_username} has connected.");
            Server.Clients[_fromClient].Username = _username;
            Server.Clients[_fromClient].SendIntoGame(_username, boardX, boardY);
        }
        
        public static void SpawnReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            if (_clientIdCheck != _fromClient)
            {
                Console.WriteLine("What the fuck guys");
                return;
            }

            foreach (var client in Server.Clients)
            {
                if (client.Value.Tcp.Socket == null)
                    return;
            }
            //If we've reached this point all clients are filled up, meaning 2 players have joined and we can start.
            foreach (var client in Server.Clients)
            {
                client.Value.StartGame();
                client.Value.Board.GameStarted = true;
            }
            Console.WriteLine("Game starting");
        }

        public static void RevealTileReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            if (_clientIdCheck != _fromClient)
            {
                Console.WriteLine("What the fuck guys");
                return;
            }

            int x = _packet.ReadInt();
            int y = _packet.ReadInt();
            Server.Clients[_fromClient].Board.RevealTile(x, y);
        }

        public static void SendFlagReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt();
            if (_clientIdCheck != _fromClient)
            {
                Console.WriteLine("What the fuck guys");
                return;
            }

            int x = _packet.ReadInt();
            int y = _packet.ReadInt();
            Server.Clients[_fromClient].Board.BoardPositions[x, y].SetFlag();
        }
    }
}
