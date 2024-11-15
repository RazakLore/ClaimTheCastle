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

        private float m_intoMazeTimer;
        private float m_maxIntoMazeTimer = 0.5f;

        private Point tileDestination;
        private Rectangle playerCollision;
        private Direction _prevAiDrection = Direction.North;
        private float _prevAiDrectionXInc = 0.0f;
        private float _prevAiDrectionYInc = -2.0f;

        private bool isPlayer;
        public bool isDeciding { get; set; }
        public bool placingBomb { get; set; }
        private Tilemap _tileMap;   //Reference to the tile map

        public Player(Vector2 startPos, Texture2D txr, int frameCount, int fps, bool isThisPlayer/*, Tilemap tileMap*/) : base(startPos, txr, frameCount, fps)
        {
            MaxBombs = 2;
            BombsPlaced = 0;
            m_moveTrigger = 0.08f;
            m_moveCounter = 0;
            m_moveChangeTimer = 0;
            m_maxMoveTime = 5;
            isPlayer = isThisPlayer;
            tileDestination = Position.ToPoint();
            playerCollision = new Rectangle((int)Position.X, (int)Position.Y + 1, 14, 14);
        }

        public void Update(GameTime gameTime, Tilemap currentMap, KeyboardState kb, KeyboardState kbOld)
        {
            if (isPlayer)
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
                int rnj = Game1.RNG.Next(0, 4);
                bool canMove = true;

                m_moveChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds; // Timer for deciding to change direction
                m_moveCounter += (float)gameTime.ElapsedGameTime.TotalSeconds; // Timer for controlling movement intervals

                // Timer for forced direction change (to prevent staying stuck on edges)
                if (m_moveChangeTimer > m_maxMoveTime)
                {
                    isDeciding = true;
                    m_moveChangeTimer = 0;  // Reset the timer when it triggers a change
                }

                if (m_moveCounter >= m_moveTrigger)
                {
                    m_moveCounter = 0;

                    m_moveChangeTimer = m_maxMoveTime;

                    m_intoMazeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    float newX = Position.X;
                    float newY = Position.Y;

                    // Give it a value to satisfy compiler;
                    moveDir = _prevAiDrection;

                    // Array of possible moves
                    float[] possibleMoves = {
                        newX + 2, newY, (float)Direction.East,  // East
                        newX - 2, newY, (float)Direction.West,  // West
                        newX, newY + 2, (float)Direction.South, // South
                        newX, newY - 2, (float)Direction.North, // North
                    };

                    int i = 0;
                    Debug.WriteLine("Prev direction: " + _prevAiDrection);
                    Vector2 newPoint = new Vector2((int)newX + _prevAiDrectionXInc, (int)newY + _prevAiDrectionYInc);

                    int idx = Game1.RNG.Next(0, 4) * 3;

                    // Check if the AI is stuck and just keep moving in its previous direction
                    if (currentMap.testing(newPoint))
                    {
                        Debug.WriteLine("Using prev dir");
                        // Skip the loop if we can still move the other direction
                        idx = possibleMoves.Length;
                        moveDir = _prevAiDrection;
                    }

                    // Generates a random number every update:
                    int randomDecision = Game1.RNG.Next(0, 15); // Make the odds of new direction 1/15

                    // If the random number is within the odds, change the AI's direction
                    if (randomDecision < 1) // Set it as a 1 in 15 chance of new direction every update
                    {
                        // Reset idx to force the new direction
                        idx = Game1.RNG.Next(0, 4) * 3;
                    }

                    //// If the AI has been in the same position for a while, force it to change direction
                    //if (m_intoMazeTimer > m_maxIntoMazeTimer)
                    //{
                    //    // Reset timer and force a random movement decision inside the maze
                    //    m_intoMazeTimer = 0;
                    //    idx = Game1.RNG.Next(0, 4) * 3;
                    //    Debug.WriteLine("Forced direction change!");
                    //}

                    // Movement loop that chooses a valid move based on collision checks
                    for (i = idx; i <= possibleMoves.Length - 3;)
                    {
                        newPoint.X = possibleMoves[i];
                        newPoint.Y = possibleMoves[i + 1];
                        moveDir = (Direction)possibleMoves[i + 2];
                        if (currentMap.testing(newPoint))
                            break;
                        // Re-randomize idx to avoid bias towards any direction
                        i = Game1.RNG.Next(0, 4) * 3;
                    }

                    // Update direction and movement
                    if (_prevAiDrection != moveDir)
                    {
                        _prevAiDrection = moveDir;
                        _prevAiDrectionXInc = possibleMoves[i] - newX;
                        _prevAiDrectionYInc = possibleMoves[i + 1] - newY;
                    }

                    if (canMove)
                        Move(moveDir);
                }
                else
                {
                    // If the AI is not actively moving, allow it to pick a random direction
                    moveDir = (Direction)Game1.RNG.Next(0, 5);
                }
            }
            
            m_rectangle = new Rectangle((int)m_position.X, (int)m_position.Y, m_txr.Width, m_txr.Height);
        }
    }

    enum AI_States
    {
        Idle,
        Exploring,
        RunningAway,
        SeekingPlayer
    }
}
