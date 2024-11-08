using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClaimTheCastle
{
    enum Direction //Ordered in the same layering of the rows in the spritesheet, easier to remember which value is which
    {
        North,      //0
        East,       //1
        South,      //2
        West        //3
    }
    class GameActor
    {
        protected Vector2 m_position;
        protected Texture2D m_txr;
        protected Rectangle m_rectangle;

        private int m_frameCount;
        private int m_animFrame;
        private Rectangle m_sourceRect;

        private float m_updateTrigger;
        private float m_fps;

        private Direction m_facing;
        public Direction Facing
        {
            set
            {
                m_facing = value;
            }
        }
        public Vector2 Position {  get { return m_position; } set { m_position = value; } }
        public Rectangle Collision {  get { return m_rectangle;  }  }
        public GameActor(Vector2 startPos, Texture2D txr, int frameCount, int fps)
        {
            m_position = startPos;
            m_txr = txr;
            m_rectangle = new Rectangle((int)m_position.X, (int)m_position.Y, m_txr.Width, m_txr.Height);

            m_frameCount = frameCount;
            m_animFrame = 0;
            m_sourceRect = new Rectangle(0, 0, txr.Width / m_frameCount, txr.Height / m_frameCount);

            m_updateTrigger = 0;
            m_fps = fps;

            m_facing = Direction.South;
        }

        public void Draw(SpriteBatch sb, GameTime gt, int tileWidth, int tileHeight)
        {
            m_updateTrigger += (float)gt.ElapsedGameTime.TotalSeconds * m_fps;

            if (m_updateTrigger >= 1)
            {
                m_updateTrigger = 0;

                m_animFrame = (m_animFrame + 1) % m_frameCount;
                m_sourceRect.X = m_animFrame * m_sourceRect.Width;
            }

            m_sourceRect.Y = (int)m_facing * m_sourceRect.Height;
            sb.Draw(m_txr, new Vector2(m_position.X, m_position.Y), /*m_sourceRect,*/ Color.White);
        }

        public void Move(Direction moveDir)
        {
            Facing = moveDir;

            switch (moveDir)
            {
                case Direction.North:
                    m_position.Y -= 2;
                    break;

                case Direction.South:
                    m_position.Y += 2;
                    break;

                case Direction.East:
                    m_position.X += 2;
                    break;

                case Direction.West:
                    m_position.X -= 2;
                    break;
            }
        }
    }
}
