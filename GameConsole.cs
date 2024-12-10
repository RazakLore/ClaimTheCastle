using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

namespace ClaimTheCastle
{
    /// <summary>
    /// Which corner of the screen to snap the console to
    /// </summary>
    public enum ConsoleLocation
    {
        Custom = 0,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    public struct ConsoleEntry
    {
        public float TimeStamp { get; set; }
        public string Value { get; set; }
        public Color Tint { get; set; }

        public ConsoleEntry(float timeStamp, string value, Color tint)
        {
            TimeStamp = timeStamp;
            Value = value;
            Tint = tint;
        }
    }

    public class GameConsole : DrawableGameComponent
    {
        private SpriteBatch _sB;

        private List<ConsoleEntry> _textEntries;
        private int _maxEntries;

        private Texture2D _pixel;

        private Rectangle _bounds;
        private Rectangle _clippingRect;

        private SpriteFont _entryFont;

        private float _timeStamp;
        private float _tillFadeRemaining;
        private float _fadeRemaining;

        private int _totalMessages;

        #region Configuration option properties
        public Color PanelColour { get; set; }
        public int BorderWidth { get; set; }
        public Color BorderColour { get; set; }
        public Color TextColour { get; set; }

        public bool ShowTimeStamps { get; set; }
        public bool LogLogging { get; set; }

        public bool FadeAfterEvent { get; set; }
        public float TimeTillFade { get; set; }
        public float Fadetime { get; set; }
        #endregion

        /// <summary>
        /// Create an in game logging console.
        /// </summary>
        /// <param name="game">The game object (usually "this")</param>
        /// <param name="bounds">Where and how big the console is to be</param>
        public GameConsole(Game game, Rectangle bounds) : base(game)
        {
            _sB = new SpriteBatch(Game.GraphicsDevice);

            _totalMessages = 0;
            _maxEntries = 0;
            Layout(bounds);

            PanelColour = Color.Black * 0.25f;
            BorderWidth = 1;
            BorderColour = Color.Black * 0.5f;
            TextColour = Color.White;

            ShowTimeStamps = true;
#if DEBUG
            LogLogging = true;
#else
            LogLogging = false;
#endif

            FadeAfterEvent = true;

            TimeTillFade = 8;
            Fadetime = 3;

            _tillFadeRemaining = 1;
            _fadeRemaining = 1;
        }

        /// <summary>
        /// Create an in game logging console.
        /// </summary>
        /// <param name="game">The game object (usually "this")</param>
        /// <param name="size">How big the console should be in pixels</param>
        /// <param name="location">Which corner of the screen the console should appear in</param>
        /// <param name="margin">How far from the corner the console should appear</param>
        public GameConsole(Game game, Point size, ConsoleLocation location = ConsoleLocation.BottomRight, int margin = 8)
            : this(game, new Rectangle(Point.Zero, size))
        {
            Point anchorPoint;

            // Calculate the pixel location from the selected screen corner location
            switch (location)
            {
                case ConsoleLocation.TopLeft:
                    anchorPoint = Point.Zero + new Point(margin);
                    break;
                case ConsoleLocation.TopRight:
                    anchorPoint = new Point(
                        game.GraphicsDevice.Viewport.Bounds.Width - _bounds.Width - margin,
                        margin);
                    break;
                case ConsoleLocation.BottomLeft:
                    anchorPoint = new Point(
                        margin,
                        game.GraphicsDevice.Viewport.Bounds.Height - _bounds.Height - margin
                    );
                    break;
                default:
                    anchorPoint = new Point(
                        game.GraphicsDevice.Viewport.Bounds.Width - _bounds.Width - margin,
                        game.GraphicsDevice.Viewport.Bounds.Height - _bounds.Height - margin
                    );
                    break;
            }

            // Re-layout based on the above
            Layout(new Rectangle(anchorPoint, size));
        }

        public override void Initialize()
        {
            _entryFont = Game.Content.Load<SpriteFont>("consoleFont");
            _maxEntries = (int)(_bounds.Height / (_entryFont.MeasureString("0").Y + 1));

            _textEntries = new List<ConsoleEntry>(_maxEntries);

            // make a pixel to paint the background and border with
            _pixel = new Texture2D(Game.GraphicsDevice, 1, 1);
            _pixel.SetData(new Color[] { Color.White });

#if DEBUG
            // In debug mode, describe the console settings when it initialises
            Log($"Console at: {_bounds.Location}.");
            Log($"Size: {_bounds.Size}");
            Log($"{_maxEntries} entries max at font size {_entryFont.LineSpacing}px.");
            Log($"Fading is {(FadeAfterEvent ? "on" : "off")}. Trigger: {TimeTillFade}s, Speed: {Fadetime}s");
            Log($"Timestamps are {(ShowTimeStamps ? "on" : "off")}.");
#endif
        }

