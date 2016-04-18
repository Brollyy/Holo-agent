using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine;
using Engine.Components;
using Engine.Utilities;

namespace Holo_agent
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D crosshairTexture;
        SpriteFont font;
        FrameCounter frameCounter;
        Scene scene;
        GameObject player;
        GameObject[] columns;
        GameObject ladder;
        GameObject tile;
        GameObject[] walls;
        GameObject[] doors;
        int collision = 0;
        Texture2D groundTexture;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            frameCounter = new FrameCounter();
            columns = new GameObject[8];
            walls = new GameObject[7];
            doors = new GameObject[2];
            player = new GameObject("Player", new Vector3(30, 18, -25), Quaternion.Identity, Vector3.One);
            player.AddNewComponent<PlayerController>();
            GameObject camera = new GameObject("Camera", Vector3.Zero, Quaternion.Identity, Vector3.One, null, player);
            Camera cameraComp = new Camera(45, graphics.GraphicsDevice.Viewport.AspectRatio, 1, 1000);
            camera.AddComponent(cameraComp);
            Collider cameraCol = camera.AddNewComponent<Collider>();
            cameraCol.bound = new Engine.Bounding_Volumes.BoundingBox(cameraCol, Vector3.Zero, 10f*Vector3.One);
            scene = new Scene(camera);
            for (int i = 0; i < 8; ++i)
            {
                columns[i] = new GameObject("Column" + i, new Vector3(80 * (i % 2), 0, -120 * (i / 2)), Quaternion.CreateFromYawPitchRoll(0, MathHelper.ToRadians(270), 0), new Vector3(0.1f, 0.1f, 0.2f), scene);
                Collider columnCol = columns[i].AddNewComponent<Collider>();
                columnCol.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol, new Vector3(0,30,0), new Vector3(60,60,150));
            }
            ladder = new GameObject("Ladder", new Vector3(60, 15, -60), Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(225), MathHelper.ToRadians(270), 0), new Vector3(0.15f, 0.15f, 0.15f), scene);
            tile = new GameObject("Ceiling panel", new Vector3(40, 0.05f, -60), Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(225), MathHelper.ToRadians(270), 0), new Vector3(0.1f, 0.1f, 0.1f), scene);
            for(int i = 0; i < 4; ++i)
            {
                walls[i] = new GameObject("Wall" + i, new Vector3(-60 + (i % 2) * 200, 30, 40 - 440*(i/2)), Quaternion.CreateFromYawPitchRoll(0, 0, 0), new Vector3(1, 0.5f, 1), scene);
            }
            for(int i = 4; i < 6; ++i)
            {
                walls[i] = new GameObject("Wall" + i, new Vector3(200 - 320*(i%2), 30, -180), Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(90), 0, 0), new Vector3(3.75f, 0.5f, 1), scene);
            }
            walls[6] = new GameObject("Ceiling", new Vector3(40, 60, -180), Quaternion.CreateFromYawPitchRoll(0, MathHelper.ToRadians(270), 0), new Vector3(2.7f, 3.66f, 1f), scene);
            for(int i = 0; i < 2; ++i)
            {
                doors[i] = new GameObject("Doors" + i, new Vector3(40, 30, 42.5f - ((i+1)%2)*442.5f), Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians((i%2)*180), 0, 0), new Vector3(0.1f, 0.165f, 0.1f), scene);
            }
            Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
            Input.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            Model columnModel = Content.Load<Model>("Models/column_001");
            groundTexture = Content.Load<Texture2D>("Textures/Ground");
            crosshairTexture = Content.Load<Texture2D>("Textures/Crosshair");
            font = Content.Load<SpriteFont>("Textures/Arial");

            for(int i = 0; i < 8; ++i)
                columns[i].AddComponent(new MeshInstance(columnModel, null));

            Model ladderModel = Content.Load<Model>("Models/ladder");
            ladder.AddComponent(new MeshInstance(ladderModel, null));
            Model tileModel = Content.Load<Model>("Models/panel_ceiling");
            tile.AddComponent(new MeshInstance(tileModel, null));
            for (int i = 0; i < 7; ++i)
                walls[i].AddComponent(new MeshInstance(tileModel, null));
            Model doorModel = Content.Load<Model>("Models/door_001");
            for (int i = 0; i < 2; ++i)
                doors[i].AddComponent(new MeshInstance(doorModel, null));
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            frameCounter.Update(gameTime);

            Input.Update(graphics);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            player.Update(gameTime);
            scene.Camera.Update(gameTime);
            for (int i = 0; i < 8; ++i)
            {
                collision = scene.Camera.GetComponent<Collider>().Collide(columns[i].GetComponent<Collider>());
                if (collision != 0)
                {
                    player.RevertLastMovement();
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            for(int i = 0; i < 8; ++i)
                columns[i].Draw(gameTime);

            ladder.Draw(gameTime);
            tile.Draw(gameTime);
            for (int i = 0; i < 7; ++i)
                walls[i].Draw(gameTime);
            for (int i = 0; i < 2; ++i)
                doors[i].Draw(gameTime);


            RasterizerState originalState = GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizerState;
            BasicEffect effect = new BasicEffect(graphics.GraphicsDevice);
            effect.World = Matrix.Identity;
            effect.View = scene.Camera.GetComponent<Camera>().ViewMatrix;
            effect.Projection = scene.Camera.GetComponent<Camera>().ProjectionMatrix;
            effect.CurrentTechnique.Passes[0].Apply();
            short[] indexes = new short[24]
            {
                0, 1, 1, 2, 2, 3, 3, 0,
                4, 5, 5, 6, 6, 7, 7, 4,
                0, 4, 1, 5, 2, 6, 3, 7
            };

            for (int i = 0; i < 8; ++i)
            {
                Engine.Bounding_Volumes.BoundingBox box = (columns[i].GetComponent<Collider>().bound as Engine.Bounding_Volumes.BoundingBox);
                Vector3[] corners = box.Corners();

                VertexPosition[] vertices = new VertexPosition[8];
                for (int j = 0; j < 8; ++j)
                {
                    vertices[j].Position = corners[j];
                }

                graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPosition>(PrimitiveType.LineList,
                                                                                  vertices, 0, 8,
                                                                                  indexes, 0, 12);
            }
            GraphicsDevice.RasterizerState = originalState;

            DrawSprite(160, 220, new Vector3(40,180,0), new Vector3(-90, 0, 0), groundTexture, 20, false);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
            spriteBatch.Draw(crosshairTexture, new Vector2((graphics.PreferredBackBufferWidth / 2) - (crosshairTexture.Width / 2), (graphics.PreferredBackBufferHeight / 2) - (crosshairTexture.Height / 2)));
            /*spriteBatch.DrawString(font, scene.Camera.GlobalPosition.ToString(), new Vector2(50, 50), Color.Black);
            spriteBatch.DrawString(font, scene.Camera.GlobalRotation.ToString(), new Vector2(50, 75), Color.Black);
            spriteBatch.DrawString(font, player.GlobalPosition.ToString(), new Vector2(50, 100), Color.Black);
            spriteBatch.DrawString(font, player.GlobalRotation.ToString(), new Vector2(50, 125), Color.Black);*/
            spriteBatch.DrawString(font, frameCounter.AverageFramesPerSecond.ToString(), new Vector2(50, 50), Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }


        void DrawSprite(float spriteX, float spriteY, Vector3 spritePosition, Vector3 spriteRotation, Texture2D spriteTexture, int tilesNumber, bool isBillboardActive)
        {
            VertexPositionTexture[] spriteVerts = new VertexPositionTexture[6];
            BasicEffect spriteEffect = new BasicEffect(graphics.GraphicsDevice);
            spriteVerts[0].Position = new Vector3(-spriteX, -spriteY, 0);
            spriteVerts[1].Position = new Vector3(-spriteX, spriteY, 0);
            spriteVerts[2].Position = new Vector3(spriteX, -spriteY, 0);
            spriteVerts[3].Position = spriteVerts[1].Position;
            spriteVerts[4].Position = new Vector3(spriteX, spriteY, 0);
            spriteVerts[5].Position = spriteVerts[2].Position;
            spriteVerts[0].TextureCoordinate = new Vector2(0, 0);
            spriteVerts[1].TextureCoordinate = new Vector2(0, tilesNumber);
            spriteVerts[2].TextureCoordinate = new Vector2(tilesNumber, 0);
            spriteVerts[3].TextureCoordinate = spriteVerts[1].TextureCoordinate;
            spriteVerts[4].TextureCoordinate = new Vector2(tilesNumber, tilesNumber);
            spriteVerts[5].TextureCoordinate = spriteVerts[2].TextureCoordinate;
            spriteEffect.Projection = scene.Camera.GetComponent<Camera>().ProjectionMatrix;
            spriteEffect.View = scene.Camera.GetComponent<Camera>().ViewMatrix;
            spriteEffect.World = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Down);
            if (isBillboardActive == true)
            {
                spriteEffect.World *= Matrix.CreateConstrainedBillboard(spritePosition, scene.Camera.GlobalPosition, Vector3.UnitY, null, null);
            }
            else
            {
                spriteEffect.World *= Matrix.CreateTranslation(spritePosition);
            }
            spriteEffect.World *= Matrix.CreateRotationX(MathHelper.ToRadians(spriteRotation.X));
            spriteEffect.World *= Matrix.CreateRotationY(MathHelper.ToRadians(spriteRotation.Y));
            spriteEffect.World *= Matrix.CreateRotationZ(MathHelper.ToRadians(spriteRotation.Z));
            spriteEffect.TextureEnabled = true;
            spriteEffect.Texture = spriteTexture;
            foreach (EffectPass pass in spriteEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, spriteVerts, 0, 2);
            }
        }
    }
}
