using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace ClaimTheCastle
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D temp, back, lvlSelFrame, lavaPreview;
        private Vector2 player;
        private Cam2D cam;
        private GameStates gameStates; private LevelSelectPreview lvlSelPreviews;

        int currentLevel = 2;                       // 0 = The Castle, 1 = Ice Cave, 2 = Lava Zone
        private List<Tilemap> arenas;
        private TextureAtlas _textureAtlas;

        private Player player1, player2, player3, player4;
        private List<Player> _players; //Add on menu game start, need to remember who is player or not

        private List<Bomb> bombs;
        private List<GenericExplosion> genExplosions;
        private List<GenericUnsafeTileClear> genericUnsafeTileClears;
        int bombcount;

        Effect wizardRed, wizardGreen, wizardYellow;

        KeyboardState kb, kbOld;
        public static SpriteFont debug;
        public static Random RNG = new Random();

        float recordFPS = 60; bool isRecord = true;
        float tileAnimationTimer;

        public static GameConsole GConsole;

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
            // Create a console (only causes the constructor to be called.)
            GConsole = new GameConsole(this, new Point(256, 512), ConsoleLocation.BottomRight)
            {
                FadeAfterEvent = false,
                ShowTimeStamps = true,
                LogLogging = false,
            };
            // Register it as a component so that its own Initialise, Update and Draw will be called when appropriate.
            Components.Add(GConsole);

            base.Initialize();
            gameStates = GameStates.Gameplay; lvlSelPreviews = LevelSelectPreview.Lava;
            
            cam.Position = new Vector2(0, 0);
            player = new Vector2 (50, 50);

            arenas = new List<Tilemap>();

            bombs = new List<Bomb>();
            bombcount = 0;

            genExplosions = new List<GenericExplosion>();

            genericUnsafeTileClears = new List<GenericUnsafeTileClear>();

            for (int i = 0; i < 3; i++)
            {
                arenas.Add(new Tilemap($"Content/Levels/level{i}", Vector2.Zero, 17, 16));
            }

            _textureAtlas = new TextureAtlas(Content.Load<Texture2D>("TextureAtlas"), 8, 6, 16, 16);
            #region Load Players
            player1 = new Player(new Vector2(2 * 16, 2 * 16), Content.Load<Texture2D>("Actors/WizardSpriteSheet"), 4, 8, true);
            player2 = new Player(new Vector2(14 * 16, 2 * 16), Content.Load<Texture2D>("Actors/WizardSpriteSheet"), 4, 8, false);
            player3 = new Player(new Vector2(2 * 16, 12 * 16), Content.Load<Texture2D>("Actors/WizardSpriteSheet"), 4, 8, false);
            player4 = new Player(new Vector2(14 * 16, 12 * 16), Content.Load<Texture2D>("Actors/WizardSpriteSheet"), 4, 8, false);
            #endregion

            debug = Content.Load<SpriteFont>("Debug");
            tileAnimationTimer = 0;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            wizardRed = Content.Load<Effect>("WizardRed"); wizardGreen = Content.Load<Effect>("WizardGreen");
            wizardYellow = Content.Load<Effect>("WizardYellow");
            temp = Content.Load<Texture2D>("Wizard"); back = Content.Load<Texture2D>("UI/TitleScreen");
            lvlSelFrame = Content.Load<Texture2D>("UI/LevelSelectFrame"); lavaPreview = Content.Load<Texture2D>("UI/LavaZonePreview"); //163 144
            
        }

        protected override void Update(GameTime gameTime)
        {
            kb = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                gameStates = GameStates.LevelSelect;
            //Exit();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Up) && kbOld.IsKeyUp(Keys.Up))
                gameStates++;

            switch(gameStates)
            {
                case GameStates.Title:
                    HandleTitle();
                    break;

                case GameStates.PlayerSelect:
                    HandlePlayerSelect();
                    break;

                case GameStates.LevelSelect:
                    HandleLevelSelect();
                    break;

                case GameStates.Options:
                    HandleOptions();
                    break;

                case GameStates.Gameplay:
                    HandleGameplay(gameTime);
                    break;
            }

            base.Update(gameTime);
            kbOld = kb;
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
                    _spriteBatch.Begin();
                    _spriteBatch.Draw(back, Vector2.Zero, Color.White);       //Title screen
                    _spriteBatch.End();
                    break;

                case GameStates.PlayerSelect:

                    break;

                case GameStates.LevelSelect:
                    //Draw arrows
                    //Draw select button
                    //Draw preview
                    //Draw area name
                    _spriteBatch.Begin();
                    switch(lvlSelPreviews)
                    {
                        case LevelSelectPreview.Castle:

                            break;

                        case LevelSelectPreview.Ice:

                            break;

                        case LevelSelectPreview.Lava:
                            _spriteBatch.Draw(lvlSelFrame, Vector2.Zero, Color.White);
                            _spriteBatch.Draw(lavaPreview, new Vector2(163, 144), Color.White);
                            _spriteBatch.DrawString(debug, "Lava Zone", new Vector2(816 / 2, 40), Color.Black);
                            break;
                    }
                    _spriteBatch.End();
                    break;

                case GameStates.Options:

                    break;

                case GameStates.Gameplay:
                    _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cam.getCam()); // Original spritebatch
                    arenas[currentLevel].Draw(_spriteBatch, _textureAtlas);
                    for (int i = 0; i < bombs.Count; i++)
                    {
                        bombs[i].Draw(_spriteBatch);
                    }
                    if (player1 != null)
                        player1.Draw(_spriteBatch, gameTime, 16, 16);
                    _spriteBatch.End();

                    _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, wizardRed, cam.getCam());    //Red wizard
                    if (player2 != null)
                        player2.Draw(_spriteBatch, gameTime, 16, 16);
                    _spriteBatch.End();

                    _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, wizardGreen, cam.getCam());  //Green Wizard
                    if (player3 != null)
                        player3.Draw(_spriteBatch, gameTime, 16, 16);
                    _spriteBatch.End();

                    _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, wizardYellow, cam.getCam()); //Yellow wizard
                    if (player4 != null)
                        player4.Draw(_spriteBatch, gameTime, 16, 16);
                    _spriteBatch.End();

                    _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cam.getCam()); //Debug
                                                                                                                                //_spriteBatch.DrawString(debug, bombs.Count.ToString(), Vector2.Zero, Color.Orange);
                                                                                                                                //_spriteBatch.DrawString(debug, player1.Position.ToString(), player1.Position, Color.Orange);
                                                                                                                                //_spriteBatch.DrawString(debug, fps.ToString() + "Frames when no explode", Vector2.Zero, Color.Black);

                    if (fps < 58 && isRecord && gameTime.TotalGameTime.TotalSeconds > 4 && bombs.Count > 2)
                    {
                        recordFPS = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
                        //isRecord = false;
                    }


                    //_spriteBatch.DrawString(debug, recordFPS.ToString() + "Frames when explode", new Vector2(20, 20), Color.Black);
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

        }
        private void HandlePlayerSelect()
        {

        }
        private void HandleLevelSelect()
        {
            if(kb.IsKeyDown(Keys.A) && kbOld.IsKeyUp(Keys.A))   // If gamepad left or if left arrow button pressed
            {
                //Do button left
            }
            if(kb.IsKeyDown(Keys.D) && kbOld.IsKeyUp(Keys.D))   // If gamepad right or if right arrow button pressed
            {
                //Do button right
            }
            if(kb.IsKeyDown(Keys.Enter) && kbOld.IsKeyUp(Keys.Enter) || kb.IsKeyDown(Keys.Space) && kbOld.IsKeyUp(Keys.Space))  // Or GamePad button A or if Select Button pressed
            {
                //Choose this level and move to next game state
                //Set the timer to count down the game starting here
                gameStates = GameStates.Gameplay;
            }
        }
        private void HandleOptions()
        {

        }
        private void HandleGameplay(GameTime gameTime)
        {
            if (player1 != null && kb.IsKeyDown(Keys.Space) && kbOld.IsKeyUp(Keys.Space) && player1.BombsPlaced <= player1.MaxBombs /*&& arenas[currentLevel].testing(new Vector2(player1.Position.X, player1.Position.Y))*/)
            {
                bombs.Add(new Bomb(new Point(player1.Position.ToPoint().X / 16, player1.Position.ToPoint().Y / 16), 3, 2, arenas[currentLevel], Content.Load<Texture2D>("TestBomb"), 1, genExplosions, genericUnsafeTileClears, gameTime));
                player1.BombsPlaced++;
            }
            #region AI Bombing
            if (player2 != null && player2.isDeciding && player2.BombsPlaced <= player2.MaxBombs)
            {
                bombs.Add(new Bomb(new Point(player2.Position.ToPoint().X / 16, player2.Position.ToPoint().Y / 16), 3, 5, arenas[currentLevel], Content.Load<Texture2D>("TestBomb"), 2, genExplosions, genericUnsafeTileClears, gameTime));
                player2.BombsPlaced++;
                player2.isDeciding = false;
            }
            if (player3 != null && player3.isDeciding && player3.BombsPlaced <= player3.MaxBombs)
            {
                bombs.Add(new Bomb(new Point(player3.Position.ToPoint().X / 16, player3.Position.ToPoint().Y / 16), 3, 5, arenas[currentLevel], Content.Load<Texture2D>("TestBomb"), 3, genExplosions, genericUnsafeTileClears, gameTime));
                player3.BombsPlaced++;
                player3.isDeciding = false;
            }
            if (player4 != null && player4.isDeciding && player4.BombsPlaced <= player4.MaxBombs)
            {
                bombs.Add(new Bomb(new Point(player4.Position.ToPoint().X / 16, player4.Position.ToPoint().Y / 16), 3, 5, arenas[currentLevel], Content.Load<Texture2D>("TestBomb"), 4, genExplosions, genericUnsafeTileClears, gameTime));
                player4.BombsPlaced++;
                player4.isDeciding = false;
            }
            #endregion

            for (int i = 0; i < bombs.Count; i++)
            {
                bombs[i].Update(gameTime);

                if (bombs[i].TimeToDie)
                {
                    foreach (var unsafeTile in genericUnsafeTileClears)
                    {
                        if (unsafeTile.Position == bombs[i].Position)
                            unsafeTile.ClearTileExecutor(gameTime);
                    }
                        
                    if (bombs[i].PlayerOwner == 1)
                        player1.BombsPlaced--;
                    else if (bombs[i].PlayerOwner == 2)
                        player2.BombsPlaced--;
                    else if (bombs[i].PlayerOwner == 3)
                        player3.BombsPlaced--;
                    else if (bombs[i].PlayerOwner == 4)
                        player4.BombsPlaced--;
                    bombs.RemoveAt(i);
                    bombcount -= 1;
                }
            }

            foreach (var explosion in genExplosions)
                explosion.TileExplosionAnimate(gameTime);


            tileAnimationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
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

            //if (arenas[currentLevel].TileData[1, 2] == 23)
            //    arenas[currentLevel].TileData[1, 2] = 17;

            if (player1 != null)
                player1.Update(gameTime, arenas[currentLevel], kb, kbOld);
            if (player2 != null)
                player2.Update(gameTime, arenas[currentLevel], kb, kbOld);
            if (player3 != null)
                player3.Update(gameTime, arenas[currentLevel], kb, kbOld);
            if (player4 != null)
                player4.Update(gameTime, arenas[currentLevel], kb, kbOld);
        }

        private void ClearTileExecutor(Bomb bomb, Point bombPosition, int explosionRadius, Tilemap _tileMap, GameTime gameTime)
        {
            ClearUnsafeTiles(bomb.Position, 0, -1, bomb.ExplosionRadius, arenas[currentLevel], gameTime);
            ClearUnsafeTiles(bomb.Position, 0, 1, bomb.ExplosionRadius, arenas[currentLevel], gameTime);
            ClearUnsafeTiles(bomb.Position, -1, 0, bomb.ExplosionRadius, arenas[currentLevel], gameTime);
            ClearUnsafeTiles(bomb.Position, 1, 0, bomb.ExplosionRadius, arenas[currentLevel], gameTime);
        }
        private void ClearUnsafeTiles(Point bombPosition, int dx, int dy, int explosionRadius, Tilemap _tileMap, GameTime gameTime)
        {
            int x = bombPosition.X;
            int y = bombPosition.Y;

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
                    _tileMap.TileData[x, y] = 16;
                _tileMap._dangerTiles[x * 17 + y] = null;
            }

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
 * Tile shadow over floor from indestructibles                              Not done
 * Throwable Cauldrons                                                      ??? Over walls???
 * Powerups                                                                 Not done
 * Title Screen                                                             Not done
 * Menu                                                                     Not done
 * Game Over Screen                                                         Not done
 * 
 * 
 */

// Notes

/*
 * Cauldrons start with 3 tile max reach                                    Done
 * Cauldrons go in 4 directions only                                        Done
 * Cauldrons have 3 second timer                                            Done
 * 2 Cauldrons max to start with                                            Done
 * Cauldrons immediately explode if hit by explosion                        Not done
 * Potion Powerup will increase bomb reach                                  Not done
 * Potion Cauldron + will increase max cauldron cap                         Not done
 * Potion Feather will increase player speed                                Not done
 * Start with 3 shields                                                     Not done
 * Explosions remove a shield and teleport player to a safe location        Not done
 * Brick walls should explode when bombed, but block the blast              Done
 * 
 * 
 * https://community.monogame.net/t/how-to-randomly-generate-maps/14228/7
 */