        public override void Update(GameTime gameTime)
        {
            // Take the current timestamp for use later
            _timeStamp = (float)gameTime.TotalGameTime.TotalSeconds;

            // If the console is not set to fade, there's nothing else to do.
            if (!FadeAfterEvent)
                return;

            // If the mouse is inside the console, make it appear
            if (_bounds.Contains(Mouse.GetState().Position))
            {
                _tillFadeRemaining = 1;
                _fadeRemaining = 1;
            }

            // If we're still waiting to start fading, just count the timer down and stop
            if (_tillFadeRemaining > 0)
            {
                _tillFadeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds / TimeTillFade;
                return;
            }

            // The timer has expired, so fade to almost invisible (change value to 0 for completely invisible)
            if (_fadeRemaining > 0.05f)
                _fadeRemaining -= (float)gameTime.ElapsedGameTime.TotalSeconds / Fadetime;

        }

        public override void Draw(GameTime gameTime)
        {
            // Draw the background and border

            //_sB.Begin();
            //_sB.DrawString(_entryFont, 
            //    "Total: " + _totalMessages.ToString()
            //    + " MpS: " + (_totalMessages / _timeStamp).ToString("0.00"), 
            //    new Vector2(_bounds.Location.X, _bounds.Location.Y - _entryFont.MeasureString("0").Y), 
            //    TextColour * _fadeRemaining);
            //DrawFrame();
            //_sB.End();

            //// Add the clipping rect to the graphics device to be used in the draw method
            //_sB.GraphicsDevice.ScissorRectangle = _clippingRect;
            //// Draw the text, but use a clipping rectangle this time so it doesn't flow outside the background
            //_sB.Begin(rasterizerState: new RasterizerState() { ScissorTestEnable = true });

            //// Work out where the first (most recent) log entry will be near the bottom of the console.
            //var linePosition = new Vector2(_bounds.Left + 4, _bounds.Bottom - _entryFont.MeasureString("0").Y - 4);

            //// loop backwards through the entries and display them
            //if (_textEntries.Count > 0)
            //{
            //    _sB.DrawString(_entryFont, _textEntries[^1].Value, linePosition, _textEntries[^1].Tint * _fadeRemaining);
            //    for (int i = _textEntries.Count - 2; i >= 0; i--)
            //    {
            //        if (_textEntries[i].TimeStamp != _textEntries[i + 1].TimeStamp)
            //            linePosition.Y -= _entryFont.MeasureString("0").Y + 4;
            //        else
            //            linePosition.Y -= _entryFont.MeasureString("0").Y + 1;
            //        _sB.DrawString(_entryFont, _textEntries[i].Value, linePosition, _textEntries[i].Tint * _fadeRemaining);
            //    }
            //}
            //_sB.End();
        }

        private void Layout(Rectangle bounds)
        {
            _bounds = bounds;
            // Set the clipping bounds to be inside the 
            _clippingRect = _bounds;
            _clippingRect.Location += new Point(4);
            _clippingRect.Size -= new Point(8);
            _sB.GraphicsDevice.ScissorRectangle = _clippingRect;
        }

        private void DrawFrame()
        {
            // Draw a background rectangle
            _sB.Draw(_pixel, _bounds, PanelColour * _fadeRemaining);
            // Draw 4 thin rectangles around the edges
            _sB.Draw(_pixel, new Rectangle(_bounds.Left, _bounds.Top - BorderWidth / 2, _bounds.Width, BorderWidth), BorderColour * _fadeRemaining);
            _sB.Draw(_pixel, new Rectangle(_bounds.Left - BorderWidth / 2, _bounds.Top, BorderWidth, _bounds.Height), BorderColour * _fadeRemaining);
            _sB.Draw(_pixel, new Rectangle(_bounds.Left, _bounds.Bottom - BorderWidth / 2, _bounds.Width, BorderWidth), BorderColour * _fadeRemaining);
            _sB.Draw(_pixel, new Rectangle(_bounds.Right - BorderWidth / 2, _bounds.Top, BorderWidth, _bounds.Height), BorderColour * _fadeRemaining);
        }

        public void Log(string message)
        {
            AddEntry(message, TextColour);
        }

        public void Warn(string message)
        {
            AddEntry(message, Color.Red);
        }

        public void Good(string message)
        {
            AddEntry(message, Color.Green);
        }

        private void AddEntry(string message, Color tint)
        {
            _totalMessages++;
            // If we're already at the max number of entries, remove one from the other end
            if (_textEntries.Count == _maxEntries)
                _textEntries.RemoveAt(0);

            // If timestamps are on, prepend the total gametime
            if (ShowTimeStamps)
                message = _timeStamp.ToString("00:00: ") + message;

            // Actually add the log entry
            _textEntries.Add(new ConsoleEntry(_timeStamp, message, tint));

            // If fading is on, appear the console
            if (FadeAfterEvent)
            {
                _tillFadeRemaining = 1;
                _fadeRemaining = 1;
            }

            // If loglogging is on, also write to the debug output.
            if (LogLogging)
            {
                Debug.WriteLine("Player Console: " + message);
            }
        }

    }
}
