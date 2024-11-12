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

        private Texture2D temp, back;
        private Vector2 player;
        private Cam2D cam;

        int currentLevel = 0;
        private List<Tilemap> arenas;
        private TextureAtlas _textureAtlas;

        private Player player1, player2, player3, player4;

        private List<Bomb> bombs;
        int bombcount;

        Effect wizardRed, wizardGreen, wizardYellow;

        KeyboardState kb, kbOld;
        public static SpriteFont debug;
        public static Random RNG = new Random();

        float recordFPS = 60; bool isRecord = true;

        public static GameConsole GConsole;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferHeight = 720;
            _graphics.PreferredBackBufferWidth = 1080;
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

            cam.Position = new Vector2(46, 20);
            player = new Vector2 (50, 50);

            arenas = new List<Tilemap>();

            bombs = new List<Bomb>();
            bombcount = 0;

            for (int i = 0; i < 2; i++)
            {
                arenas.Add(new Tilemap($"Content/Levels/level{i}", Vector2.Zero, 64, 16));
            }

            _textureAtlas = new TextureAtlas(Content.Load<Texture2D>("TextureAtlas"), 8, 6, 16, 16);
            player1 = new Player(new Vector2(34, 17), Content.Load<Texture2D>("Actors/WizardSpriteSheet"), 4, 8, true);
            player2 = new Player(new Vector2(4 * 16, 6 * 16), Content.Load<Texture2D>("Actors/WizardSpriteSheet"), 4, 8, false);
            player3 = new Player(new Vector2(5 * 16, 5 * 16), Content.Load<Texture2D>("Actors/WizardSpriteSheet"), 4, 8, false);
            player4 = new Player(new Vector2(7 * 16, 7 * 16), Content.Load<Texture2D>("Actors/WizardSpriteSheet"), 4, 8, false);

            debug = Content.Load<SpriteFont>("Debug");

        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            wizardRed = Content.Load<Effect>("WizardRed"); wizardGreen = Content.Load<Effect>("WizardGreen");
            wizardYellow = Content.Load<Effect>("WizardYellow");
            temp = Content.Load<Texture2D>("Wizard"); back = Content.Load<Texture2D>("TitleScreen");
            
        }

        protected override void Update(GameTime gameTime)
        {
            kb = Keyboard.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            
            if (kb.IsKeyDown(Keys.Space) && kbOld.IsKeyUp(Keys.Space)/* && player1.BombsPlaced <= player1.MaxBombs*/)
            {
                player1.BombsPlaced++;
                bombs.Add(new Bomb(new Point(player1.Position.ToPoint().X / 16, player1.Position.ToPoint().Y / 16), 3, 5, arenas[currentLevel], Content.Load<Texture2D>("TestBomb")));
                bombcount += 1;
            }

            if (player2.isDeciding)
                bombs.Add(new Bomb(new Point(player2.Position.ToPoint().X / 16, player2.Position.ToPoint().Y / 16), 3, 5, arenas[currentLevel], Content.Load<Texture2D>("TestBomb")));
            if (player3.isDeciding)
                bombs.Add(new Bomb(new Point(player3.Position.ToPoint().X / 16, player3.Position.ToPoint().Y / 16), 3, 5, arenas[currentLevel], Content.Load<Texture2D>("TestBomb")));

            for (int i = 0; i < bombs.Count; i++)
            {
                bombs[i].Update(gameTime);
                if (bombs[i].TimeToDie == true)
                {
                    bombs.RemoveAt(i);
                    bombcount -= 1;
                }
            }

            player1.Update(gameTime, arenas[currentLevel], kb, kbOld);
            player2.Update(gameTime, arenas[currentLevel], kb, kbOld);
            player3.Update(gameTime, arenas[currentLevel], kb, kbOld);
            player4.Update(gameTime, arenas[currentLevel], kb, kbOld);

            //cam.Position.X = (-player1.Position.X + _graphics.PreferredBackBufferWidth / (2 * 3));
            //cam.Position.Y = (-player1.Position.Y + _graphics.PreferredBackBufferHeight / (2 * 3));

            base.Update(gameTime);
            kbOld = kb;
        }

        protected override void Draw(GameTime gameTime)
        {
            float fps = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cam.getCam()); // Original spritebatch
            _spriteBatch.Draw(back, Vector2.Zero, Color.White);
            arenas[currentLevel].Draw(_spriteBatch, _textureAtlas);
            for (int i = 0; i < bombs.Count; i++)
            {
                bombs[i].Draw(_spriteBatch);
            }
            player1.Draw(_spriteBatch, gameTime, 16, 16);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, wizardRed, cam.getCam());    //Red wizard
            player2.Draw(_spriteBatch, gameTime, 16, 16);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, wizardGreen, cam.getCam());  //Green Wizard
            player3.Draw(_spriteBatch, gameTime, 16, 16);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, wizardYellow, cam.getCam()); //Yellow wizard
            player4.Draw(_spriteBatch, gameTime, 16, 16);
            _spriteBatch.End();

            _spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, cam.getCam()); //Debug
            //_spriteBatch.DrawString(debug, bombs.Count.ToString(), Vector2.Zero, Color.Orange);
            //_spriteBatch.DrawString(debug, player1.Position.ToString(), player1.Position, Color.Orange);
            _spriteBatch.DrawString(debug, fps.ToString() + "Frames when no explode", Vector2.Zero, Color.Black);
            
            if (fps < 58 && isRecord && bombs.Count > 0)
            {
                recordFPS = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
                //isRecord = false;
            }
                
                
            _spriteBatch.DrawString(debug, recordFPS.ToString() + "Frames when explode", new Vector2(20, 20), Color.Black);
            _spriteBatch.End();

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
    }
}



//Checklist

/*
 * Player Functionality
 * Tilemap
 * Cauldron Spawning
 * Destructible Tiles - Indestructible Tiles
 * Tile shadow over floor from indestructibles
 * Throwable Cauldrons
 * Powerups
 * Title Screen
 * Menu
 * Game Over Screen
 * 
 * 
 */

// Notes

/*
 * Cauldrons start with 3 tile max reach
 * Cauldrons go in 4 directions only
 * Cauldrons have 3 second timer
 * 2 Cauldrons max to start with
 * Cauldrons immediately explode if hit by explosion
 * Potion Powerup will increase bomb reach
 * Potion Cauldron + will increase max cauldron cap
 * Potion Feather will increase player speed
 * Start with 3 shields
 * Explosions remove a shield and teleport player to a safe location
 * Brick walls should explode when bombed, but block the blast
 * 
 * 
 * https://community.monogame.net/t/how-to-randomly-generate-maps/14228/7
 */