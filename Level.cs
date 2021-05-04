using System.Collections.Generic;

using Microsoft.Xna.Framework;

using PolyOne.Utility;
using PolyOne.Scenes;
using PolyOne.Engine;
using PolyOne.LevelProcessor;

using AntiGrave.Platforms;


namespace AntiGrave
{
    public enum GameTags
    {
        None = 0,
        PlayerOne = 1,
        PlayerTwo = 2,
        Solid = 3,
        Empty = 4,
        Bullet = 5,
        ShieldOne = 6,
        ShieldTwo = 7
    }

    public class Level : Scene
    {
        public LevelTilesSolid tilesSolid;
        public LevelTilesEmpty tilesEmpty;

        public LevelTiler Tile { get; private set; }
        LevelData levelData = new LevelData();

        bool[,] collisionInfoSolid;
        bool[,] collisionInfoEmpty;
        bool[,] spawnPoints;

        public Player playerOne;
        public Player playerTwo;

        public GraveCamera Camera { get; set; }
        public List<Vector2> SpawnPoints = new List<Vector2>();

        public Level()
        {
        }

        public void LoadLevel(string levelName)
        {
            LoadContent();

            Tile = new LevelTiler();

            levelData = Engine.Instance.Content.Load<LevelData>(levelName);
            Tile.LoadContent(levelData);

            collisionInfoSolid = LevelTiler.TileConverison(Tile.CollisionLayer, 2);
            tilesSolid = new LevelTilesSolid(collisionInfoSolid);
            this.Add(tilesSolid);

            spawnPoints = LevelTiler.TileConverison(Tile.CollisionLayer, 3);
            SpawnPointAssignment();

            collisionInfoEmpty = LevelTiler.TileConverison(Tile.CollisionLayer, 0);
            tilesEmpty = new LevelTilesEmpty(collisionInfoEmpty);
            this.Add(tilesEmpty);

            playerOne = new Player(Tile.PlayerPosition[0], PlayerIndex.One);
            this.Add(playerOne);
            playerOne.Added(this);

            playerTwo = new Player(Tile.PlayerPosition[1], PlayerIndex.Two);
            this.Add(playerTwo);
            playerTwo.Added(this);

            Camera = new GraveCamera();
        }

        public override void LoadContent()
        {
            base.LoadContent();
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update()
        {
            base.Update();

            Camera.Update();
        }

        private void SpawnPointAssignment()
        {
            for (int x = 0; x < spawnPoints.GetLength(0) ; x++)
            {
                for (int y = 0; y < spawnPoints.GetLength(1); y++)
                {
                    bool index = spawnPoints[x, y];
                    if(index == true) {
                        SpawnPoints.Add(new Vector2(x * TileInformation.TileWidth, 
                                                    y * TileInformation.TileHeight - TileInformation.TileHeight / 2));
                    }
                }
            }
        }

        public bool MatchFinished(out int index)
        {
            index = 0;

            if(playerOne.HitPoints >= playerOne.HitLimit) {
                index = 1;
                return true;
            }

            if (playerTwo.HitPoints >= playerTwo.HitLimit) {
                index = 2;
                return true;
            }
            return false;
        }

        public override void Draw()
        {
            Engine.Begin(Camera.TransformMatrix);
            Tile.DrawBackground();
            base.Draw();
            Engine.End();
        }
    }
}
