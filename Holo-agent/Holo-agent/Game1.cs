//#define DRAW_DEBUG_WIREFRAME

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine;
using Engine.Components;
using Engine.Utilities;
using Microsoft.Xna.Framework.Audio;

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
        GameObject pistol;
        GameObject machineGun;
        GameObject gunfire1, gunfire2;
        GameObject floor;
        Weapon weapon1, weapon2;
        float timer;
        const float TIMER = 1;
        SoundEffect shot;
        Texture2D gunfireTexture;
        int collision = 0;
        Texture2D floorTexture;
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            // Uncomment to enable 60+ FPS.
            /*graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;*/
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Input.Initialize();
            // TODO: Add your initialization logic here
            frameCounter = new FrameCounter();
            columns = new GameObject[8];
            walls = new GameObject[7];
            doors = new GameObject[2];
            scene = new Scene();
            player = new GameObject("Player", new Vector3(30, 18, -25), Quaternion.Identity, Vector3.One, scene);
            player.AddNewComponent<PlayerController>();
            Collider playerCol = player.AddNewComponent<Collider>();
            playerCol.bound = new Engine.Bounding_Volumes.BoundingBox(playerCol, Vector3.Zero, 10f * Vector3.One);
            GameObject camera = new GameObject("Camera", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, player);
            Camera cameraComp = new Camera(45, graphics.GraphicsDevice.Viewport.AspectRatio, 1, 1000);
            camera.AddComponent(cameraComp);
            scene.Camera = camera;
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
                Collider wallCol = walls[i].AddNewComponent<Collider>();
                wallCol.bound = new Engine.Bounding_Volumes.BoundingBox(wallCol, new Vector3(0, 0, 1.5f), new Vector3(60, 60, 2));
            }
            for(int i = 4; i < 6; ++i)
            {
                walls[i] = new GameObject("Wall" + i, new Vector3(200 - 320*(i%2), 30, -180), Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(90), 0, 0), new Vector3(3.75f, 0.5f, 1), scene);
                Collider wallCol = walls[i].AddNewComponent<Collider>();
                wallCol.bound = new Engine.Bounding_Volumes.BoundingBox(wallCol, new Vector3(1.5f, 0, 0), new Vector3(60, 60, 2));
            }
            walls[6] = new GameObject("Ceiling", new Vector3(40, 60, -180), Quaternion.CreateFromYawPitchRoll(0, MathHelper.ToRadians(270), 0), new Vector3(2.7f, 3.66f, 1f), scene);
            for(int i = 0; i < 2; ++i)
            {
                doors[i] = new GameObject("Doors" + i, new Vector3(40, 30, 42.5f - ((i+1)%2)*442.5f), Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians((i%2)*180), 0, 0), new Vector3(0.1f, 0.165f, 0.1f), scene);
                Collider doorCol = doors[i].AddNewComponent<Collider>();
                doorCol.bound = new Engine.Bounding_Volumes.BoundingBox(doorCol, new Vector3(0, 0, 0), new Vector3(450, 180, 30));
                doors[i].AddNewComponent<DoorInteraction>();
            }
            floor = new GameObject("Floor", new Vector3(40, 0, -180), Quaternion.CreateFromYawPitchRoll(0, 0, MathHelper.ToRadians(90)), Vector3.One, scene);
            //pistol = new GameObject("Pistol", new Vector3(8, 9, -9), Quaternion.Identity, Vector3.One, scene, player);
            machineGun = new GameObject("MachineGun", new Vector3(3, -1.75f, -5), Quaternion.Identity, Vector3.One, scene, player);
            //gunfire1 = new GameObject("Gunfire1", new Vector3(-3.5f, -10.5f, -9), Quaternion.Identity, Vector3.One, scene, pistol);
            gunfire2 = new GameObject("Gunfire2", new Vector3(0, 1, -9), Quaternion.Identity, Vector3.One, scene, machineGun);
            //pistol.AddComponent(new Weapon(WeaponTypes.Pistol, 12, 28, 12, 240, 1000));
            machineGun.AddComponent(new Weapon(WeaponTypes.MachineGun, 32, 72, 32, 640, 1000));
            timer = 1;
            //weapon1 = pistol.GetComponent<Weapon>();
            weapon2 = machineGun.GetComponent<Weapon>();
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
            Model columnModel = Content.Load<Model>("Models/column_001");
            floorTexture = Content.Load<Texture2D>("Textures/Ground");
            gunfireTexture = Content.Load<Texture2D>("Textures/Gunfire");
            crosshairTexture = Content.Load<Texture2D>("Textures/Crosshair");
            font = Content.Load<SpriteFont>("Textures/Arial");
            shot = Content.Load<SoundEffect>("Sounds/Pistol");

            for (int i = 0; i < 8; ++i)
            {
                columns[i].AddComponent(new MeshInstance(columnModel, null));
            }

            Model ladderModel = Content.Load<Model>("Models/ladder");
            ladder.AddComponent(new MeshInstance(ladderModel, null));
            Model tileModel = Content.Load<Model>("Models/panel_ceiling");
            tile.AddComponent(new MeshInstance(tileModel, null));
            Model playerModel = Content.Load<Model>("Models/animacja_bieg");
            player.GetComponent<PlayerController>().PlayerMesh = new MeshInstance(playerModel, null);
            for (int i = 0; i < 7; ++i)
            {
                walls[i].AddComponent(new MeshInstance(tileModel, null));
            }
            Model doorModel = Content.Load<Model>("Models/door_001");
            for (int i = 0; i < 2; ++i)
            {
                doors[i].AddComponent(new MeshInstance(doorModel, null));
            }
            floor.AddComponent(new SpriteInstance(floorTexture, new Vector3(0, 160, 220), 20, new BasicEffect(graphics.GraphicsDevice), graphics));
            //gunfire1.AddComponent(new SpriteInstance(gunfireTexture, new Vector3(0, 5, 5), 1, new BasicEffect(graphics.GraphicsDevice), graphics));
            //gunfire1.GetComponent<SpriteInstance>().Enabled = false;
            //Model pistolModel = Content.Load<Model>("Models/Pistol");
            //pistol.AddComponent(new MeshInstance(pistolModel, null));
            gunfire2.AddComponent(new SpriteInstance(gunfireTexture, new Vector3(0, 5, 5), 1, new BasicEffect(graphics.GraphicsDevice), graphics));
            gunfire2.GetComponent<SpriteInstance>().Enabled = false;
            Model machineGunModel = Content.Load<Model>("Models/Machine_Gun");
            machineGun.AddComponent(new MeshInstance(machineGunModel, null));
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

            Input.Update(gameTime, graphics);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            scene.Update(gameTime);
            for (int i = 0; i < 8; ++i)
            {
                collision = player.GetComponent<Collider>().Collide(columns[i].GetComponent<Collider>());
                if (collision != 0)
                {
                    player.GetComponent<PlayerController>().Revert();
                }
            }
            for (int i = 0; i < 6; ++i)
            {
                collision = player.GetComponent<Collider>().Collide(walls[i].GetComponent<Collider>());
                if (collision != 0)
                {
                    player.GetComponent<PlayerController>().Revert();
                }
            }
            for (int i = 0; i < 2; ++i)
            {
                collision = player.GetComponent<Collider>().Collide(doors[i].GetComponent<Collider>());
                if (collision != 0)
                {
                    player.GetComponent<PlayerController>().Revert();
                }
            }
            timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            scene.Draw(gameTime);

