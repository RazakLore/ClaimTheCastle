using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ClaimTheCastle
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D mainMenuBackground, playerSelectBackground, lvlSelFrame, castlePreview, icePreview, lavaPreview;
        private SoundEffect explosionSFX, deathScream, teleportSFX; private Song menuTrack, toTheDeath;
        private Cam2D cam;
        private GameStates gameStates; private LevelSelectPreview lvlSelPreviews;

        int currentLevel = 1;                                               // 0 = The Castle, 1 = Ice Cave, 2 = Lava Zone
        private List<Tilemap> arenas;
        private TextureAtlas _textureAtlas;
        private UserInterface _userInterface;

        // List of gamepad states
        private List<Player> _players;                                      //Add on menu game start, need to remember who is player or not
        private List<GamePadState> _gamePadStates, _gamePadStatesOld;       // List of all players gamepad states and old states

        private List<Bomb> bombs;
        private List<GenericExplosion> genExplosions;
        private List<GenericUnsafeTileClear> genericUnsafeTileClears;
        private List<DangerSign> dangerSigns;                               // Add to this every time the bomb hits a wall it cannot pass
        private List<GenericExplosionAnimation> explodeAnims;               // Add to this list every tile that the danger tile crosses over
        private List<TeleportAnimation> teleAnims;
        int bombcount;

        KeyboardState kb, kbOld;
        public static SpriteFont debug;
        public static Random RNG = new Random();
        public static GameConsole GConsole;

        float recordFPS = 60; bool isRecord = true;
        float tileAnimationTimer;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferHeight = 720;
            _graphics.PreferredBackBufferWidth = 816;
        }

        protected override void Initialize()
        {
            #region Debug
            // Create a console (only causes the constructor to be called.)
            GConsole = new GameConsole(this, new Point(256, 512), ConsoleLocation.BottomRight)
            {
                FadeAfterEvent = false,
                ShowTimeStamps = true,
                LogLogging = false,
            };
            // Register it as a component so that its own Initialise, Update and Draw will be called when appropriate.
            Components.Add(GConsole);
            #endregion

            base.Initialize();
            gameStates = GameStates.Title; lvlSelPreviews = LevelSelectPreview.Castle;
            
            cam.Position = new Vector2(0, 0);

            #region Lists
            arenas = new List<Tilemap>();

            bombs = new List<Bomb>();
            bombcount = 0;
            
            genExplosions = new List<GenericExplosion>();
            genericUnsafeTileClears = new List<GenericUnsafeTileClear>();
            explodeAnims = new List<GenericExplosionAnimation>();
            dangerSigns = new List<DangerSign>();
            teleAnims = new List<TeleportAnimation>();

            _players = new List<Player>();
            _gamePadStates = new List<GamePadState>();
            _gamePadStatesOld = new List<GamePadState>();
            #endregion

            for (int i = 0; i < 3; i++)
            {
                arenas.Add(new Tilemap($"Content/Levels/level{i}", Vector2.Zero, 17, 16));
            }
            #region Assets
            _textureAtlas = new TextureAtlas(Content.Load<Texture2D>("TextureAtlas"), 8, 6, 16, 16);
            _userInterface = new UserInterface(Content.Load<Texture2D>("UI/UISpriteSheet"), Content.Load<Texture2D>("UI/TransparentOverlay"), Content.Load<Texture2D>("UI/ConfirmLevelButton"), Content.Load<Texture2D>("UI/StartButton"), Content.Load<Texture2D>("UI/ExitButton"), Content.Load<Texture2D>("UI/Player1Sel"), Content.Load<Texture2D>("UI/Player2Sel"), Content.Load<Texture2D>("UI/Player3Sel"), Content.Load<Texture2D>("UI/Player4Sel"), Content.Load<Texture2D>("UI/KbAvailableTick"), Content.Load<Texture2D>("UI/KbUnavailableCross"));
            explosionSFX = Content.Load<SoundEffect>("Sounds/CauldronExplosion"); deathScream = Content.Load<SoundEffect>("Sounds/DeathScream"); teleportSFX = Content.Load<SoundEffect>("Sounds/ZoomSFX");
            menuTrack = Content.Load<Song>("Sounds/MenuTrack"); toTheDeath = Content.Load<Song>("Sounds/ToTheDeath");
            #endregion
            #region Load Players
            _players.Add(new Player(new Vector2(2 * 16, 2 * 16), Content.Load<Texture2D>("Actors/BlueWizardSpritesheet"), 4, 8, false, 0, arenas[currentLevel]));
            _players.Add(new Player(new Vector2(14 * 16, 2 * 16), Content.Load<Texture2D>("Actors/RedWizardSpritesheet"), 4, 8, false, 0, arenas[currentLevel]));
            _players.Add(new Player(new Vector2(2 * 16, 12 * 16), Content.Load<Texture2D>("Actors/GreenWizardSpriteSheet"), 4, 8, false, 0, arenas[currentLevel]));
            _players.Add(new Player(new Vector2(14 * 16, 12 * 16), Content.Load<Texture2D>("Actors/YellowWizardSpriteSheet"), 4, 8, false, 0, arenas[currentLevel]));
            #endregion

            for (int i = 0; i < _players.Count; i++)
            {
                _gamePadStates.Add(GamePad.GetState(i));
                _gamePadStatesOld.Add(GamePad.GetState(i));
            }

            debug = Content.Load<SpriteFont>("Debug");
            tileAnimationTimer = 0;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            mainMenuBackground = Content.Load<Texture2D>("UI/TitleScreen"); playerSelectBackground = Content.Load<Texture2D>("UI/PlayerSelectScreen");
            lvlSelFrame = Content.Load<Texture2D>("UI/LevelSelectFrame"); lavaPreview = Content.Load<Texture2D>("UI/LavaZonePreview"); //163 144
            castlePreview = Content.Load<Texture2D>("UI/castlePreview"); icePreview = Content.Load<Texture2D>("UI/icePreview");
        }

        protected override void Update(GameTime gameTime)
        {
            kb = Keyboard.GetState();
            for (int i = 0; i < _gamePadStates.Count; i++)  // For all controllers, get state
            {
                _gamePadStates[i] = GamePad.GetState(i);
            }

            _userInterface.Update(gameTime, Mouse.GetState(), kb, gameStates);

            switch(gameStates)
            {
                case GameStates.Title:
                    HandleTitle();
                    break;

                case GameStates.PlayerSelect:
                    HandlePlayerSelect();
                    break;

                case GameStates.LevelSelect:
                    HandleLevelSelect(gameTime);
                    break;

                case GameStates.Options:
                    break;

                case GameStates.Gameplay:
                    HandleGameplay(gameTime);
                    break;
            }

            base.Update(gameTime);
            kbOld = kb;
            for (int i = 0; i < _gamePadStatesOld.Count; i++)   // Set all old state for all controllers
            {
                _gamePadStatesOld[i] = _gamePadStates[i];
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            float fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (currentLevel == 0)                          //Set the level's background colour for explosions
                GraphicsDevice.Clear(Color.Gray);
            else if (currentLevel == 1)
                GraphicsDevice.Clear(Color.LightBlue);
            else if (currentLevel == 2)
                GraphicsDevice.Clear(Color.DarkOrange);
            else    
                GraphicsDevice.Clear(Color.Black);

            switch(gameStates)
            {
                case GameStates.Title:
                    _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
                    _spriteBatch.Draw(mainMenuBackground, Vector2.Zero, Color.White);       //Title screen
                    _userInterface.Draw(_spriteBatch, _players[0].HealthPoints, _players[1].HealthPoints, _players[2].HealthPoints, _players[3].HealthPoints, _players);
                    _spriteBatch.End();
                    break;

                case GameStates.PlayerSelect:
                    _userInterface.UserInterfaceState = Interface.PlayerSelect;
                    _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
                    _spriteBatch.Draw(playerSelectBackground, Vector2.Zero, Color.White);   // Player select screen
                    _userInterface.Draw(_spriteBatch, _players[0].HealthPoints, _players[1].HealthPoints, _players[2].HealthPoints, _players[3].HealthPoints, _players);
                    _spriteBatch.End();
                    break;

                case GameStates.LevelSelect:
                    _userInterface.UserInterfaceState = Interface.LevelSelect;
                    _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, null);
                    _spriteBatch.Draw(lvlSelFrame, Vector2.Zero, Color.White);
                    switch (lvlSelPreviews)
                    {
                        case LevelSelectPreview.Castle:
                            _spriteBatch.DrawString(debug, "The Castle", new Vector2(816 / 2.5f, 40), Color.Black);
                            _spriteBatch.Draw(castlePreview, new Vector2(163, 144), Color.White);
                            break;

                        case LevelSelectPreview.Ice:
                            _spriteBatch.DrawString(debug, "Frigid Halls", new Vector2(816 / 2.5f, 40), Color.Black);
                            _spriteBatch.Draw(icePreview, new Vector2(163, 144), Color.White);
                            break;

                        case LevelSelectPreview.Lava:
                            _spriteBatch.DrawString(debug, "Lava Zone", new Vector2(816 / 2.5f, 40), Color.Black);
                            _spriteBatch.Draw(lavaPreview, new Vector2(163, 144), Color.White);
                            break;
                    }
                    _spriteBatch.DrawString(debug, "PRESS SPACE TO BEGIN", new Vector2(816 / 2.5f, 640), Color.Black);
                    _userInterface.Draw(_spriteBatch, _players[0].HealthPoints, _players[1].HealthPoints, _players[2].HealthPoints, _players[3].HealthPoints, _players);
                    _spriteBatch.End();
                    break;

                case GameStates.Options:

                    break;

                case GameStates.Gameplay:
                    _userInterface.UserInterfaceState = Interface.Gameplay;
                    _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cam.getCam()); // Original spritebatch

                    arenas[currentLevel].Draw(_spriteBatch, _textureAtlas);

                    for (int i = 0; i < bombs.Count; i++)
                        bombs[i].Draw(_spriteBatch);
                    #region Foreach loops
                    foreach (var player in _players)
                        player.Draw(_spriteBatch, gameTime, 16, 16);

                    foreach (var danger in dangerSigns)
                        danger.Draw(_spriteBatch);

                    foreach (var animatedExplode in explodeAnims)
                        animatedExplode.Draw(_spriteBatch);

                    foreach (var animatedTeleport in teleAnims)
                        animatedTeleport.Draw(_spriteBatch);
                    #endregion

                    _userInterface.Draw(_spriteBatch, _players[0].HealthPoints, _players[1].HealthPoints, _players[2].HealthPoints, _players[3].HealthPoints, _players);
                    if (_userInterface.IsGameOver)
                        _spriteBatch.DrawString(debug, "  Player " + FindTheWinner() + " won the game!\n\n  Press ENTER to\n return to Title.", new Vector2(55, 110), Color.White);

                    if (fps < 58 && isRecord && gameTime.TotalGameTime.TotalSeconds > 4 && bombs.Count > 2)
                    {
                        recordFPS = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
                        //isRecord = false; // Debug
                    }
                    _spriteBatch.End();
                    break;
            }
            base.Draw(gameTime);
        }

        struct Cam2D
        {
            public Vector2 Position; // Camera's Position

            public Matrix getCam()  // Function to get the camera's values
            {
                Matrix temp;
                temp = Matrix.CreateTranslation(new Vector3(Position.X, Position.Y, 0));
                temp *= Matrix.CreateScale(3);  // Zooms in by 2x
                return temp;
            }
        }

        private void HandleTitle()
        {
            bool isGamePadAPressed = false;
            Rectangle exitRectWorkaround = new Rectangle(573, 505, 214, 42);
            if (MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Play(menuTrack);
                MediaPlayer.IsRepeating = true;
            }

            #region Buttons
            for (int i = 0;  i < _gamePadStates.Count; i++)
            {
                if (_userInterface.titleButtonState == TitleButtonState.Start && _gamePadStates[i].IsButtonDown(Buttons.A) && _gamePadStatesOld[i].IsButtonUp(Buttons.A))
                    isGamePadAPressed = true;
                if (_gamePadStates[i].IsButtonDown(Buttons.DPadDown) && _gamePadStatesOld[i].IsButtonUp(Buttons.DPadDown))
                    _userInterface.titleButtonState++;
                if (_gamePadStates[i].IsButtonDown(Buttons.DPadUp) && _gamePadStatesOld[i].IsButtonUp(Buttons.DPadUp))
                    _userInterface.titleButtonState--;
                if (_userInterface.titleButtonState == TitleButtonState.Exit && _gamePadStates[i].IsButtonDown(Buttons.A) && _gamePadStatesOld[i].IsButtonUp(Buttons.A))
                    Exit();
            }

            if (_userInterface.titleButtonState < 0)
                _userInterface.titleButtonState = (TitleButtonState)2;
            if (_userInterface.titleButtonState > (TitleButtonState)2)
                _userInterface.titleButtonState = 0;
            
            if (exitRectWorkaround.Contains(Mouse.GetState().Position) && Mouse.GetState().LeftButton == ButtonState.Pressed)
                Exit();
            #endregion

            if (kb.IsKeyDown(Keys.Space) && kbOld.IsKeyUp(Keys.Space) || isGamePadAPressed || _userInterface.IsStartPressed)
            {
                _userInterface.CanPickKeyboard = true;
                foreach (var player in _players)
                {
                    player.ControlType = 3;
                    player.IsPlayer = false;
                }
                gameStates = GameStates.PlayerSelect;
                _userInterface.IsStartPressed = false;
                isGamePadAPressed = false;
            }
        }
        private void HandlePlayerSelect()
        {
            bool[] gamepadAssigned = new bool[_gamePadStates.Count];    // Creates a bool to track if a gamepad has already been assigned to a player during player selection
            #region Gamestate stuff
            if (kb.IsKeyDown(Keys.Escape) && kbOld.IsKeyUp(Keys.Escape))    // Previous state
            {
                gameStates = GameStates.Title;
                _userInterface.UserInterfaceState = Interface.Title;
            }
            for (int i = 0;  i < _gamePadStates.Count; i++)
            {
                if (_gamePadStates[i].IsButtonDown(Buttons.B) && _gamePadStatesOld[i].IsButtonUp(Buttons.B))    // Previous state but controller
                {
                    gameStates = GameStates.Title;
                    _userInterface.UserInterfaceState = Interface.Title;
                }
                if (_gamePadStates[i].IsButtonDown(Buttons.Start) && _gamePadStatesOld[i].IsButtonUp(Buttons.Start))    // Next state but controller
                {
                    gameStates = GameStates.LevelSelect;
                    _userInterface.UserInterfaceState= Interface.LevelSelect;
                }
            }
            if (kb.IsKeyDown(Keys.Space) && kbOld.IsKeyUp(Keys.Space))  // Next state
                gameStates = GameStates.LevelSelect;
            #endregion
            #region Player controls binding
            if (!_players[0].IsPlayer)
            {
                if (kb.IsKeyDown(Keys.Enter) && kbOld.IsKeyUp(Keys.Enter))
                {
                    _players[0].IsPlayer = true;
                    _players[0].ControlType = 0;
                    _userInterface.CanPickKeyboard = false;
                }

                for (int gamepadIndex = 0; gamepadIndex < _gamePadStates.Count; gamepadIndex++)
                {
                    GamePadState gp = _gamePadStates[gamepadIndex];             // Assigns gamepadstates to shorter name
                    GamePadState gpOld = _gamePadStatesOld[gamepadIndex];

                    if (!gamepadAssigned[gamepadIndex] && (gp.Buttons.A == ButtonState.Pressed && gpOld.Buttons.A == ButtonState.Released))
                    {
                        _players[0].IsPlayer = true;                        // Sets this player's update logic to playable logic rather than AI
                        _players[0].ControlType = 2;                        // Control type 2 is gamepad
                        _players[0].AssignedGamepadIndex = gamepadIndex;    // Assign this gamepad number to this player so that they respond to that one only
                        gamepadAssigned[gamepadIndex] = true;               // Add this numbered controller to the bool list so the other controllers may not steal it
                        break;
                    }
                }
            }
            if (!_players[1].IsPlayer && _players[0].IsPlayer)  // Only when player 1 has selected should player 2 get the option to join
            {
                if (kb.IsKeyDown(Keys.Enter) && kbOld.IsKeyUp(Keys.Enter) && _players[0].ControlType != 0)
                {
                    _players[1].IsPlayer = true;
                    _players[1].ControlType = 0;
                    _userInterface.CanPickKeyboard = false;
                }
                for (int gamepadIndex = 0; gamepadIndex < _gamePadStates.Count; gamepadIndex++)
                {
                    GamePadState gp = _gamePadStates[gamepadIndex];
                    GamePadState gpOld = _gamePadStatesOld[gamepadIndex];

                    if (!gamepadAssigned[gamepadIndex] && (gp.Buttons.A == ButtonState.Pressed && gpOld.Buttons.A == ButtonState.Released))
                    {
                        _players[1].IsPlayer = true;
                        _players[1].ControlType = 2;
                        _players[1].AssignedGamepadIndex = gamepadIndex;
                        gamepadAssigned[gamepadIndex] = true;
                        break;
                    }
                }
            }
            if (!_players[2].IsPlayer && _players[1].IsPlayer)  // Only when player 2 has selected should player 3 get the option to join
            {
                if (kb.IsKeyDown(Keys.Enter) && kbOld.IsKeyUp(Keys.Enter) && _players[0].ControlType != 0 && _players[1].ControlType != 0)
                {
                    _players[2].IsPlayer = true;
                    _players[2].ControlType = 0;
                    _userInterface.CanPickKeyboard = false;
                }
                for (int gamepadIndex = 0; gamepadIndex < _gamePadStates.Count; gamepadIndex++)
                {
                    GamePadState gp = _gamePadStates[gamepadIndex];
                    GamePadState gpOld = _gamePadStatesOld[gamepadIndex];

                    if (!gamepadAssigned[gamepadIndex] && (gp.Buttons.A == ButtonState.Pressed && gpOld.Buttons.A == ButtonState.Released))
                    {
                        bool gamepadAlreadyAssigned = false;
                        foreach (var player in _players)
                        {
                            if (player.AssignedGamepadIndex == gamepadIndex)
                            {
                                gamepadAlreadyAssigned = true;
                                break;
                            }
                        }
                        if (gamepadAlreadyAssigned)
                            continue;

                        _players[2].IsPlayer = true;
                        _players[2].ControlType = 2;
                        _players[2].AssignedGamepadIndex = gamepadIndex;
                        gamepadAssigned[gamepadIndex] = true;
                        break;
                    }
                }
            }
            if (!_players[3].IsPlayer && _players[2].IsPlayer)  // Only when player 3 has selected should player 4 get the option to join
            {
                if (kb.IsKeyDown(Keys.Enter) && kbOld.IsKeyUp(Keys.Enter) && _players[0].ControlType != 0 && _players[1].ControlType != 0)
                {
                    _players[2].IsPlayer = true;
                    _players[2].ControlType = 0;
                    _userInterface.CanPickKeyboard = false;
                }
                for (int gamepadIndex = 0; gamepadIndex < _gamePadStates.Count; gamepadIndex++)
                {
                    GamePadState gp = _gamePadStates[gamepadIndex];
                    GamePadState gpOld = _gamePadStatesOld[gamepadIndex];

                    if (!gamepadAssigned[gamepadIndex] && (gp.Buttons.A == ButtonState.Pressed && gpOld.Buttons.A == ButtonState.Released))
                    {
                        bool gamepadAlreadyAssigned = false;
                        foreach (var player in _players)
                        {
                            if (player.AssignedGamepadIndex == gamepadIndex)
                            {
                                gamepadAlreadyAssigned = true;
                                break;
                            }
                        }
                        if (gamepadAlreadyAssigned)
                            continue;

                        _players[2].IsPlayer = true;
                        _players[2].ControlType = 2;
                        _players[2].AssignedGamepadIndex = gamepadIndex;
                        gamepadAssigned[gamepadIndex] = true;
                        break;
                    }
                }
            }
            #endregion
        }
        private void HandleLevelSelect(GameTime gameTime)
        {
            _userInterface.arrowTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            #region Keyboard controls
            if (kb.IsKeyDown(Keys.Escape) && kbOld.IsKeyUp(Keys.Escape))    // Previous state
            {
                gameStates = GameStates.PlayerSelect;
                _userInterface.UserInterfaceState = Interface.PlayerSelect;
            }
            if (kb.IsKeyDown(Keys.A) && kbOld.IsKeyUp(Keys.A))   // If gamepad left or if left arrow button pressed
            {
                lvlSelPreviews -= 1;  //Do button left
                _userInterface.arrowState = ArrowState.LeftSelected; _userInterface.arrowTimer = 0;
            }
            if(kb.IsKeyDown(Keys.D) && kbOld.IsKeyUp(Keys.D))   // If gamepad right or if right arrow button pressed
            {
                lvlSelPreviews += 1;  //Do button right
                _userInterface.arrowState = ArrowState.RightSelected; _userInterface.arrowTimer = 0;
            }
            if(kb.IsKeyDown(Keys.Enter) && kbOld.IsKeyUp(Keys.Enter) || kb.IsKeyDown(Keys.Space) && kbOld.IsKeyUp(Keys.Space))  // Or GamePad button A or if Select Button pressed
            {
                LoadLevelData();
            }
            #endregion
            #region Gamepad controls
            for (int i = 0; i < _gamePadStates.Count; i++)
            {
                if (_gamePadStates[i].IsButtonDown(Buttons.A) && _gamePadStatesOld[i].IsButtonUp(Buttons.A))
                    LoadLevelData();
                if (_gamePadStates[i].IsButtonDown(Buttons.DPadLeft) && _gamePadStatesOld[i].IsButtonUp(Buttons.DPadLeft))
                {
                    lvlSelPreviews -= 1;  //Do button left
                    _userInterface.arrowState = ArrowState.LeftSelected; _userInterface.arrowTimer = 0;
                }
                if (_gamePadStates[i].IsButtonDown(Buttons.DPadRight) && _gamePadStatesOld[i].IsButtonUp(Buttons.DPadRight))
                {
                    lvlSelPreviews += 1;  //Do button right
                    _userInterface.arrowState = ArrowState.RightSelected; _userInterface.arrowTimer = 0;
                }
                if (_gamePadStates[i].IsButtonDown(Buttons.B) && _gamePadStatesOld[i].IsButtonUp(Buttons.B))    // Previous state
                {
                    gameStates = GameStates.PlayerSelect;
                    _userInterface.UserInterfaceState = Interface.PlayerSelect;
                }
            }
            #endregion
            if ((int)lvlSelPreviews < 0)
                lvlSelPreviews = LevelSelectPreview.Lava;   // To loop around previews
            if ((int)lvlSelPreviews > 2)
                lvlSelPreviews = LevelSelectPreview.Castle;
        }
        private void HandleGameplay(GameTime gameTime)
        {
            BombSpawning(gameTime);

            #region For loops
            for (int i = 0; i < bombs.Count; i++)
            {
                bombs[i].Update(gameTime);
                HandleBombDeath(i, gameTime);
            }

            foreach (var explosion in genExplosions)               // Update the general explosions for tile deletion
                explosion.TileExplosionAnimate(gameTime);

            for (int i = 0; i < explodeAnims.Count; i++)
            {
                explodeAnims[i].Animate(gameTime);
                if (explodeAnims[i].TimeToDie)
                    explodeAnims.RemoveAt(i);
            }       //Update the explosion animations on empty tiles then delete when finished
                
            for (int i = 0; i < teleAnims.Count; i++)
            {
                teleAnims[i].Animate(gameTime);
                if (teleAnims[i].TimeToDie)
                    teleAnims.RemoveAt(i);
            }

            AnimateLevelTiles(gameTime);
            for (int i = 0; i < dangerSigns.Count; i++)
            {
                dangerSigns[i].Countdown(gameTime);
                if (dangerSigns[i].TimeToDie)
                    dangerSigns.RemoveAt(i);
            }        // Count the danger sign lifespans to 0 then delete when reached

            for (int i = 0; i < _players.Count; i++)              // Update the players when not dead and move them to new location and revoke control when they are
            {
                if (_players[i] != null && !_players[i].IsDead)
                {
                    _players[i].Update(gameTime, arenas[currentLevel], bombs, kb, kbOld, _gamePadStates[_players[i].AssignedGamepadIndex], _gamePadStatesOld[_players[i].AssignedGamepadIndex]);
                }
                if (_players[i].HealthPoints <= 0)
                {
                    _players[i].IsDead = true;
                    _players[i].Position = new Vector2(300, 300);
                }
            }
            #endregion
            if (AreThreeDead()) // Detect if there is a winner
            {
                _userInterface.IsGameOver = true;
                if (kb.IsKeyDown(Keys.Enter) && kbOld.IsKeyUp(Keys.Enter))
                {
                    gameStates = GameStates.Title;
                    _userInterface.IsGameOver = false;
                    _userInterface.UserInterfaceState = Interface.Title;
                    MediaPlayer.Stop(); MediaPlayer.Play(menuTrack);
                }
                for (int i = 0; i < _gamePadStates.Count; i++)
                {
                    if (_gamePadStates[i].IsButtonDown(Buttons.A) && _gamePadStatesOld[i].IsButtonUp(Buttons.A))
                    {
                        gameStates = GameStates.Title;
                        _userInterface.IsGameOver = false;
                        _userInterface.UserInterfaceState = Interface.Title;
                        MediaPlayer.Stop(); MediaPlayer.Play(menuTrack);
                    }
                }
            }
        }

        private bool AreThreeDead()
        {
            int deadCount = _players.Count(p => p.IsDead);
            return deadCount >= 3;                              // If 3 people have died, return true
        }
        internal int FindTheWinner()
        {
            var winner = _players.FirstOrDefault(p => !p.IsDead);
            if (winner != null)
                return _players.IndexOf(winner) + 1;    // Return the index of the player
            return -1;                              // Default return for if no winner is found
        }
        private bool IsThisBombNearPlayer(Point playerPosition, Point bombPosition, int bombRadius)
        {
            //Bomb position is in tile coordinates, so (2, 2) for example. Player is in screen coordinates, so (192, 32) for example. We must divide the player position
            //by the width and height of the tiles so that their values accurately reflect each other. Bomb radius follows the same logic, as it is the value of tiles (3) for example.
            int playerXPos = playerPosition.X /= 16;
            int playerYPos = playerPosition.Y /= 16;
            if (playerXPos >= bombPosition.X && playerXPos < bombPosition.X + 1 &&
                playerYPos >= -bombRadius + bombPosition.Y &&
                playerYPos <= bombRadius + bombPosition.Y)
            {
                // This should be checking if bomb X coordinate is same as player X coordinate, then checking if its in the radius above or below the bombs position 
                return true;
            }
            if (playerYPos >= bombPosition.Y && playerYPos < bombPosition.Y + 1 &&
                playerXPos >= -bombRadius + bombPosition.X &&
                playerXPos <= bombRadius + bombPosition.X)
            {
                return true;
            }
            else
                return false;
        }

        private bool IsThisBombNearAnotherBomb(Point thisBombPosition, Point explodingBombPosition, int bombRadius)
        {
            //Bomb position is in tile coordinates, so (2, 2) for example. Player is in screen coordinates, so (192, 32) for example. We must divide the player position
            //by the width and height of the tiles so that their values accurately reflect each other. Bomb radius follows the same logic, as it is the value of tiles (3) for example.
            if (thisBombPosition.X >= explodingBombPosition.X && thisBombPosition.X < explodingBombPosition.X + 1 &&
                thisBombPosition.Y >= -bombRadius + explodingBombPosition.Y &&
                thisBombPosition.Y <= bombRadius + explodingBombPosition.Y)
            {
                // This should be checking if bomb X coordinate is same as player X coordinate, then checking if its in the radius above or below the bombs position 
                return true;
            }
            if (thisBombPosition.Y >= explodingBombPosition.Y && thisBombPosition.Y < explodingBombPosition.Y + 1 &&
                thisBombPosition.X >= -bombRadius + explodingBombPosition.X &&
                thisBombPosition.X <= bombRadius + explodingBombPosition.X)
            {
                return true;
            }
            else
                return false;
        }
        private void BombSpawning(GameTime gameTime)
        {
            for (int i = 0; i < _players.Count; i++)        // Handle the logic for spawning bombs
            {
                if (_players[i] != null && _players[i].IsPlayer && _players[i].BombsPlaced <= _players[i].MaxBombs)
                {
                    if (_players[i].ControlType == 0)
                    {
                        if (kb.IsKeyDown(Keys.Space) && kbOld.IsKeyUp(Keys.Space))
                        {
                            bombs.Add(new Bomb(new Point(_players[i].Position.ToPoint().X / 16, _players[i].Position.ToPoint().Y / 16), 3, 3, arenas[currentLevel], Content.Load<Texture2D>("Actors/CauldronSpriteSheet"), i, 
                                genExplosions, genericUnsafeTileClears, explodeAnims, Content.Load<Texture2D>("Actors/ExplodeAnim"), dangerSigns, Content.Load<Texture2D>("Actors/DangerSign"), gameTime));
                            _players[i].BombsPlaced++;
                        }
                    }
                    if (_players[i].ControlType == 1)
                    {
                        if (kb.IsKeyDown(Keys.RightControl) && kbOld.IsKeyUp(Keys.RightControl))
                        {
                            bombs.Add(new Bomb(new Point(_players[i].Position.ToPoint().X / 16, _players[i].Position.ToPoint().Y / 16), 3, 3, arenas[currentLevel], Content.Load<Texture2D>("Actors/CauldronSpriteSheet"), i, 
                                genExplosions, genericUnsafeTileClears, explodeAnims, Content.Load<Texture2D>("Actors/ExplodeAnim"), dangerSigns, Content.Load<Texture2D>("Actors/DangerSign"), gameTime));
                            _players[i].BombsPlaced++;
                        }
                    }
                    if (_players[i].ControlType == 2)
                    {
                        if (_gamePadStates[_players[i].AssignedGamepadIndex].Buttons.A == ButtonState.Pressed && _gamePadStatesOld[_players[i].AssignedGamepadIndex].Buttons.A == ButtonState.Released)
                        {
                            bombs.Add(new Bomb(new Point(_players[i].Position.ToPoint().X / 16, _players[i].Position.ToPoint().Y / 16), 3, 3, arenas[currentLevel], Content.Load<Texture2D>("Actors/CauldronSpriteSheet"), i, 
                                genExplosions, genericUnsafeTileClears, explodeAnims, Content.Load<Texture2D>("Actors/ExplodeAnim"), dangerSigns, Content.Load<Texture2D>("Actors/DangerSign"), gameTime));
                            _players[i].BombsPlaced++;
                        }
                    }
                }
                else if (_players[i] != null && _players[i].isDeciding && _players[i].BombsPlaced <= _players[i].MaxBombs)
                {
                    bombs.Add(new Bomb(new Point(_players[i].Position.ToPoint().X / 16, _players[i].Position.ToPoint().Y / 16), 3, 3, arenas[currentLevel], Content.Load<Texture2D>("Actors/CauldronSpriteSheet"), i,
                                genExplosions, genericUnsafeTileClears, explodeAnims, Content.Load<Texture2D>("Actors/ExplodeAnim"), dangerSigns, Content.Load<Texture2D>("Actors/DangerSign"), gameTime));
                    _players[i].BombsPlaced++;
                    _players[i].isDeciding = false;
                }
            }
        }
        private void HandleBombDeath(int i, GameTime gameTime)
        {
            if (bombs[i].TimeToDie)
            {
                for (int j = 0; j < _players.Count; j++)
                {
                    if (bombs[i].PlayerOwner == j)
                        _players[j].BombsPlaced--;

                    if (_players[j]._tileMap.IsDanger(_players[j].Position) && IsThisBombNearPlayer(_players[j].Position.ToPoint(), bombs[i].Position, bombs[i].ExplosionRadius))
                    {
                        teleAnims.Add(new TeleportAnimation(Content.Load<Texture2D>("Actors/TeleportAnim"), _players[j].Position.ToPoint()));
                        teleportSFX.Play();
                        int whatPlace = RNG.Next(0, 4);         // This is somewhat buggy, the respawn doesnt always trigger
                        Vector2 newPos = Vector2.Zero;
                        switch (whatPlace)
                        {
                            case 0:
                                newPos = new Vector2(2 * 16, 2 * 16);
                                if (arenas[currentLevel].IsDanger(newPos))
                                    whatPlace = RNG.Next(0, 4);
                                break;
                            case 1:
                                newPos = new Vector2(14 * 16, 2 * 16);
                                if (arenas[currentLevel].IsDanger(newPos))
                                    whatPlace = RNG.Next(0, 4);
                                break;
                            case 2:
                                newPos = new Vector2(2 * 16, 12 * 16);
                                if (arenas[currentLevel].IsDanger(newPos))
                                    whatPlace = RNG.Next(0, 4);
                                break;
                            case 3:
                                newPos = new Vector2(14 * 16, 12 * 16);
                                if (arenas[currentLevel].IsDanger(newPos))
                                    whatPlace = RNG.Next(0, 4);
                                break;
                        }
                        _players[j].Position = newPos;
                        _players[j].HealthPoints -= 1;
                        deathScream.Play();
                    }
                }

                foreach (var bomb in bombs)     // CASCADING CAULDRON EXPLOSIONS
                {
                    if (bomb._tileMap.IsDanger(new Vector2(bomb.Position.X * 16, bomb.Position.Y * 16)) && IsThisBombNearAnotherBomb(bomb.Position, bombs[i].Position, bombs[i].ExplosionRadius))
                    {
                        bomb.TimeToExplode = 0.15f;
                    }
                }

                foreach (var unsafeTile in genericUnsafeTileClears)
                {
                    if (unsafeTile.Position == bombs[i].Position)
                        unsafeTile.ClearTileExecutor(gameTime);
                }
                bombs.RemoveAt(i);
                explosionSFX.Play();
                bombcount -= 1;
            }
        }
        private void AnimateLevelTiles(GameTime gameTime)
        {
            tileAnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds; // Animate any tiles that need animating
            if (tileAnimationTimer > 0.5f)
            {
                for (int x = 0; x < arenas[currentLevel].Dimensions; x++)
                {
                    for (int y = 0; y < arenas[currentLevel].Dimensions; y++)
                    {
                        if (arenas[currentLevel].TileData[x, y] == 17)          // If tile is lava 1, set to lava 2
                        {
                            arenas[currentLevel].TileData[x, y] = 23;
                        }
                        else if (arenas[currentLevel].TileData[x, y] == 23)
                        {
                            arenas[currentLevel].TileData[x, y] = 17;
                        }
                    }
                }
                tileAnimationTimer = 0;
            }
        }

        private void LoadLevelData()
        {
            currentLevel = (int)lvlSelPreviews;
            gameStates = GameStates.Gameplay;
            _userInterface.UserInterfaceState = Interface.Gameplay;
            _players[0].Position = new Vector2(2 * 16, 2 * 16); _players[1].Position = new Vector2(14 * 16, 2 * 16); _players[2].Position = new Vector2(2 * 16, 12 * 16);
            _players[3].Position = new Vector2(14 * 16, 12 * 16);
            bombs.Clear();
            dangerSigns.Clear();
            explodeAnims.Clear();
            MediaPlayer.Stop(); MediaPlayer.Play(toTheDeath);

            foreach (var player in _players)
            {
                player._tileMap = arenas[currentLevel];
                player.BombsPlaced = 0;
                player.IsDead = false;                  // What happens in this area that makes walls undetectable to the AI and bombs do no damage??
                player.HealthPoints = 3;
            }
            arenas[(int)lvlSelPreviews].RandomiseDestructibleWalls($"Content/Levels/level{(int)lvlSelPreviews}");
        }
    }
    enum GameStates
    {
        Title,
        PlayerSelect,
        LevelSelect,
        Options,
        Gameplay
    }
    enum LevelSelectPreview
    {
        Castle,
        Ice,
        Lava
    }
}



//Checklist

/*
 * Player Functionality                                                     Done
 * Tilemap                                                                  Done
 * Cauldron Spawning                                                        Done
 * Destructible Tiles - Indestructible Tiles                                Done
 * Powerups                                                                 Not done
 * Title Screen                                                             Done
 * Menu                                                                     Done
 * Game Over Screen                                                         Done
 */

// Notes

/*
 * Cauldrons start with 3 tile max reach                                    Done
 * Cauldrons go in 4 directions only                                        Done
 * Cauldrons have 3 second timer                                            Done
 * 2 Cauldrons max to start with                                            Done
 * Cauldrons immediately explode if hit by explosion                        Done
 * Potion Powerup will increase bomb reach                                  Not done
 * Potion Cauldron + will increase max cauldron cap                         Not done
 * Potion Feather will increase player speed                                Not done
 * Start with 3 shields                                                     Done
 * Explosions remove a shield and teleport player to a safe location        Done
 * Brick walls should explode when bombed, but block the blast              Done
 */