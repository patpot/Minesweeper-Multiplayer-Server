using System;
using System.Collections.Generic;
using System.Text;

namespace Minesweeper
{
    class GameLogic
    {
        public static void Update()
        {
            ThreadManager.UpdateMain();
        }

        public static void EndGame()
        {
            //Calculate winner
            Board player1Board = Server.Clients[1].Board;
            Board player2Board = Server.Clients[2].Board;

            int player1FlagsCorrect = 0;
            foreach(Tile tile in player1Board.BoardPositions)
            {
                if (tile.CurrentTileType == Tile.TileType.Flag && tile.IsMine)
                    player1FlagsCorrect++;
            }

            int player2FlagsCorrect = 0;
            foreach (Tile tile in player2Board.BoardPositions)
            {
                if (tile.CurrentTileType == Tile.TileType.Flag && tile.IsMine)
                    player2FlagsCorrect++;
            }

            //Display winner
            ServerSend.EndGame(player1FlagsCorrect, player2FlagsCorrect);
        }
    }
}