#if DRAW_DEBUG_WIREFRAME
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
            for (int i = 0; i < 6; ++i)
            {
                Engine.Bounding_Volumes.BoundingBox box = (walls[i].GetComponent<Collider>().bound as Engine.Bounding_Volumes.BoundingBox);
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
            for (int i = 0; i < 2; ++i)
            {
                Engine.Bounding_Volumes.BoundingBox box = (doors[i].GetComponent<Collider>().bound as Engine.Bounding_Volumes.BoundingBox);
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

            //Ray
            VertexPosition[] line = new VertexPosition[2];
            line[0].Position = player.GlobalPosition - new Vector3(0, 1, 0);
            line[1].Position = line[0].Position + player.LocalToWorldMatrix.Forward * 100.0f;
            graphics.GraphicsDevice.DrawUserPrimitives<VertexPosition>(PrimitiveType.LineList, line, 0, 1);

            GraphicsDevice.RasterizerState = originalState;
#endif
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
            if (player.GetComponent<PlayerController>() != null)
                spriteBatch.Draw(crosshairTexture, new Vector2((graphics.PreferredBackBufferWidth / 2) - (crosshairTexture.Width / 2), (graphics.PreferredBackBufferHeight / 2) - (crosshairTexture.Height / 2)), player.GetComponent<PlayerController>().CrosshairColor);
            /*if (player.GetChild("Pistol") != null)
            {
                spriteBatch.DrawString(font, weapon1.getMagazine() + "/" + weapon1.getAmmo(), new Vector2(10, 410), Color.Red, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
                if (weapon1.info != null)
                    spriteBatch.DrawString(font, weapon1.info, new Vector2(50, 95), Color.SeaGreen);
                if (weapon1.getGunfire())
                {
                    timer = TIMER;
                    if (timer >= 0)
                        gunfire1.GetComponent<SpriteInstance>().Draw(gameTime);
                    weapon1.setGunfire(false);
                    shot.Play();
                }
            }*/
            if (player.GetChild("MachineGun") != null)
            {
                spriteBatch.DrawString(font, weapon2.getMagazine() + "/" + weapon2.getAmmo(), new Vector2(10, 410), Color.Red, 0, Vector2.Zero, 4, SpriteEffects.None, 0);
                if (weapon2.info != null)
                    spriteBatch.DrawString(font, weapon2.info, new Vector2(50, 95), Color.SeaGreen);
                if (weapon2.getGunfire())
                {
                    timer = TIMER;
                    if (timer >= 0)
                        gunfire2.GetComponent<SpriteInstance>().Draw(gameTime);
                    weapon2.setGunfire(false);
                    shot.Play();
                }
            }
            /*spriteBatch.DrawString(font, scene.Camera.GlobalPosition.ToString(), new Vector2(50, 50), Color.Black);
            spriteBatch.DrawString(font, scene.Camera.GlobalRotation.ToString(), new Vector2(50, 75), Color.Black);
            spriteBatch.DrawString(font, player.GlobalPosition.ToString(), new Vector2(50, 100), Color.Black);
            spriteBatch.DrawString(font, player.GlobalRotation.ToString(), new Vector2(50, 125), Color.Black);*/
            spriteBatch.DrawString(font, frameCounter.AverageFramesPerSecond.ToString(), new Vector2(50, 50), Color.Black);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
