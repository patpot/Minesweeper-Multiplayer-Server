using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using GameServer;

namespace Minesweeper
{
    class Server
    {
        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> Clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> PacketHandlers = new Dictionary<int, PacketHandler>();
        private static TcpListener _tcpListener;
        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting server...");
            InitialiseServerData();

            _tcpListener = new TcpListener(IPAddress.Any, Port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Server started on {Port}.");
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = _tcpListener.EndAcceptTcpClient(_result);
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}");
            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (Clients[i].Tcp.Socket == null)
                {
                    Clients[i].Tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full");
        }

        private static void InitialiseServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                Clients.Add(i, new Client(i));
            }

            PacketHandlers = new Dictionary<int, PacketHandler>()
            {
                {(int)ServerPackets.WELCOME, ServerHandle.WelcomeReceived },
                {(int)ServerPackets.SPAWN_PLAYER, ServerHandle.SpawnReceived },
                {(int)ClientPackets.SEND_REVEAL_TILE, ServerHandle.RevealTileReceived },
                {(int)ClientPackets.SEND_FLAG, ServerHandle.SendFlagReceived }
            };

            Console.WriteLine("Initalized packets.");
        }
    }
}
