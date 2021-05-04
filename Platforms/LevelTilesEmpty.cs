using PolyOne.Collision;
using PolyOne.Utility;

namespace AntiGrave.Platforms
{
    public class LevelTilesEmpty : Empty
    {
        public Grid Grid { get; private set; }

        public LevelTilesEmpty(bool[,] solidData)
        {
            this.Active = false;
            this.Collider = (this.Grid = new Grid(TileInformation.TileWidth, TileInformation.TileHeight, solidData));
        }
    }
}
