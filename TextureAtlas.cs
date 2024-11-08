using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ClaimTheCastle
{
    class TextureAtlas
    {
        #region Properties
        public Texture2D Texture { get; }
        public int TileWidth { get; }
        public int TileHeight { get; }
        public int TilesWide { get; }
        public int TilesHigh { get; }
        #endregion
        public Rectangle[] SourceRectangles { get; }

        public TextureAtlas(Texture2D image, int tilesWide, int tilesHigh, int tileWidth, int tileHeight)
        {
            Texture = image;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            TilesWide = tilesWide;
            TilesHigh = tilesHigh;

            var tiles = tilesWide * tilesHigh;

            SourceRectangles = new Rectangle[tiles];

            var tile = 0;

            for (int y = 0; y < tilesHigh; y++)
                for (int x = 0; x < tilesWide; x++)
                {
                    SourceRectangles[tile] = new Rectangle(x * tileWidth, y * tileHeight, tileWidth, tileHeight);
                    tile++;
                }
        }
    }
}
