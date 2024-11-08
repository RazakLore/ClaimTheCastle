using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

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
        public bool IsExploded { get; set; }
        public bool TimeToDie { get; set; }
        private Texture2D m_txr;

        private Tilemap _tileMap;   //Reference to the tile map
        public Bomb(Point position, int explosionRadius, float timeToExplode, Tilemap tileMap, Texture2D cauldron) : base(position)
        {
            Position = position;
            ExplosionRadius = explosionRadius;
            TimeToExplode = timeToExplode;
            IsExploded = false;
            TimeToDie = false;
            _tileMap = tileMap;
            m_txr = cauldron;
        }

        public void Update(GameTime gameTime)
        {
            if (IsExploded)
                return;

            if (TimeToExplode <= 0)
            {
                TriggerExplosion();
                IsExploded = true;
                TimeToDie = true;
            }
            else
                TimeToExplode -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        }

        public void TriggerExplosion()
        {
            Explode(new Point(Position.X, Position.Y));
        }

        private void Explode(Point bombPosition)
        {
            Debug.WriteLine($"Bomb detonated at {bombPosition}!");

            // Call function to handle the explosion in all directions
            ExplodeInDirection(bombPosition, 0, -1);                    //Up
            ExplodeInDirection(bombPosition, 0, 1);                     //Down
            ExplodeInDirection(bombPosition, -1, 0);                    //Left
            ExplodeInDirection(bombPosition, 1, 0);                     //Right
        }

        private void ExplodeInDirection(Point bombPosition, int dx, int dy)
        {
            int x = bombPosition.X;
            int y = bombPosition.Y;

            for (int i = 0; i <= ExplosionRadius; i++)
            {
                x += dx;
                y += dy;
                Debug.WriteLine($"Headed in direction ({dx}, {dy})");

                // Debugging: Log the current tile coordinates and check the tile data
                Debug.WriteLine($"Checking tile at ({x}, {y})");

                //Stop the explosion if it goes out of bounds
                if (!_tileMap.IsOnMap(new Point(x, y)))
                    break;

                int tileType = _tileMap.GetTile(new Point(x, y));
                Debug.WriteLine($"Tile at ({x}, {y}) is {tileType}");

                if (tileType == 2)   //Look for tiles of type 2
                {
                    Debug.WriteLine($"Destroying tile at ({x}, {y})!!!!!!!!!!!!!!!");
                    _tileMap.DestroyTile(x, y);     //Destroy the destructible wall (ANIMATE THIS LATER)
                    break;                          //Stop the explosion in this direction as it has hit a wall
                }

                if (tileType == 1)   //Solid wall
                {
                    Debug.WriteLine($"Explosion stopped at solid wall at ({x}, {y})");
                    break;                          //Stop the explosion in this direction as it has hit a wall
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(m_txr, new Vector2(Position.X * 16, Position.Y * 16), Color.White);
        }
    }
}
