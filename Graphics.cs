using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ClaimTheCastle
{
    internal class StaticGraphic
    {
        protected Vector2 m_pos;
        protected Texture2D m_txr;
        public StaticGraphic(Texture2D txrImage, Vector2 position)
        {
            m_pos = position;
            m_txr = txrImage;
        }
    }

    class AnimatedGraphic : StaticGraphic
    {
        private Texture2D _art;
        protected Rectangle sourceRect, destRect;
        public AnimatedGraphic(Texture2D txr, Vector2 pos) : base(txr, pos)
        {
            _art = txr;
            sourceRect = new Rectangle(0, 0, txr.Bounds.Width / 8, txr.Bounds.Height);
            destRect = new Rectangle(0, 0, txr.Bounds.Width / 8, txr.Bounds.Height);
        }

        public void DrawAnimatedGraphic(SpriteBatch sb)
        {
            sb.Draw(_art, destRect, sourceRect, Color.White);
        }
    }
}
