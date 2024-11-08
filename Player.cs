using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;

namespace ClaimTheCastle
{
    internal class Player : GameActor
    {
        public int MaxBombs { get; set; }
        public int BombsPlaced { get; set; }
        private float m_moveTrigger;
        private float m_moveCounter;
        private float m_maxMoveTime;
        private float m_moveChangeTimer;
        private Point tileDestination;

        private bool isPlayer;
        public bool isDeciding { get; set; }
        private Tilemap _tileMap;   //Reference to the tile map

        public Player(Vector2 startPos, Texture2D txr, int frameCount, int fps, bool isPlayer/*, Tilemap tileMap*/) : base(startPos, txr, frameCount, fps)
        {
            MaxBombs = 2;
            BombsPlaced = 0;
            m_moveTrigger = 0.1f;
            m_moveCounter = 0;
            m_moveChangeTimer = 0;
            m_maxMoveTime = 5;
            this.isPlayer = isPlayer;
            tileDestination = Position.ToPoint();
        }

        public void Update(GameTime gameTime, Tilemap currentMap, KeyboardState kb, KeyboardState kbOld)
        {
            if (this.isPlayer)
            {
                if (kb.IsKeyDown(Keys.W))
                {
                    if (currentMap.testing(new Vector2((int)Position.X, (int)Position.Y - 2)))
                    {
                        Move(Direction.North);
                    }
                }
                if (kb.IsKeyDown(Keys.S))
                {
                    if (currentMap.testing(new Vector2((int)Position.X, (int)Position.Y + 2)))
                    {
                        Move(Direction.South);
                    }
                }
                if (kb.IsKeyDown(Keys.A))
                {

                    if (currentMap.testing(new Vector2((int)Position.X - 2, (int)Position.Y)))
                    {
                        Move(Direction.West);
                    }
                }
                if (kb.IsKeyDown(Keys.D))
                {
                    if (currentMap.testing(new Vector2((int)Position.X + 2, (int)Position.Y)))
                    {
                        Move(Direction.East);
                    }
                }
            }
            else        // AI PLAYERS
            {
                Direction moveDir;

                
                if (isDeciding)
                {
                    tileDestination = new Point(Game1.RNG.Next(2, 14), Game1.RNG.Next(1, 11));
                    isDeciding = false;
                    Debug.WriteLine(tileDestination + " headed to!");
                }
                 
                m_moveChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                m_moveCounter += (float)gameTime.ElapsedGameTime.TotalSeconds; //Timer

                if (m_moveChangeTimer > m_maxMoveTime)
                {
                    isDeciding = true;
                    m_moveChangeTimer = 0;
                }

                if (m_moveCounter >= m_moveTrigger)
                {
                    m_moveCounter = 0;

                    if (tileDestination.Y < Position.Y / 16 && currentMap.testing(new Vector2((int)Position.X, (int)Position.Y - 2)))
                    {
                        moveDir = Direction.North;
                    }
                    else if (tileDestination.Y > Position.Y / 16 && currentMap.testing(new Vector2((int)Position.X, (int)Position.Y + 2)))
                    {
                        moveDir = Direction.South;
                    }
                    else if (tileDestination.X < Position.X / 16 && currentMap.testing(new Vector2((int)Position.X - 2, (int)Position.Y)))
                    {
                        moveDir = Direction.West;
                    }
                    else if (tileDestination.X > Position.X / 16 && currentMap.testing(new Vector2((int)Position.X + 2, (int)Position.Y)))
                    {
                        moveDir = Direction.East;
                    }
                    else if (tileDestination == Position.ToPoint())
                    {
                        m_moveChangeTimer = m_maxMoveTime;
                        moveDir = (Direction)Game1.RNG.Next(0, 4);
                    }
                    else
                        moveDir = (Direction)Game1.RNG.Next(0, 4);

                    Move(moveDir);
                }
                else
                    moveDir = (Direction)Game1.RNG.Next(0, 4);


                //if (_tileMap.GetTileIndex(Position).X > tileDestination.X)
                //{
                //    //Move(Direction.)
                //}
            }
            

            m_rectangle = new Rectangle((int)m_position.X, (int)m_position.Y, m_txr.Width, m_txr.Height);
        }
    }
}
