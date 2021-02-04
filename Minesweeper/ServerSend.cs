using GameServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minesweeper
{
    class ServerSend
    {
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.Clients[_toClient].Tcp.SendData(_packet);
        }

        private static void SendTCPDataToAll(Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                Server.Clients[i].Tcp.SendData(_packet);
            }
        }

        private static void SendTCPDataToAll(int _exceptClient, Packet _packet)
        {
            _packet.WriteLength();
            for (int i = 1; i <= Server.MaxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.Clients[i].Tcp.SendData(_packet);
                }
            }
        }
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.WELCOME))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void SpawnBoard(int _toClient, Board _board, int boardX, int boardY)
        {
            using (Packet _packet = new Packet((int)ServerPackets.SPAWN_PLAYER))
            {
                _packet.Write(_board.Id);
                _packet.Write(_board.Username);
                _packet.Write(boardX);
                _packet.Write(boardY);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void StartGame(int _toClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.START_GAME))
            {
                _packet.Write(_toClient);
                SendTCPData(_toClient, _packet);
            }
        }

        public static void RevealTile(int _toClient, int x, int y, Tile.TileType tileType, int numMines)
        {
            using (Packet _packet = new Packet((int)ServerPackets.SEND_REVEAL_TILE))
            {
                _packet.Write(_toClient);
                _packet.Write(x);
                _packet.Write(y);
                _packet.Write((int)tileType);
                _packet.Write(numMines);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void SetFlag(int _toClient, int x, int y, Tile.TileType tileType)
        {
            using (Packet _packet = new Packet((int)ServerPackets.SEND_FLAG))
            {
                _packet.Write(_toClient);
                _packet.Write(x);
                _packet.Write(y);
                _packet.Write((int)tileType);

                SendTCPDataToAll( _packet);
            }
        }

        public static void PlayerDisconnect(int _dcdClient)
        {
            using (Packet _packet = new Packet((int)ServerPackets.CLIENT_DISCONNECT))
            {
                _packet.Write(_dcdClient);

                SendTCPDataToAll(_packet);
            }
        }
    }
}
