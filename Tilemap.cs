using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace ClaimTheCastle
{
    enum tileState
    {
        Walkable,
        Danger,
        Blocked
    }

    class Tilemap
    {
        private int[,] _tileData;
        private int _tileSize;
        public int Dimensions { get; set; }
        public int[,] TileData { get { return _tileData; } }
        
        private Rectangle?[] _realTiles;        // ? to make this a nullable value type
        public Rectangle?[] _dangerTiles { get; set; }      // Keep track of all tiles about to explode
        public Vector2 Location { get; set; }


        public bool IsWalkable(Vector2 playerPos)
        {
            foreach (Rectangle? tileRect in _realTiles)
            {
                if (tileRect.HasValue && tileRect.Value.Intersects(new Rectangle((int)playerPos.X, (int)playerPos.Y + 1, _tileSize - 2, _tileSize - 2)))    //Check that tileRect is not null
                {
                    return false;       //There is a collision, so the player cannot walk
                }
            }
            return true;
            #region 
            //switch (_tileData[GetTileIndex(playerPos).X, GetTileIndex(playerPos).Y])
            //{
            //    case 0:
            //        return tileState.Walkable;
            //    case 4:
            //        return tileState.Danger;
            //    case 8:
            //        return tileState.Walkable;
            //    case 12:
            //        return tileState.Danger;
            //    case 16:
            //        return tileState.Walkable;
            //    case 19:
            //        return tileState.Walkable;
            //    case 20:
            //        return tileState.Danger;
            //    default:
            //        return tileState.Blocked;
            //}
            #endregion
        }
        public bool IsDanger(Vector2 playerPos)
        {
            foreach (Rectangle? tileRect in _dangerTiles)
            {
                if (tileRect.HasValue)
                {
                    var rect = new Rectangle((int)playerPos.X, (int)playerPos.Y + 1, _tileSize - 2, _tileSize - 2);
                    if (tileRect.Value.Intersects(rect))
                        return true;
                }
            }
            return false;
        }

        public Tilemap(string mapSource, Vector2 location, int dimensions, int tileSize)
        {
            Location = location;
            Dimensions = dimensions;
            _tileData = new int[dimensions, dimensions];
            _tileSize = tileSize;
            _realTiles = new Rectangle?[dimensions * dimensions];
            _dangerTiles = new Rectangle?[dimensions * dimensions];
            //gen static normal, loop through x number of times picking random pos and if random pos is not occupied put barrel down then 

            var reader = new StreamReader(File.OpenRead(mapSource + ".txt"));

            int i = 0, j = 0;
            Game1.GConsole.Log("Building Tilemap...");
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                j = 0;
                foreach (var val in values)
                {
                    _tileData[j, i] = int.Parse(val);

                    if (_tileData[j, i] == 0)
                    {
                        do
                        {
                            _tileData[j, i] = Game1.RNG.Next(0, 3);         //Roughly randomise the layout of a map
                            if (_tileData[j, i] == 1)
                                _tileData[j, i] = 2;
                        }
                        while (_tileData[j, i] == 1);                       //Skips the solid blocks
                    }
                    if (_tileData[j, i] == 8)
                    {
                        do
                        {
                            _tileData[j, i] = Game1.RNG.Next(8, 11);      //Roughly randomise the layout of a map
                            if (_tileData[j, i] == 9)
                                _tileData[j, i] = 10;
                        }
                        while (_tileData[j, i] == 9) ;
                    }
                    if (_tileData[j, i] == 16)
                    {
                        do
                        {
                            _tileData[j, i] = Game1.RNG.Next(16, 19);       //Roughly randomise the layout of a map
                            if (_tileData[j, i] == 17)
                                _tileData[j, i] = 18;
                        }
                        while (_tileData[j, i] == 17);
                    }
                    if (_tileData[j, i] != 0 && _tileData[j, i] != 3 && _tileData[j, i] != 8 && _tileData[j, i] != 11 && _tileData[j, i] != 16 && _tileData[j, i] != 19 && _tileData[j, i] != 21 && _tileData[j, i] != 22)
                    {
                        int x = j * _tileSize;
                        int y = i * _tileSize;
                        _realTiles[i * dimensions + j] = new Rectangle(x, y, _tileSize, _tileSize);
                        Debug.WriteLine($"Tile ({i}, {j}) maps to index {_tileData}, Rectangle: {x}, {y}, {_tileSize}, {_tileSize}");
                    }

                    j++;
                }
                i++;
            }
            Game1.GConsole.Log("Read " + i + " lines and " + j + " entries per line into tilemap size + " + _tileData.Length);
        }

        public void DestroyTile(int j, int i, int oldTile)
        {
            //Check if the tile is destructible
            if (oldTile == 2)
            {
                _realTiles[i * Dimensions + j] = null;
                _tileData[j, i] = 0;
                Game1.GConsole.Log($"Tile at ({i}, {j}) destroyed.");
            }
            else
                Game1.GConsole.Warn($"Cannot destroy tile at ({j}, {i}) - it is not destructible.");

            if (oldTile == 10)
            {
                _realTiles[i * Dimensions + j] = null;
                _tileData[j, i] = 8;
                Game1.GConsole.Log($"Tile at ({i}, {j}) destroyed.");
            }
            else
                Game1.GConsole.Warn($"Cannot destroy tile at ({j}, {i}) - it is not destructible.");

            if (oldTile == 18)
            {
                _realTiles[i * Dimensions + j] = null;
                _tileData[j, i] = 16;
                Game1.GConsole.Log($"Tile at ({i}, {j}) destroyed.");
            }
            else
                Game1.GConsole.Warn($"Cannot destroy tile at ({j}, {i}) - it is not destructible.");
        }

        public void Draw(SpriteBatch sb, TextureAtlas ta)
        {
            var destination = Vector2.Zero;

            for (int i = 0; i < _tileData.GetLength(0); i++)
            {
                for (int j = 0; j < _tileData.GetLength(1); j++)
                {
                    var currentTile = _tileData[i, j];
                    if (currentTile == -1)
                        continue;

                    destination.X = _tileSize * i;
                    destination.Y = _tileSize * j;
                    sb.Draw(ta.Texture, Location + destination, ta.SourceRectangles[currentTile], Color.White);
                    //sb.Draw(ta.Texture, Location + destination, _realTiles[currentTile], Color.Red);
                    //sb.DrawString(Game1.debug, _tileData[i, j].ToString(), Location + destination, Color.White);  // Supposed to show collision rectangles for debugging, very broken!
                }
            }
        }

        public Point ScreenToTile(Vector2 pos)
        {
            return ((pos - Location) / _tileSize).ToPoint();
        }

        #region GetTile
        public int GetTile(Point tileLocation)
        {
            var (x, y) = tileLocation;
            if (x >= 0 && x < _tileData.GetLength(0) && y >= 0 && y < _tileData.GetLength(1))
                return _tileData[x, y];
            else
                return -1;
        }

        public int GetTile(Vector2 screenLocation)
        {
            return GetTile(ScreenToTile(screenLocation));
        }
        #endregion

        #region IsOnMap
        public bool IsOnMap(Point tileLocation)
        {
            var (x, y) = tileLocation;
            return (x >= 0 && x < _tileData.GetLength(0) && y >= 0 && y < _tileData.GetLength(1));
        }

        public bool IsOnMap(Vector2 screenLocation)
        {
            return IsOnMap(ScreenToTile(screenLocation));
        }
        #endregion

        public Point GetTileIndex(Vector2 playerPos)
        {
            int tileX = (int)Math.Floor(playerPos.X / _tileSize);
            int tileY = (int)Math.Floor(playerPos.Y / _tileSize);
            return new Point(tileX, tileY);
        }

        public void RandomiseDestructibleWalls(string mapSource)
        {

            _realTiles = null;          // Must reset the current rectangles? to have the new collision maps for level changes
            _dangerTiles = null; 
            var reader = new StreamReader(File.OpenRead(mapSource + ".txt"));

            int i = 0, j = 0;

            _realTiles = new Rectangle?[Dimensions * Dimensions];
            _dangerTiles = new Rectangle?[Dimensions * Dimensions];
            Game1.GConsole.Log("Building Tilemap...");
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                j = 0;
                foreach (var val in values)
                {
                    _tileData[j, i] = int.Parse(val);

                    if (_tileData[j, i] == 0)
                    {
                        do
                        {
                            _tileData[j, i] = Game1.RNG.Next(0, 3);         //Roughly randomise the layout of a map
                            if (_tileData[j, i] == 1)
                                _tileData[j, i] = 2;
                        }
                        while (_tileData[j, i] == 1 || _tileData[j, i] == 5);                       //Skips the solid blocks
                    }
                    if (_tileData[j, i] == 8)
                    {
                        do
                        {
                            _tileData[j, i] = Game1.RNG.Next(8, 11);      //Roughly randomise the layout of a map
                            if (_tileData[j, i] == 9)
                                _tileData[j, i] = 10;
                        }
                        while (_tileData[j, i] == 9);
                    }
                    if (_tileData[j, i] == 16)
                    {
                        do
                        {
                            _tileData[j, i] = Game1.RNG.Next(16, 19);       //Roughly randomise the layout of a map
                            if (_tileData[j, i] == 17)
                                _tileData[j, i] = 18;
                        }
                        while (_tileData[j, i] == 17);
                    }
                    if (_tileData[j, i] != 0 && _tileData[j, i] != 3 && _tileData[j, i] != 8 && _tileData[j, i] != 11 && _tileData[j, i] != 16 && _tileData[j, i] != 19 && _tileData[j, i] != 21 && _tileData[j, i] != 22)
                    {
                        int x = j * _tileSize;
                        int y = i * _tileSize;
                        _realTiles[i * Dimensions + j] = new Rectangle(x, y, _tileSize, _tileSize);
                        Debug.WriteLine($"Tile ({i}, {j}) maps to index {_tileData}, Rectangle: {x}, {y}, {_tileSize}, {_tileSize}");
                    }

                    j++;
                }
                i++;
            }
            Game1.GConsole.Log("Read " + i + " lines and " + j + " entries per line into tilemap size + " + _tileData.Length);
        }
    }
}
