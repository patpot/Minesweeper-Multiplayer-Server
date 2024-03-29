﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Minesweeper
{
    class Board
    {
        public int Id;
        public string Username;
        public Tile[,] BoardPositions;
        public int NumberOfMines;

        private int _lives = 3;
        public int Lives 
        { 
            get { return _lives; }
            set { _lives = value; }
        }

        public bool LockedIn;
        public bool GameStarted;
        public List<Tile> TilesCheckedThisTurn = new List<Tile>();
        public int FlagsLeft;

        public Board(int _id, string _username, int boardX, int boardY)
        {
            Id = _id;
            Username = _username;

            BoardPositions = new Tile[boardX, boardY];
            for (int x = 0; x < boardX; x++)
            {
                for (int y = 0; y < boardY; y++)
                {
                    BoardPositions[x, y] = new Tile(this,x, y);
                }
            }
            NumberOfMines = (int)MathF.Ceiling(boardX * boardY / 5);
            FlagsLeft = NumberOfMines;

            Random rand = new Random();
            for (int i = 0; i < NumberOfMines; i++)
            {
                bool validPos = false;
                while (!validPos)
                {
                    int randomX = rand.Next(0, boardX);
                    int randomY = rand.Next(0, boardY);
                    if (IsMine(randomX, randomY))
                        continue;
                    else
                    {
                        BoardPositions[randomX, randomY].IsMine = true;
                        validPos = true;
                    }
                }
            }

            List<Tuple<int, int>> adjacentOffsets = new List<Tuple<int, int>>();
            adjacentOffsets.Add(Tuple.Create(-1, 1)) ;  adjacentOffsets.Add(Tuple.Create(0, 1)); adjacentOffsets.Add(Tuple.Create(1, 1));
            adjacentOffsets.Add(Tuple.Create(-1, -0));                                           adjacentOffsets.Add(Tuple.Create(1, 0));
            adjacentOffsets.Add(Tuple.Create(-1, -1)); adjacentOffsets.Add(Tuple.Create(0, -1)); adjacentOffsets.Add(Tuple.Create(1, -1));
            for (int i = 0; i < boardX; i++)
            {
                for (int j = 0; j < boardY; j++)
                {
                    Tile tileToCheck = BoardPositions[i, j];
                    int numMines = 0;
                    foreach(var offset in adjacentOffsets)
                    {
                        int xPos = i + offset.Item1;
                        int yPos = j + offset.Item2;
                        if (xPos >= 0 && xPos < boardX && yPos >= 0 && yPos < boardY)
                        {
                            tileToCheck.AdjacentTiles.Add(BoardPositions[xPos, yPos]);
                            if (BoardPositions[xPos, yPos].IsMine)
                                numMines++;
                        }

                    }
                    tileToCheck.NumberOfAdjacentMines = numMines;
                }
            }
        }

        public void RevealTile(int x, int y)
        {
            if (!GameStarted || LockedIn) return;

            BoardPositions[x, y].RevealTile();
            TilesCheckedThisTurn.Clear();
        }

        public bool IsMine(int x, int y) => BoardPositions[x, y].IsMine;

    }

    class Tile
    {
        public Board Board;
        public List<Tile> AdjacentTiles = new List<Tile>();
        public int NumberOfAdjacentMines;
        public int X;
        public int Y;
        public bool IsMine;
        public TileType CurrentTileType = TileType.Unrevealed;

        public Tile(Board board, int x, int y)
        {
            Board = board;
            X = x;
            Y = y;
        }

        public void RevealTile()
        {
            if (CurrentTileType == TileType.Flag) return; //If there's a flag, don't do anything

            if (IsMine)
            {
                // Lose
                ServerSend.PlayerHitMine(Board.Id);
            }
            else
            {
                CurrentTileType = TileType.Revealed;
                if (NumberOfAdjacentMines == 0)
                {
                    foreach (Tile tile in AdjacentTiles)
                    {
                        if (!Board.TilesCheckedThisTurn.Contains(tile))
                        {
                            Board.TilesCheckedThisTurn.Add(tile);
                            tile.RevealTile();
                        }
                    }
                }
                ServerSend.RevealTile(Board.Id, X, Y, CurrentTileType, NumberOfAdjacentMines);
            }
        }

        public void SetFlag()
        {
            if (!Board.GameStarted || Board.LockedIn) return;
            if (CurrentTileType == TileType.Revealed) return;

            if (CurrentTileType == TileType.Flag)
            {
                CurrentTileType = TileType.Unrevealed;
                Board.FlagsLeft++;
            }
            else
            {
                if (Board.FlagsLeft <= 0) return;
                CurrentTileType = TileType.Flag;
                Board.FlagsLeft--;
            }
            ServerSend.SetFlag(Board.Id, X, Y, CurrentTileType);
        }

        public enum TileType
        {
            Unrevealed,
            Revealed,
            Flag
        }
    }
}
