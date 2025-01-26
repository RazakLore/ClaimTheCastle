using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace ClaimTheCastle
{
    class UserInterface
    {
        private Interface userInterface;
        public ArrowState arrowState { get; set; }
        public TitleButtonState titleButtonState { get; set; }
        private Texture2D uiSpriteSheet, transparentScreen, selectButton, startButton, quitButton, play1Sel, play2Sel, play3Sel, play4Sel, tick, cross;
        private Rectangle iconSourceRectangle;
        public Rectangle startRect { get; private set; }
        public Rectangle optRect { get; private set; } // for detecting mouse click in game 1 to change the ui state
        public Rectangle exitRect { get; private set; }
        public bool CanPickKeyboard {  private get; set; }
        public bool IsGameOver { get; set; }
        public float arrowTimer {  get; set; }
        public bool IsStartPressed { get; set; }
        public Interface UserInterfaceState
        {
            get { return userInterface; }
            set { userInterface = value; }
        }
        public UserInterface(Texture2D uiSheet, Texture2D tranScreen, Texture2D selButton, Texture2D staButton, Texture2D quiButton, Texture2D p1Screen, Texture2D p2Screen, Texture2D p3Screen, Texture2D p4Screen, Texture2D tic, Texture2D cros)
        {
            uiSpriteSheet = uiSheet;
            iconSourceRectangle = new Rectangle(0, 0, 16, 16);
            transparentScreen = tranScreen;
            selectButton = selButton;
            startButton = staButton;
            quitButton = quiButton;
            startRect = new Rectangle(566, 400, 229, 51);
            exitRect = new Rectangle(573, 505, 214, 42);
            play1Sel = p1Screen;
            play2Sel = p2Screen;
            play3Sel = p3Screen;
            play4Sel = p4Screen;
            tick = tic;
            cross = cros;
        }

        public void Update(GameTime gt, MouseState ms, KeyboardState kb, GameStates gameState)
        {
            switch (userInterface)
            {
                case Interface.Title:
                    if (startRect.Contains(ms.Position))
                    {
                        titleButtonState = TitleButtonState.Start;
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            IsStartPressed = true;
                        }
                    }
                        
                    if (exitRect.Contains(ms.Position))
                    {
                        titleButtonState = TitleButtonState.Exit;
                    }
                    break;

                case Interface.PlayerSelect:

                    break;

                case Interface.LevelSelect:

                    break;

                case Interface.Gameplay:

                    break;
            }
        }

        public void Draw(SpriteBatch sb, int p1hp, int p2hp, int p3hp, int p4hp, List<Player> player)
        {
            switch (userInterface)
            {
                case Interface.Title:
                    sb.DrawString(Game1.debug, "Press SPACE to go to\nplayer select screen.\nWorking buttons coming soon!", new Vector2(595, 5), Color.White);
                    switch (titleButtonState)
                    {
                        case TitleButtonState.Start:
                            sb.Draw(startButton, new Vector2(566, 400), Color.Yellow);
                            sb.Draw(quitButton, new Vector2(573, 505), Color.White);
                            sb.Draw(uiSpriteSheet, new Vector2(530, 408), new Rectangle(16, 16, 16, 16), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
                            break;

                        case TitleButtonState.Options:

                            break;

                        case TitleButtonState.Exit:
                            sb.Draw(startButton, new Vector2(566, 400), Color.White);
                            sb.Draw(quitButton, new Vector2(573, 505), Color.Yellow);
                            sb.Draw(uiSpriteSheet, new Vector2(547, 513), new Rectangle(16, 16, 16, 16), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
                            break;
                    }
                    break;

                case Interface.PlayerSelect:
                    for (int i = 0; i < player.Count; i++)
                    {
                        // Only show the selection prompt if the current player has not selected and the previous player has selected
                        if (!player[i].IsPlayer)
                        {
                            // Check if the previous player has made a selection (i.e., their IsPlayer is true)
                            if (i == 0 || player[i - 1].IsPlayer)
                            {
                                // Determine the position based on the player index
                                Vector2 position = i switch
                                {
                                    0 => new Vector2(200, 55),        // Player 1
                                    1 => new Vector2(450, 55),        // Player 2
                                    2 => new Vector2(200, 425),       // Player 3
                                    3 => new Vector2(450, 425),       // Player 4
                                    _ => new Vector2(0, 0)            // Default position (if necessary)
                                };

                                // Display the selection input icons for the current player
                                DisplaySelectionInputIcons(sb, i + 1, position);
                            }
                        }
                    }
                    break;

                case Interface.LevelSelect:
                    switch (arrowState)
                    {
                        case ArrowState.LeftSelected:
                            if (arrowTimer < 0)
                            {
                                sb.Draw(uiSpriteSheet, new Vector2(50, 335), new Rectangle(32, 48, 16, 16), Color.White, 0, Vector2.Zero, 3, SpriteEffects.None, 0);    // Left selected
                                if (arrowTimer < -0.4f)
                                    arrowTimer = 0.4f;
                            }
                            else if (arrowTimer > 0)
                                sb.Draw(uiSpriteSheet, new Vector2(50, 335), new Rectangle(16, 48, 16, 16), Color.White, 0, Vector2.Zero, 3, SpriteEffects.None, 0);     // Left deselected
                            sb.Draw(uiSpriteSheet, new Vector2(716, 335), new Rectangle(48, 48, 16, 16), Color.White, 0, Vector2.Zero, 3, SpriteEffects.None, 0);       // Right deselected
                            break;

                        case ArrowState.RightSelected:
                            if (arrowTimer < 0)
                            {
                                sb.Draw(uiSpriteSheet, new Vector2(716, 335), new Rectangle(64, 48, 16, 16), Color.White, 0, Vector2.Zero, 3, SpriteEffects.None, 0);   // Right selected
                                if (arrowTimer < -0.4f)
                                    arrowTimer = 0.4f;
                            }
                            else if (arrowTimer > 0)
                                sb.Draw(uiSpriteSheet, new Vector2(716, 335), new Rectangle(48, 48, 16, 16), Color.White, 0, Vector2.Zero, 3, SpriteEffects.None, 0);   // Right deselected
                            sb.Draw(uiSpriteSheet, new Vector2(50, 335), new Rectangle(16, 48, 16, 16), Color.White, 0, Vector2.Zero, 3, SpriteEffects.None, 0);     // Left deselected
                            
                            break;
                    }
                    sb.Draw(uiSpriteSheet, new Vector2(70, 300), new Rectangle(64, 16, 16, 16), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0); // A
                    sb.Draw(uiSpriteSheet, new Vector2(70, 395), new Rectangle(32, 32, 16, 16), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0); // Dpad left

                    sb.Draw(uiSpriteSheet, new Vector2(710, 300), new Rectangle(16, 32, 16, 16), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);// D
                    sb.Draw(uiSpriteSheet, new Vector2(710, 395), new Rectangle(48, 32, 16, 16), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);// Dpad right
                    break;

                case Interface.Gameplay:
                    {
                        switch (IsGameOver)
                        {
                            case false:
                                //Draw icons, shields
                                int yOffset = 16;
                                sb.Draw(uiSpriteSheet, Vector2.Zero, new Rectangle(16, 0, 16, 16), Color.White);    // Blue hat
                                for (int i = 0; i < p1hp; i++)
                                {
                                    sb.Draw(uiSpriteSheet, new Vector2(0, yOffset), new Rectangle(64, 64, 16, 16), Color.White);      // Draws this player's HP
                                    yOffset += 16;
                                }
                                yOffset = 16;
                                sb.Draw(uiSpriteSheet, new Vector2(256, 0), new Rectangle(32, 0, 16, 16), Color.White);    // Red hat
                                for (int i = 0; i < p2hp; i++)
                                {
                                    sb.Draw(uiSpriteSheet, new Vector2(256, yOffset), new Rectangle(64, 64, 16, 16), Color.White);
                                    yOffset += 16;
                                }
                                yOffset = 208;
                                sb.Draw(uiSpriteSheet, new Vector2(0, 224), new Rectangle(48, 0, 16, 16), Color.White);    // Green hat
                                for (int i = 0; i < p3hp; i++)
                                {
                                    sb.Draw(uiSpriteSheet, new Vector2(0, yOffset), new Rectangle(64, 64, 16, 16), Color.White);
                                    yOffset -= 16;
                                }
                                yOffset = 208;
                                sb.Draw(uiSpriteSheet, new Vector2(256, 224), new Rectangle(64, 0, 16, 16), Color.White);    // Yellow hat
                                for (int i = 0; i < p4hp; i++)
                                {
                                    sb.Draw(uiSpriteSheet, new Vector2(256, yOffset), new Rectangle(64, 64, 16, 16), Color.White);
                                    yOffset -= 16;
                                }
                                break;

                            case true:
                                sb.Draw(transparentScreen, Vector2.Zero, Color.White);
                                sb.DrawString(Game1.debug, "GAME OVER", new Vector2(350, 350), Color.White);
                                break;
                        }
                        break;
                    }
            }
        }

        private void DisplaySelectionInputIcons(SpriteBatch sb, int playerIndex, Vector2 icon1)
        {
            if (playerIndex == 1)
                sb.Draw(play1Sel, Vector2.Zero, Color.White);
            if (playerIndex == 2)
                sb.Draw(play2Sel, Vector2.Zero, Color.White);
            if (playerIndex == 3)
                sb.Draw(play3Sel, Vector2.Zero, Color.White);
            if (playerIndex == 4)
                sb.Draw(play4Sel, Vector2.Zero, Color.White);

            switch (CanPickKeyboard)
            {
                case false:
                    sb.Draw(cross, new Vector2(486, 227), Color.White);
                    break;

                case true:
                    sb.Draw(tick, new Vector2(486, 227), Color.White);
                    break;
            }
        }
    }

    enum Interface
    {
        Title,
        PlayerSelect,
        LevelSelect,
        Gameplay
    }

    enum TitleButtonState
    {
        Start,
        Options,
        Exit
    }

    enum ArrowState
    {
        LeftSelected,
        RightSelected,
    }
}
