using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ClaimTheCastle
{
    class GameObject
    {
        public Point Position { get; set; }
        
        public GameObject(Point position)
        {
            Position = position;
        }
    }
    class Bomb : GameObject
    {
        public int ExplosionRadius { get; set; }
        public float TimeToExplode { get; set; }
        private float timeToDelete;
        public bool IsExploded { get; set; }
        public bool TimeToDie { get; set; }
        public Rectangle CauldronCollision { get; set; }
        public Rectangle?[] DangerTiles { get; set; }
        private Texture2D m_txr;

        private Tilemap _tileMap;   //Reference to the tile map

        private int playerOwner;
        public int PlayerOwner { get { return playerOwner; } }
        public int x { get; set; }
        public int y { get; set; }
        private List<GenericExplosion> genericExplosions;
        private int oldTile;
        public HashSet<Point> UnsafeTiles { get; private set; }

        public Bomb(Point position, int explosionRadius, float timeToExplode, Tilemap tileMap, Texture2D cauldron, int playerOwn, List<GenericExplosion> explodeTrigger, List<GenericUnsafeTileClear> dangerTrigger, GameTime gameTime) : base(position)
        {
            Position = position;
            ExplosionRadius = explosionRadius;
            TimeToExplode = timeToExplode;
            IsExploded = false;
            TimeToDie = false;
            _tileMap = tileMap;
            m_txr = cauldron;
            playerOwner = playerOwn;
            CauldronCollision = new Rectangle(position.X, position.Y, 16, 16);
            DangerTiles = new Rectangle?[7 * 7];
            genericExplosions = explodeTrigger;
            UnsafeTiles = new HashSet<Point>();
            dangerTrigger.Add(new GenericUnsafeTileClear(Position, ExplosionRadius, _tileMap, gameTime));
        }

        public void Update(GameTime gameTime)
        {
            //if (IsExploded)
                //return;             //Break out the update loop here

            if (TimeToExplode <= 0)
            {
                TriggerExplosion(gameTime);
                IsExploded = true;
                if (timeToDelete >= 0.4f)
                    TimeToDie = true;           // Doesnt fix the problem
                else
                    timeToDelete += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
                TimeToExplode -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        }

        public void TriggerExplosion(GameTime gameTime)
        {
            Explode(new Point(Position.X, Position.Y), gameTime);
        }

        private void Explode(Point bombPosition, GameTime gameTime)
        {
            Game1.GConsole.Warn($"Bomb detonated at {bombPosition}!");

            // Call function to handle the explosion in all directions
            ExplodeInDirection(bombPosition, 0, -1, gameTime);                    //Up
            ExplodeInDirection(bombPosition, 0, 1, gameTime);                     //Down
            ExplodeInDirection(bombPosition, -1, 0, gameTime);                    //Left
            ExplodeInDirection(bombPosition, 1, 0, gameTime);                     //Right
        }

        private void ExplodeInDirection(Point bombPosition, int dx, int dy, GameTime gameTime)
        {
            x = bombPosition.X;
            y = bombPosition.Y;

            for (int i = 0; i < ExplosionRadius; i++)      // For every tile before 4
            {
                x += dx;    //Head 1 in targeted direction
                y += dy;
                Game1.GConsole.Log($"Headed in direction ({dx}, {dy})");

                // Debugging: Log the current tile coordinates and check the tile data
                Game1.GConsole.Log($"Checking tile at ({x}, {y})");

                //Stop the explosion if it goes out of bounds
                if (!_tileMap.IsOnMap(new Point(x, y)))     //If explosion is not on a map tile
                    break;

                int tileType = _tileMap.GetTile(new Point(x, y));       //Get the type of tile the blast is on
                Game1.GConsole.Log($"Tile at ({x}, {y}) is {tileType}");   // Log the type of tile

                //UnsafeTiles.Add(new Point(x, y));           // Add this tile to the hash set so that it is marked as unsafe

                if (tileType == 2 || tileType == 10 || tileType == 18)   //Look for tiles of type 2
                {
                    Game1.GConsole.Log($"Destroying tile at ({x}, {y})!");

                    oldTile = _tileMap.TileData[x, y];                                                  //Remember what kind of wall this is
                    _tileMap.TileData[x, y] = 24;                                                       //Set the wall to the first animation frame
                    genericExplosions.Add(new GenericExplosion(new Point(x, y), _tileMap, oldTile));    //Add the explosion logic to the generic list

                    break;                                                                              //Stop the explosion in this direction as it has hit a wall
                }

                if (tileType == 1 || tileType == 9 || tileType == 17)   //Solid wall
                {
                    Game1.GConsole.Log($"Explosion stopped at solid wall at ({x}, {y})");
                    break;                          //Stop the explosion in this direction as it has hit a wall
                }

                
                //_tileMap.TileData[x, y] = 4;        // Same for lava

                //Collision with other bombs to instantly explode them when hit with this explosion
            }
        }

        public void Draw(SpriteBatch sb)
        {
            //if (!_tileMap.animateExplosion)
                sb.Draw(m_txr, new Vector2(Position.X * 16, Position.Y * 16), Color.White);
            //else
               //sb.Draw(m_txr, new Vector2(Position.X * 16, Position.Y * 16), Color.Red);
        }

        
        
    }

    class GenericExplosion
    {
        int[,] _tileData;
        private Tilemap tilemap;
        private float animationTimer = 0;
        public bool animateExplosion { get; set; }
        
        private Point placeToExplode;
        private int j, i;
        private int originalTile;
        public GenericExplosion(Point position, Tilemap tilemapPass, int oldTile)
        {
            _tileData = tilemapPass.TileData;
            tilemap = tilemapPass;
            animateExplosion = true;
            placeToExplode = position;
            j = placeToExplode.X;
            i = placeToExplode.Y;
            originalTile = oldTile;
        }

        public void TileExplosionAnimate(GameTime gameTime)
        {
            if (_tileData[j, i] < 24 || _tileData[j, i] > 30)
                originalTile = _tileData[j, i];
            if (animateExplosion)
            {
                if (_tileData[j, i] >= 24 && _tileData[j, i] < 30)
                    animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (animationTimer > 0.1f)
                {
                    if (_tileData[j, i] != 30)      // While the tile isn't the final tile in the animation
                    {
                        _tileData[j, i]++;          // Increment the tile value
                    }
                    if (_tileData[j, i] >= 30)      // Once the tile is equal or greater than the final tile in the animation
                    {
                        tilemap.DestroyTile(j, i, originalTile);    // Execute destruction code
                        animateExplosion = false;                   // End the animation
                    }
                    animationTimer = 0;             // While animation hasn't ended, reset timer to go again
                }
            }
        }
    }

    class GenericUnsafeTileClear
    {
        public Point Position { get; private set; }
        private Tilemap _tileMap;
        private int explosionRadius;
        private int x, y;
        public GenericUnsafeTileClear(Point position, int explodeRadi, Tilemap tilemapPass, GameTime gameTime)
        {
            Position = position;
            explosionRadius = explodeRadi;
            _tileMap = tilemapPass;
            GetUnsafeTiles(0, -1, gameTime);
            GetUnsafeTiles(0, 1, gameTime);
            GetUnsafeTiles( -1, 0, gameTime);
            GetUnsafeTiles(1, 0, gameTime);
        }
        
        public void ClearTileExecutor(GameTime gameTime)
        {
            ClearUnsafeTiles(0, -1, gameTime);
            ClearUnsafeTiles(0, 1, gameTime);
            ClearUnsafeTiles(-1, 0, gameTime);
            ClearUnsafeTiles(1, 0, gameTime);
        }

        private void GetUnsafeTiles(int dx, int dy, GameTime gameTime)
        {
            x = Position.X;
            y = Position.Y;

            for (int i = 0; i < explosionRadius; i++)      // For every tile before 4
            {
                x += dx;    //Head 1 in targeted direction
                y += dy;

                if (!_tileMap.IsOnMap(new Point(x, y)))     //If explosion is not on a map tile
                    break;

                int tileType = _tileMap.GetTile(new Point(x, y));       //Get the type of tile the blast is on

                if (tileType == 2 || tileType == 10 || tileType == 18)   //Look for tiles of type 2
                    break;

                if (tileType == 1 || tileType == 9 || tileType == 17)   //Solid wall
                    break;

                if (tileType == 0)
                    _tileMap.TileData[x, y] = 4;        // If dungeon floor, set to dungeon floor copy but its unsafe!!
                if (tileType == 8)
                    _tileMap.TileData[x, y] = 12;       // Same for ice
                if (tileType == 16 || tileType == 19)
                {
                    _tileMap.TileData[x, y] = 4;        // Just a visual tracker to see what tiles are marked.
                    _tileMap._dangerTiles[x * 17 + y] = new Rectangle(x * 16, y * 16, 16, 16);
                }
            }
        }
        private void ClearUnsafeTiles(int dx, int dy, GameTime gameTime)
        {
            x = Position.X;
            y = Position.Y;

            for (int i = 0; i < explosionRadius; i++)      // For every tile before 4
            {
                x += dx;    //Head 1 in targeted direction
                y += dy;

                if (!_tileMap.IsOnMap(new Point(x, y)))     //If explosion is not on a map tile
                    break;

                int tileType = _tileMap.GetTile(new Point(x, y));       //Get the type of tile the blast is on

                if (tileType == 2 || tileType == 10 || tileType == 18)   //Look for tiles of type 2
                    break;

                if (tileType == 1 || tileType == 9 || tileType == 17)   //Solid wall
                    break;

                if (_tileMap.TileData[x, y] == 4)
                    _tileMap.TileData[x, y] = 16;           // Instead of this if loop, add an animation in here instead
                _tileMap._dangerTiles[x * 17 + y] = null;   // Clear this tile of danger rectangle
            }
        }
    }

    class Powerups : GameObject
    {
        private Point m_pos;
        private int m_powerupType;
        private Tilemap _tileMap;   //Ref to tile map
        public Powerups(Point position, int powerupType, int oldTile, Tilemap tilemap) : base(position)
        {
            m_pos = position;
            m_powerupType = powerupType;
            if (m_powerupType == 0)
                _tileMap.TileData[m_pos.X, m_pos.Y] = 5;        // Increased explosion radius
            if (m_powerupType == 1)
                _tileMap.TileData[m_pos.X, m_pos.Y] = 6;        // Extra cauldron
            if (m_powerupType == 2)
                _tileMap.TileData[m_pos.X, m_pos.Y] = 7;        // Increased player speed
        }

        public void Update()
        {
            //Collision check with players
        }

        //Tilemap destroyytile on player collision

    }
}
