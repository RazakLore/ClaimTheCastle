using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Reflection;

namespace ClaimTheCastle
{
    class Tilemap
    {
        private int[,] _tileData;
        private int _tileSize;
        public int Dimensions { get; set; }
        public int[,] TileData { get { return _tileData; } }
        
        private Rectangle?[] _realTiles;        // ? to make this a nullable value type
        public Vector2 Location { get; set; }
        public bool isWalkable(Point idx)
        {
            switch (_tileData[idx.X, idx.Y])
            {
                case 0:
                    return true;
                default:
                    return false;
            }
        }
        public bool testing(Vector2 playerPos)
        {
            foreach (Rectangle? tileRect in _realTiles)
            {
                if (tileRect.HasValue && tileRect.Value.Intersects(new Rectangle((int)playerPos.X, (int)playerPos.Y + 1, _tileSize - 2, _tileSize - 2)))    //Check that tileRect is not null
                {
                    return false;       //There is a collision, so the player cannot walk
                }
            }
            return true;                //There was no collision, player can walk
        }

        public Tilemap(string mapSource, Vector2 location, int dimensions, int tileSize)
        {
            Location = location;
            Dimensions = dimensions;
            _tileData = new int[dimensions, dimensions];
            _tileSize = tileSize;
            _realTiles = new Rectangle?[dimensions * dimensions];
            //gen static normal, loop through x number of times picking random pos and if random pos is not occupied put barrel down then 

            var reader = new StreamReader(File.OpenRead(mapSource + ".txt"));

            int i = 0, j = 0;
            Debug.WriteLine("Building Tilemap...");
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
                            _tileData[j, i] = Game1.RNG.Next(0, 3);     //Roughly randomise the layout of a map
                        while (_tileData[j, i] == 1);                   //Skips the solid blocks
                    }
                    if (_tileData[j, i] != 0)
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
            Debug.WriteLine("\tRead " + i + " lines and " + j + " entries per line into tilemap size + " + _tileData.Length);
        }

        public void DestroyTile(int j, int i)
        {
            //Check if the tile is destructible
            if (_tileData[j, i] == 2)
            {
                _realTiles[i * Dimensions + j] = null;
                _tileData[j, i] = 0;
                Debug.WriteLine($"Tile at ({i}, {j}) destroyed.");
            }
            else
                Debug.WriteLine($"Cannot destroy tile at ({j}, {i}) - it is not destructible.");
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
                    //sb.DrawString(Game1.debug, _tileData[i, j].ToString(), Location + destination, Color.White);
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
    }
}
