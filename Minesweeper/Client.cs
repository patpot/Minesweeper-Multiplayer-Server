using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using GameServer;

namespace Minesweeper
{
    class Client
    {
        public static int DataBufferSize = 4096;
        public int Id;
        public string Username;
        public Board Board;
        public TCP Tcp;
        
        public Client(int _clientId)
        {
            Id = _clientId;
            Tcp = new TCP(Id);
        }
        public class TCP
        {
            public TcpClient Socket;

            private readonly int _id;
            private NetworkStream _stream;
            private Packet _receivedData;
            private byte[] _receiveBuffer;
            public TCP(int _id)
            {
                this._id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                Socket = _socket;
                Socket.ReceiveBufferSize = DataBufferSize;
                Socket.SendBufferSize = DataBufferSize;

                _stream = _socket.GetStream();

                _receivedData = new Packet();
                _receiveBuffer = new byte[DataBufferSize];
                _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(_id, "Welcome to the server");
            }

            public void SendData(Packet _packet)
            {
                try
                {
                    if (Socket != null)
                    {
                        _stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception _ex)
                {
                    Console.WriteLine($"Error sending data to player {_id} via TCP: {_ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = _stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        Server.Clients[_id]._disconnect();
                        return;
                    }

                    byte[] _data = new byte[_byteLength];
                    Array.Copy(_receiveBuffer, _data, _byteLength);

                    _receivedData.Reset(HandleData(_data));
                    _stream.BeginRead(_receiveBuffer, 0, DataBufferSize, ReceiveCallback, null);
                }
                catch (Exception _ex)
                {
                    Console.Write($"Error receiving TCP data: {_ex}");
                    Disconnect();
                }
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;

                _receivedData.SetBytes(_data);

                if (_receivedData.UnreadLength() >= 4)
                {
                    _packetLength = _receivedData.ReadInt();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                while (_packetLength > 0 && _packetLength <= _receivedData.UnreadLength())
                {
                    byte[] _packetBytes = _receivedData.ReadBytes(_packetLength);
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt();
                            Server.PacketHandlers[_packetId](_id, _packet);
                        }
                    });

                    _packetLength = 0;
                    if (_receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = _receivedData.ReadInt();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }
                }
                if (_packetLength <= 1)
                {
                    return true;
                }

                return false;
            }

            public void Disconnect()
            {
                Socket.Close();
                _stream = null;
                _receivedData = null;
                _receiveBuffer = null;
                Socket = null;
                ServerSend.PlayerDisconnect(_id);
            }
        }

        public void SendIntoGame(string _playerName, int boardX, int boardY)
        {
            Board = new Board(Id, _playerName, boardX, boardY);

            foreach (Client _client in Server.Clients.Values)
            {
                if (_client.Board != null)
                    if (_client.Id != Id)
                        ServerSend.SpawnBoard(Id, _client.Board, boardX, boardY);
            }

            foreach (Client _client in Server.Clients.Values)
            {
                if (_client.Board != null)
                {
                    ServerSend.SpawnBoard(_client.Id, Board, boardX, boardY);
                }
            }
        }

        public void StartGame() => ServerSend.StartGame(Id);

        private void _disconnect()
        {
            Console.WriteLine($"{Tcp.Socket.Client.RemoteEndPoint} has disconnected.");

            Board = null;
            Tcp.Disconnect();
        }
    }
}
