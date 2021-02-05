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

        public static void PlayerHitMine(int _playerId)
        {
            Board thisBoard = Server.Clients[_playerId].Board;
            thisBoard.Lives -= 1;
            using (Packet _packet = new Packet((int)ServerPackets.PLAYER_HIT_MINE))
            {
                _packet.Write(_playerId);
                _packet.Write(thisBoard.Lives);

                SendTCPDataToAll(_packet);
            }
            //If one of the boards was already ended then this is the second player hitting a mine and therefore gameover 
            foreach (var clients in Server.Clients.Values)
                if (clients.Board.GameStarted == false && thisBoard.Lives == 0)
                    GameLogic.EndGame();
            //Needs to be done here for the first logic check to work
            SendMessage(time: 3f, message: $"{Server.Clients[_playerId].Username} has hit a mine");
            if (thisBoard.Lives == 0)
            {
                thisBoard.GameStarted = false;
                thisBoard.LockedIn = true;
            }
        }

        public static void SendMessage(float time, string message)
        {
            using (Packet _packet = new Packet((int)ServerPackets.SEND_MESSAGE))
            {
                _packet.Write(time);
                _packet.Write(message);

                SendTCPDataToAll(_packet);
            }
        }

        public static void EndGame(int player1FlagsCorrect, int player2FlagsCorrect)
        {
            using (Packet _packet = new Packet((int)ServerPackets.GAME_END))
            {
                _packet.Write(player1FlagsCorrect);
                _packet.Write(player2FlagsCorrect);

                SendTCPDataToAll(_packet);
            }
        }
    }
}
