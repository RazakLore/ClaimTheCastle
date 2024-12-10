using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using System;

namespace ClaimTheCastle
{
    internal class Player : GameActor
    {
        public int MaxBombs { get; set; }
        public int BombsPlaced { get; set; }
        private float m_moveTrigger;
        private float m_moveCounter;
        private float m_maxMoveTime;        // Maximum value for move timer below
        private float m_moveChangeTimer;    // Timer for moving
        private float m_timeUntilBomb;      // Timer for placing bombs
        private int randomBombThreshold;

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
        private Tilemap _tileMap;   // Reference to the tile map

        //AI state management
        private AI_States currentState = AI_States.Exploring;    // Initial state is Idle

        public Player(Vector2 startPos, Texture2D txr, int frameCount, int fps, bool isThisPlayer/*, Tilemap tileMap*/) : base(startPos, txr, frameCount, fps)
        {
            MaxBombs = 2;
            BombsPlaced = 0;
            m_moveTrigger = 0.06f;
            m_moveCounter = 0;
            m_moveChangeTimer = 0;
            m_maxMoveTime = 5;
            isPlayer = isThisPlayer;
            tileDestination = Position.ToPoint();
            playerCollision = new Rectangle((int)Position.X, (int)Position.Y + 1, 14, 14);
            randomBombThreshold = Game1.RNG.Next(3, 10);
        }

        public void Update(GameTime gameTime, Tilemap currentMap, KeyboardState kb, KeyboardState kbOld)
        {
            if (isPlayer)
            {
                if (kb.IsKeyDown(Keys.W))
                {
                    if (currentMap.IsWalkable(new Vector2((int)Position.X, (int)Position.Y - 2)))
                    {
                        Move(Direction.North);
                    }
                }
                if (kb.IsKeyDown(Keys.S))
                {
                    if (currentMap.IsWalkable(new Vector2((int)Position.X, (int)Position.Y + 2)))
                    {
                        Move(Direction.South);
                    }
                }
                if (kb.IsKeyDown(Keys.A))
                {
                    if (currentMap.IsWalkable(new Vector2((int)Position.X - 2, (int)Position.Y)))
                    {
                        Move(Direction.West);
                    }
                }
                if (kb.IsKeyDown(Keys.D))
                {
                    if (currentMap.IsWalkable(new Vector2((int)Position.X + 2, (int)Position.Y)))
                    {
                        Move(Direction.East);
                    }
                }
            }
            else        // AI PLAYERS
            {
                m_moveChangeTimer += (float)gameTime.ElapsedGameTime.TotalSeconds; // Timer for deciding to change direction
                m_moveCounter += (float)gameTime.ElapsedGameTime.TotalSeconds; // Timer for controlling movement intervals

                bool isBombNearby = CheckBombProximity();

                //if (isBombNearby)
                //    currentState = AI_States.RunningAway;
                //else if (DetectPlayerNearby())
                //    currentState = AI_States.SeekingPlayer;
                //else if (m_moveCounter > m_moveTrigger)
                //    currentState = AI_States.Exploring;
                //else
                //    currentState = AI_States.Idle;

                switch (currentState)
                {
                    case AI_States.Idle:
                        HandleIdleState();
                        break;
                    case AI_States.Exploring:
                        HandleExploringState(gameTime, currentMap);
                        break;
                    case AI_States.RunningAway:
                        HandleFleeingState();
                        break;
                    case AI_States.SeekingPlayer:
                        HandleSeekingState();
                        break;
                }
            }

            m_rectangle = new Rectangle((int)m_position.X, (int)m_position.Y, m_txr.Width, m_txr.Height);
        }

        private void HandleIdleState()
        {
            // Stand idle, fallback behaviour, do not place bombs, switch out to another behaviour when possible
            currentState = AI_States.Exploring;
        }

        private void HandleExploringState(GameTime gameTime, Tilemap currentMap)
        {
            Direction moveDir;
            bool canMove = true;

            if (m_moveCounter > m_moveTrigger)
            {
                m_moveCounter = 0;

                m_moveChangeTimer = m_maxMoveTime;

                //Timer for bomb place
                m_timeUntilBomb += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (m_timeUntilBomb > randomBombThreshold)                                      // There is a bug here, when the game first starts it takes forever before bombs start spawning, even if t = 3
                {
                    isDeciding = true;
                    m_timeUntilBomb = 0;
                    randomBombThreshold = Game1.RNG.Next(3, 10);
                }

                //Timer for moving into maze - this might be redundant
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
                Game1.GConsole.Log("Prev direction: " + _prevAiDrection);
                Vector2 newPoint = new Vector2((int)newX + _prevAiDrectionXInc, (int)newY + _prevAiDrectionYInc);

                int idx = Game1.RNG.Next(0, 4) * 3;

                // Check if the AI is stuck and just keep moving in its previous direction
                if (currentMap.IsWalkable(newPoint))
                {
                    Game1.GConsole.Log("Using prev dir");
                    // Skip the loop if we can still move the other direction
                    idx = possibleMoves.Length;
                    moveDir = _prevAiDrection;
                }

                // Generates a random number every update:
                int randomDecision = Game1.RNG.Next(0, 15); // Make the odds of new direction 1/15

                // If the random number is within the odds, change the AI's direction
                if (randomDecision < 1) // Set it as a 1 in 15 chance of new direction every update  --  Should also do AND (OR OR...) collision checks here to prevent wall bouncing
                {
                    // Reset idx to force the new direction
                    idx = Game1.RNG.Next(0, 4) * 3;
                }

                // Movement loop that chooses a valid move based on collision checks
                for (i = idx; i <= possibleMoves.Length - 3;)
                {
                    newPoint.X = possibleMoves[i];
                    newPoint.Y = possibleMoves[i + 1];
                    moveDir = (Direction)possibleMoves[i + 2];
                    if (currentMap.IsWalkable(newPoint))
                    {
                        if (currentMap.IsDanger(newPoint))
                        {
                            currentState = AI_States.RunningAway;
                            break;
                        }
                        break;
                    }

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
            
        }
        
        private void HandleFleeingState()
        {
            // Run away from bombs
            Position = new Vector2(100, 100);
        }

        private void HandleSeekingState()
        {
            // LOS
            // Place bomb when close enough
        }

        private bool DetectPlayerNearby()
        {
            return false; //Placeholder
        }
        private bool CheckBombProximity()
        {
            return false;   //Placeholder
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
