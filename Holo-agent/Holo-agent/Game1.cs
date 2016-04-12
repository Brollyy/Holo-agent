using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine;
using Engine.Components;
using Engine.Bounding_Volumes;

namespace Holo_agent
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D groundTexture;
        Texture2D crosshairTexture;
        Texture2D treeTexture;
        SpriteFont font;
        Scene scene;
        GameObject robot;
        int collision = 0;

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
            GameObject camera = new GameObject("Camera", new Vector3(0, 18, 100), Quaternion.Identity, Vector3.One);
            Camera cameraComp = new Camera(45, graphics.GraphicsDevice.Viewport.AspectRatio, 1, 1000);
            camera.AddComponent(cameraComp);
            Collider cameraCol = camera.AddNewComponent<Collider>();
            cameraCol.bound = new Engine.Bounding_Volumes.BoundingSphere(cameraCol, Vector3.Zero, 18.0f);
            scene = new Scene(camera);
            robot = new GameObject("Robot", new Vector3(0, 15, 0), Quaternion.Identity, Vector3.One, scene);
            Collider robotCol = robot.AddNewComponent<Collider>();
            robotCol.bound = new Engine.Bounding_Volumes.BoundingBox(robotCol, new Vector3(Vector2.Zero, 5.0f), 5.0f*Vector3.One);
            Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
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
            groundTexture = Content.Load<Texture2D>("Textures/Grass");
            Model model = Content.Load<Model>("Models/Model");
            crosshairTexture = Content.Load<Texture2D>("Textures/Crosshair");
            treeTexture = Content.Load<Texture2D>("Textures/Tree");
            font = Content.Load<SpriteFont>("Textures/Arial");

            MeshInstance mesh = new MeshInstance(model, null);
            robot.AddComponent(mesh);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            scene.Camera.Update(gameTime);
            collision = scene.Camera.GetComponent<Collider>().Collide(robot.GetComponent<Collider>());
            if (collision != 0)
            {
                scene.Camera.RevertLastMovement();
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

            robot.Draw(gameTime);

                    RasterizerState originalState = GraphicsDevice.RasterizerState;

                    RasterizerState rasterizerState = new RasterizerState();
                    rasterizerState.FillMode = FillMode.WireFrame;
                    GraphicsDevice.RasterizerState = rasterizerState;

            BasicEffect effect = new BasicEffect(graphics.GraphicsDevice);
            effect.World = Matrix.Identity;
            effect.View = scene.Camera.GetComponent<Camera>().ViewMatrix;
            effect.Projection = scene.Camera.GetComponent<Camera>().ProjectionMatrix;
            effect.CurrentTechnique.Passes[0].Apply();

            Engine.Bounding_Volumes.BoundingBox box = (robot.GetComponent<Collider>().bound as Engine.Bounding_Volumes.BoundingBox);
            Vector3[] corners = box.Corners();      

                    VertexPosition[] vertices = new VertexPosition[8];
                    for(int i = 0; i < 8; ++i)
                    {
                        vertices[i].Position = corners[i];
                    }
                    short[] indexes = new short[24]
                    {
                        0, 1, 1, 2, 2, 3, 3, 0,
                        4, 5, 5, 6, 6, 7, 7, 4,
                        0, 4, 1, 5, 2, 6, 3, 7
                    };
                    
                    graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPosition>(PrimitiveType.LineList, 
                                                                                      vertices, 0, 8,
                                                                                      indexes, 0, 12);
                    GraphicsDevice.RasterizerState = originalState;

            DrawSprite(200, 200, Vector3.Zero, new Vector3(-90, 0, 0), groundTexture, 20, false);
            for (int i = -1; i < 2; i++)
                DrawSprite(30, 30, new Vector3(100 * i, 30, -150), new Vector3(0, 0, 0), treeTexture, 1, true);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
            spriteBatch.Draw(crosshairTexture, new Vector2((graphics.PreferredBackBufferWidth / 2) - (crosshairTexture.Width / 2), (graphics.PreferredBackBufferHeight / 2) - (crosshairTexture.Height / 2)));
            spriteBatch.DrawString(font, scene.Camera.LocalPosition.ToString(), new Vector2(50, 50), Color.Black);
            spriteBatch.DrawString(font, scene.Camera.LocalQuaternionRotation.ToString(), new Vector2(50, 100), Color.Black);
            spriteBatch.DrawString(font, scene.Camera.LocalEulerRotation.ToString(), new Vector2(50, 150), Color.Black);
            spriteBatch.DrawString(font, collision.ToString(), new Vector2(50, 200), Color.Black);
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
