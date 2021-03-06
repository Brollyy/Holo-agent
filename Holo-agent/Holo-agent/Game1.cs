﻿#define DRAW_DEBUG_WIREFRAME

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Engine;
using Engine.Components;
using Engine.Utilities;
using Microsoft.Xna.Framework.Audio;
using Animation;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Storage;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Holo_agent
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        RenderTarget2D renderTarget;
        RenderTarget2D screenRenderTarget;
        Effect hologramRecordingShader;
        Effect healthShader;
        Effect bloomShader;
        Effect screenLightingShader;
        Effect hitShader;
        float gammaValue = 1.0f;
        float brightnessValue = 0.0f;
        float contrastValue = 0.0f;
        float hitTime = 0.0f;
        Effect pauseMenuShader;
        Effect gameOverShader;
        Texture2D healthShaderTexture;
        Texture2D crosshair;
        SpriteFont dialogueFont, interfaceFont;
        FrameCounter frameCounter;
        Scene scene;
        GameObject level;
        GameObject player;
        GameObject enemy;
        GameObject enemy2;
        GameObject enemy3;
        GameObject gunfire;
        GameObject bench;
        GameObject bench1;
        GameObject bench2;
        GameObject bench3;
        GameObject bench4;
        GameObject column2;
        GameObject bench7;
        GameObject bench8;
        GameObject bench9;
        GameObject biurko1;
        GameObject krzesełko;
        GameObject mirror;
        List<GameObject> propsRoom5;
        List<GameObject> doors;
        List<SpriteInstance> particles;
        GameObject particleFireEmitter, particleExplosionEmitter, particleSmokeEmitter, particleBloodEmitter;
        List<GameObject> weapons;
        List<GameObject> gunfires;
        List<Collider> weaponColliders;
        Weapon weapon;
        float /*emitterTimer,*/ objectiveTimer;
        float hologramRecordingTimer = 0.0f;
        float hologramRecordingMaxTime;
        SoundEffect shot;
        SoundEffect dialog1;
        SoundEffect dialog2;
        SoundEffect dialog3a;
        SoundEffect dialog3b;
        SoundEffect dialog3c;
        SoundEffect dialog4a;
        SoundEffect dialog4b;
        int p_dialog1 = 0;
        SoundEffectInstance injurySound;
        List<SoundEffectInstance> stepsSounds;
        Texture2D gunfireTexture;
        Texture2D floorTexture;
        GameState gameState = GameState.Intro;
        GameMenu gameMenu;
        Vector2 objectivePosition = Vector2.UnitY * -500;
        string objectiveString = "[Uratuj informatorów]";
        private double? startTime;
        StorageDevice device;
        Type[] knownTypes;
        private float playerHealth;

        public float PlayerHealth
        {
            get
            {
                return playerHealth;
            }
        }
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            gameMenu = new GameMenu();
            knownTypes = new Type[]
            {
                typeof(AIController), typeof(AnimationController), typeof(Camera), typeof(CharacterController), typeof(Collider),
                typeof(DoorInteraction), typeof(EnemyController), typeof(HologramPlayback), typeof(HologramRecorder), typeof(Interaction),
                typeof(MeshInstance), typeof(ParticleSystem), typeof(PlayerController), typeof(Rigidbody), typeof(SpriteInstance),
                typeof(Weapon), typeof(WeaponInteraction), typeof(Engine.Bounding_Volumes.BoundingVolume), typeof(Engine.Bounding_Volumes.BoundingBox),
                typeof(Engine.Bounding_Volumes.BoundingSphere), typeof(Vector3), typeof(GameObject)
            };
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
            InitializeGame();
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            LoadGame();
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
            gameState = gameMenu.Update(gameState, this, gameTime);
            if(gameState.Equals(GameState.Intro))
            {
                if (IsMouseVisible)
                    IsMouseVisible = false;
            }
            if(gameState.Equals(GameState.Credits))
            {
                if (IsMouseVisible)
                    IsMouseVisible = false;
            }
            if (gameState.Equals(GameState.Menu))
            {
                if (!IsMouseVisible)
                    IsMouseVisible = true;
            }
            if (gameState.Equals(GameState.Keypad))
            {
                if (!IsMouseVisible)
                    IsMouseVisible = true;
            }
            if (gameState.Equals(GameState.Pause))
            {
                if (!IsMouseVisible)
                    IsMouseVisible = true;
            }
            if(gameState.Equals(GameState.GameOver))
            {
                IsMouseVisible = true;
            }
            if (gameState.Equals(GameState.Game))
            {
                if (IsMouseVisible)
                {
                    IsMouseVisible = false;
                    Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
                }
                frameCounter.Update(gameTime);
                Input.Update(gameTime, graphics);
                scene.Update(gameTime);
                if(scene.hitLastFrame)
                {
                    hitTime = -2.0f;
                    scene.hitLastFrame = false;
                }

                if (objectiveTimer > 0)
                    objectiveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                else
                    objectivePosition = Vector2.Lerp(objectivePosition, new Vector2(objectivePosition.X, -250), 0.05f);
                if (player.GetComponent<PlayerController>() != null)
                {
                    weapon = player.GetComponent<PlayerController>().getWeapon();
                    playerHealth = player.GetComponent<PlayerController>().Health;
                }
                if (weapon != null)
                    gunfire = weapon.getGunfireInstance();
                /*emitterTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (emitterTimer >= 6)
                {
                    particleFireEmitter.GetComponent<ParticleSystem>().Destroy();
                    particleFireEmitter.Destroy();
                    particleSmokeEmitter.GetComponent<ParticleSystem>().Destroy();
                    particleSmokeEmitter.Destroy();
                    emitterTimer = 0;
                }*/
                /*if (particleBloodEmitter.GetComponent<ParticleSystem>().getParticlesCount() == 0)
                    particleBloodEmitter.GetComponent<ParticleSystem>().Init();
                if (particleExplosionEmitter.GetComponent<ParticleSystem>().getParticlesCount() == 0)
                    particleExplosionEmitter.GetComponent<ParticleSystem>().Init();*/
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicWrap;
            GraphicsDevice.SetRenderTarget(screenRenderTarget);
            if(gameState.Equals(GameState.Intro))
            {
                GraphicsDevice.Clear(Color.Black);
                gameMenu.Draw(spriteBatch, graphics);
            }
            if(gameState.Equals(GameState.Credits))
            {
                GraphicsDevice.Clear(Color.Black);
                gameMenu.Draw(spriteBatch, graphics);
            }
            if (gameState.Equals(GameState.Menu))
            {
                GraphicsDevice.Clear(Color.Black);
                gameMenu.Draw(spriteBatch, graphics);
            }
            if (gameState.Equals(GameState.Pause))
            {
                GraphicsDevice.Clear(Color.Transparent);
                Texture2D texture = DrawSceneToTexture(renderTarget, gameTime);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                pauseMenuShader.Parameters["ScreenTexture"].SetValue(texture);
                pauseMenuShader.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(pauseMenuShader.Parameters["ScreenTexture"].GetValueTexture2D(), new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                spriteBatch.End();
                gameMenu.Draw(spriteBatch, graphics);
            }
            if (gameState.Equals(GameState.Keypad))
            {
                GraphicsDevice.Clear(Color.Transparent);
                Texture2D texture = DrawSceneToTexture(renderTarget, gameTime);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                pauseMenuShader.Parameters["ScreenTexture"].SetValue(texture);
                pauseMenuShader.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(pauseMenuShader.Parameters["ScreenTexture"].GetValueTexture2D(), new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                spriteBatch.End();
                gameMenu.Draw(spriteBatch, graphics);
            }
            if (gameState.Equals(GameState.GameOver))
            {
                GraphicsDevice.Clear(Color.Transparent);
                Texture2D texture = DrawSceneToTexture(renderTarget, gameTime);
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                gameOverShader.Parameters["ScreenTexture"].SetValue(texture);
                gameOverShader.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(gameOverShader.Parameters["ScreenTexture"].GetValueTexture2D(), new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                healthShader.CurrentTechnique.Passes[0].Apply();
                spriteBatch.Draw(healthShaderTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                spriteBatch.End();
                gameMenu.Draw(spriteBatch, graphics);
            }
            if (gameState.Equals(GameState.Game))
            {
                if (startTime == null) startTime = gameTime.TotalGameTime.TotalSeconds;
                GraphicsDevice.Clear(Color.Black);
                Texture2D texture = DrawSceneToTexture(renderTarget, gameTime);
                Texture2D mirrorTexture = DrawSceneToTexture(renderTarget, gameTime);
                Texture2D bloomTexture;
                if (player.GetComponent<PlayerController>() == null)
                {
                    if (hologramRecordingTimer < hologramRecordingMaxTime)
                    {
                        hologramRecordingTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        hologramRecordingShader.Parameters["RecordingTime"].SetValue(hologramRecordingTimer / hologramRecordingMaxTime);
                    }
                }
                else
                {
                    if (hologramRecordingTimer > 0.0f)
                    {
                        hologramRecordingTimer = 0.0f;
                        hologramRecordingShader.Parameters["RecordingTime"].SetValue(0.0f);
                    }
                }

                if (player.GetComponent<PlayerController>() == null && hologramRecordingTimer > 0.0f)
                {
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, hologramRecordingShader);
                    spriteBatch.Draw(texture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.End();
                }
                else
                {
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                    bloomShader.Parameters["ScreenTexture"].SetValue(texture);
                    bloomShader.CurrentTechnique.Passes[0].Apply();
                    bloomTexture = bloomShader.Parameters["ScreenTexture"].GetValueTexture2D();
                    spriteBatch.Draw(bloomTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.End();
                }

                if (player.GetComponent<PlayerController>() != null)
                {
                    healthShader.Parameters["Health"].SetValue(player.GetComponent<PlayerController>().Health);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, healthShader);
                    spriteBatch.Draw(healthShaderTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.End();
                }
                if(hitTime < 0.0f)
                {
                    hitShader.Parameters["TimeFromHit"].SetValue(hitTime);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, hitShader);
                    spriteBatch.Draw(texture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.End();
                    hitTime += (float)gameTime.ElapsedGameTime.TotalSeconds * 2;


                }

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                Point w = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                Dialogues.Draw(ref spriteBatch, dialogueFont, w, gameTime);
                if (enemy.GetComponent<EnemyController>() != null)
                {
                    Weapon enemyWeapon = enemy.GetComponent<EnemyController>().Weapon.GetComponent<Weapon>();
                    if (enemyWeapon != null)
                    {
                        GameObject gunfireInstance = enemyWeapon.getGunfireInstance();
                        if (gunfireInstance != null) gunfireInstance.GetInactiveComponent<SpriteInstance>().Draw(gameTime);
                    }
                }
                if (enemy2.GetComponent<EnemyController>() != null)
                {
                    Weapon enemyWeapon = enemy2.GetComponent<EnemyController>().Weapon.GetComponent<Weapon>();
                    if (enemyWeapon != null)
                    {
                        GameObject gunfireInstance = enemyWeapon.getGunfireInstance();
                        if (gunfireInstance != null) gunfireInstance.GetInactiveComponent<SpriteInstance>().Draw(gameTime);
                    }
                }
                if (enemy3.GetComponent<EnemyController>() != null)
                {
                    Weapon enemyWeapon = enemy3.GetComponent<EnemyController>().Weapon.GetComponent<Weapon>();
                    if (enemyWeapon != null)
                    {
                        GameObject gunfireInstance = enemyWeapon.getGunfireInstance();
                        if (gunfireInstance != null) gunfireInstance.GetInactiveComponent<SpriteInstance>().Draw(gameTime);
                    }
                }
         
                if (player.GetComponent<PlayerController>() != null)
                {
                    if (weapon != null)
                    {
                        string weaponInfo = weapon.getMagazine() + "/" + weapon.getAmmo();
                        Vector2 weaponInfoSize = 0.5f * interfaceFont.MeasureString(weaponInfo);
                        spriteBatch.DrawString(interfaceFont, weaponInfo, new Vector2(w.X - 1.05f * weaponInfoSize.X, w.Y - 1.05f * weaponInfoSize.Y), Color.Red, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                        GameObject gunfireInstance = weapon.getGunfireInstance();
                        if (gunfireInstance != null)
                            gunfireInstance.GetInactiveComponent<SpriteInstance>().Draw(gameTime);
                    }
                    int selectedPath = player.GetComponent<PlayerController>().SelectedPath;
                    int selectedPlaying = player.GetComponent<PlayerController>().PlayingPath;
                    bool previewing = player.GetComponent<PlayerController>().HologramPreviewing;
                    bool playing = player.GetComponent<PlayerController>().HologramPlaying;
                    for (int i = 0; i < 3; ++i)
                    {
                        Color color = Color.Black;
                        bool recorded = player.GetComponent<PlayerController>().IsPathRecorded(i);
                        float cooldown = player.GetComponent<PlayerController>().PathCooldown(i);
                        string desc = (i + 1) + ": ";
                        if (playing && selectedPlaying == i) desc += "Playing for " + ((float)(int)(cooldown * 10)) / 10 + "s";
                        else if (previewing && selectedPath == i) desc += "Preview";
                        else if (recorded)
                        {
                            if (cooldown > 0.0f) desc += "Ready in " + ((float)(int)(cooldown * 10)) / 10 + "s";
                            else desc += "Ready";
                        }
                        else desc += "Empty";

                        if (selectedPath == i)
                        {
                            if (playing && selectedPlaying == selectedPath) color = Color.Red;
                            else if (previewing) color = Color.LightBlue;
                            else color = Color.White;
                        }
                        else
                        {
                            if (playing && selectedPlaying == i) color = Color.DarkRed;
                            else color = Color.Black;
                        }
                        Vector2 descSize = 0.2f * interfaceFont.MeasureString(desc);
                        spriteBatch.DrawString(interfaceFont, desc, new Vector2(0.02f * w.X, w.Y - (0.2f * descSize.Y + 1.05f * (3 - i) * descSize.Y)), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
                    }
                    spriteBatch.Draw(crosshair, new Vector2((graphics.PreferredBackBufferWidth / 2) - (crosshair.Width / 2), (graphics.PreferredBackBufferHeight / 2) - (crosshair.Height / 2)), player.GetComponent<PlayerController>().CrosshairColor);
                    if (player.GetComponent<PlayerController>().CrosshairColor == Color.Lime)
                    {
                        string message = "Naciśnij F aby ";
                        Interaction inter = player.GetComponent<PlayerController>().ClosestObject.GetComponent<Interaction>();
                        if (inter is DoorInteraction) message += "otworzyć drzwi.";
                        if (inter is WeaponInteraction) message += "podnieść broń.";
                        if (inter is KeypadInteraction) message += "wpisać kod.";
                        spriteBatch.DrawString(interfaceFont, message, new Vector2(w.X / 2 - 0.25f * interfaceFont.MeasureString(message).X / 2, 0.6f * w.Y), Color.Orange, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
                    }
                    Minimap.Draw(ref spriteBatch);
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Tab))
                {
                    Vector2 objectiveSize = 0.25f * interfaceFont.MeasureString(objectiveString);
                    objectivePosition = new Vector2(w.X / 2 - objectiveSize.X / 2, 0.2f * w.Y - objectiveSize.Y / 2);
                    objectiveTimer = 2;
                }
                spriteBatch.DrawString(interfaceFont, objectiveString, objectivePosition, Color.Orange, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
#if DRAW_DEBUG_WIREFRAME
                spriteBatch.DrawString(interfaceFont, player.GlobalPosition.ToString(), new Vector2(5, 5), Color.Green, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
#endif
                spriteBatch.End();
                //

            }

            GraphicsDevice.SetRenderTarget(null);
            screenLightingShader.Parameters["Gamma"].SetValue(gammaValue);
            screenLightingShader.Parameters["Brightness"].SetValue(brightnessValue);
            screenLightingShader.Parameters["Contrast"].SetValue(contrastValue);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, screenLightingShader);
            spriteBatch.Draw(screenRenderTarget, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void InitializeGame()
        {
            /*graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();*/

            Input.Initialize();
            Input.BindActionPress(GameAction.SAVE, SaveGame);
            Input.BindActionPress(GameAction.LOAD, LoadGame);
            Input.BindActionContinuousPress(GameAction.GAMMA_UP, ChangeGamma);
            Input.BindActionContinuousPress(GameAction.GAMMA_DOWN, ChangeGamma);
            Input.BindActionContinuousPress(GameAction.BRIGHTNESS_UP, ChangeBrightness);
            Input.BindActionContinuousPress(GameAction.BRIGHTNESS_DOWN, ChangeBrightness);
            Input.BindActionContinuousPress(GameAction.CONTRAST_UP, ChangeContrast);
            Input.BindActionContinuousPress(GameAction.CONTRAST_DOWN, ChangeContrast);
            // TODO: Add your initialization logic here
            frameCounter = new FrameCounter();
            weapons = new List<GameObject>();
            gunfires = new List<GameObject>();
            weaponColliders = new List<Collider>();
            doors = new List<GameObject>();
            propsRoom5 = new List<GameObject>();
            renderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                true,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            screenRenderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            scene = new Scene();
            //GameObject roomTemp = new GameObject("RoomTemp", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-10000, -10000, -10000), new Vector3(10000, 10000, 10000)));
            //GameObject roomTemp = new GameObject("RoomTemp", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-10000, -10000, -10000), new Vector3(10000, 10000, 10000)));
            GameObject room = new GameObject("Room1", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(175, -5, -330), new Vector3(400, 100, 100)));
            GameObject room2 = new GameObject("Room2", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-400, -5, -425), new Vector3(190, 100, -70)));
            GameObject room3 = new GameObject("Room3", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-185, -5, -75), new Vector3(130, 40, 90)));
            GameObject room4 = new GameObject("Room4", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-265, -65, -10), new Vector3(-180, 40, 40)));
            GameObject room5 = new GameObject("Room5", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-300, -65, 35), new Vector3(-180, -25, 230)));
            GameObject room6 = new GameObject("Room6", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-178, -65, 100), new Vector3(5, -25, 300)));
            GameObject room7 = new GameObject("Room7", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-3, -170, 170), new Vector3(40, -25, 215)));
            GameObject room8 = new GameObject("Room8", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-1500, -170, -300), new Vector3(500, -130, 1150)));
            //GameObject room9 = new GameObject("Room9", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-175, -250, 218), new Vector3(-110, -130, 265)));
            //GameObject room10 = new GameObject("Room10", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-255, -250, 260), new Vector3(-10, -170, 420)));
            //GameObject room11 = new GameObject("Room11", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-20, -250, 255), new Vector3(160, -210, 510)));
            //GameObject room12 = new GameObject("Room12", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-60, -250, 440), new Vector3(120, -170, 570)));
            //GameObject room13 = new GameObject("Room13", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-160, -250, 400), new Vector3(-55, -170, 620)));
            level = new GameObject("Level", new Vector3(-100, -226, 550), Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(-1000 * Vector3.One, 1000 * Vector3.One));

            scene.AddRoomConnection(room, room2, new BoundingBox());
            scene.AddRoomConnection(room2, room3, new BoundingBox());
            scene.AddRoomConnection(room3, room4, new BoundingBox());
            scene.AddRoomConnection(room4, room5, new BoundingBox());
            scene.AddRoomConnection(room5, room6, new BoundingBox());
            scene.AddRoomConnection(room6, room7, new BoundingBox());
            scene.AddRoomConnection(room7, room8, new BoundingBox());
           // scene.AddRoomConnection(room8, room9, new BoundingBox());
           // scene.AddRoomConnection(room9, room10, new BoundingBox());
           // scene.AddRoomConnection(room10, room11, new BoundingBox());
           // scene.AddRoomConnection(room11, room12, new BoundingBox());
           // scene.AddRoomConnection(room12, room13, new BoundingBox());
           // scene.AddRoomConnection(room13, room10, new BoundingBox());

            (new GameObject("Floor1_1", new Vector3(287.5f, -5, -130), Quaternion.Identity, Vector3.One, scene, room, new BoundingBox(new Vector3(-112.5f, -5, -200), new Vector3(112.5f, 6, 165)))).AddNewComponent<Collider>();
            (new GameObject("Floor2_1", new Vector3(10, -5f, -230), Quaternion.Identity, Vector3.One, scene, room2, new BoundingBox(new Vector3(-400, -5f, -155), new Vector3(180, 6f, 155)))).AddNewComponent<Collider>();
            (new GameObject("Floor3_1", new Vector3(-87.5f, -5f, -12.5f), Quaternion.Identity, Vector3.One, scene, room3, new BoundingBox(new Vector3(-97.5f, -5, -65), new Vector3(97.5f, 6, 52.5f)))).AddNewComponent<Collider>();
            (new GameObject("Floor4_1", new Vector3(-207.5f, -5f, -4.25f), Quaternion.Identity, Vector3.One, scene, room4, new BoundingBox(new Vector3(-40, 1, -5), new Vector3(22.5f, 6, 5)))).AddNewComponent<Collider>();
            (new GameObject("Floor4_2", new Vector3(-194, -10f, 12.5f), Quaternion.CreateFromAxisAngle(Vector3.Right, (float)Math.PI/6), Vector3.One, scene, room4, new BoundingBox(new Vector3(-9, -3, -15f), new Vector3(9, 3, 12.5f)))).AddNewComponent<Collider>();
            (new GameObject("Floor4_5", new Vector3(-207.5f, -20f, 28f), Quaternion.Identity, Vector3.One, scene, room4, new BoundingBox(new Vector3(-40, 1, -5), new Vector3(22.5f, 6, 5)))).AddNewComponent<Collider>();
            (new GameObject("Floor4_3", new Vector3(-194, -55f, 16.5f), Quaternion.CreateFromAxisAngle(Vector3.Right, (float)Math.PI / 4.5f), Vector3.One, scene, room4, new BoundingBox(new Vector3(-9, -3, -15f), new Vector3(9, 3, 12.5f)))).AddNewComponent<Collider>();
            (new GameObject("Floor4_4", new Vector3(-221, -30f, 12.5f), Quaternion.CreateFromAxisAngle(Vector3.Right, (float)(-1*Math.PI / 6)), Vector3.One, scene, room4, new BoundingBox(new Vector3(-9, -3, -15f), new Vector3(9, 3, 12.5f)))).AddNewComponent<Collider>();
            (new GameObject("Floor4_6", new Vector3(-207.5f, -45f, -4.25f), Quaternion.Identity, Vector3.One, scene, room4, new BoundingBox(new Vector3(-40, 1, -5), new Vector3(22.5f, 6, 5)))).AddNewComponent<Collider>();
            (new GameObject("Floor5_1", new Vector3(-285f, -67f, -10f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(0, 0, 0), new Vector3(300, 5, 400)))).AddNewComponent<Collider>();
            (new GameObject("Floor8_1", new Vector3(-1085f, -170f, 150f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-1000, 0, -1000), new Vector3(2000, 5, 2000)))).AddNewComponent<Collider>();
            (new GameObject("Wall1_1", new Vector3(330, 5, -20.0f), Quaternion.Identity, Vector3.One, scene, room, new BoundingBox(new Vector3(-60, -5, -7.5f), new Vector3(60, 60, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall1_2", new Vector3(287, 5, -110f), Quaternion.Identity, Vector3.One, scene, room, new BoundingBox(new Vector3(-110, -5, -106f), new Vector3(11, 60, 95f)))).AddNewComponent<Collider>();
            (new GameObject("Wall1_3", new Vector3(377.5f, 5, -110f), Quaternion.Identity, Vector3.One, scene, room, new BoundingBox(new Vector3(-11, -5, -224f), new Vector3(10, 60, 90f)))).AddNewComponent<Collider>();
            (new GameObject("Wall1_4", new Vector3(280f, 5, -330f), Quaternion.Identity, Vector3.One, scene, room, new BoundingBox(new Vector3(-90, -5, -7.5f), new Vector3(110, 60, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall2_1", new Vector3(185, 5, -130f), Quaternion.Identity, Vector3.One, scene, room2, new BoundingBox(new Vector3(-7.5f, -5, -106f), new Vector3(11, 85, 90f)))).AddNewComponent<Collider>();
            (new GameObject("Wall2_2", new Vector3(185, 5, -372.5f), Quaternion.Identity, Vector3.One, scene, room2, new BoundingBox(new Vector3(-7.5f, -5, -106f), new Vector3(11, 85, 90f)))).AddNewComponent<Collider>();
           (new GameObject("Wall2_3", new Vector3(170, 5, -80f), Quaternion.Identity, Vector3.One, scene, room2, new BoundingBox(new Vector3(-205f, -5, -7.5f), new Vector3(11, 85, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall2_4", new Vector3(-140, 5, -80f), Quaternion.Identity, Vector3.One, scene, room2, new BoundingBox(new Vector3(-350f, -5, -7.5f), new Vector3(61, 85, 5f)))).AddNewComponent<Collider>();
           // (new GameObject("Wall2_5", new Vector3(185, 5, -348f), Quaternion.Identity, Vector3.One, scene, room2, new BoundingBox(new Vector3(-600f, -5, -7.5f), new Vector3(51, 85, 7.5f)))).AddNewComponent<Collider>();
           (new GameObject("Wall2_6", new Vector3(-370, 5, -65f), Quaternion.Identity, Vector3.One, scene, room2, new BoundingBox(new Vector3(-7.5f, -5, -406f), new Vector3(11, 85, -16f)))).AddNewComponent<Collider>();
            (new GameObject("Wall3_1", new Vector3(5, 5, -36f), Quaternion.Identity, Vector3.One, scene, room3, new BoundingBox(new Vector3(-2f, -5, -52f), new Vector3(11, 85, 45f)))).AddNewComponent<Collider>();
            (new GameObject("Wall3_2", new Vector3(-91, 5, 40f), Quaternion.Identity, Vector3.One, scene, room3, new BoundingBox(new Vector3(-91f, -5, -7.5f), new Vector3(120, 85, 30.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall3_3", new Vector3(-196, 5, -16f), Quaternion.Identity, Vector3.One, scene, room3, new BoundingBox(new Vector3(-5f, -5, -56f), new Vector3(16, 85, 7f)))).AddNewComponent<Collider>();
            (new GameObject("Wall3_4", new Vector3(-210, 5, -16f), Quaternion.Identity, Vector3.One, scene, room3, new BoundingBox(new Vector3(-50f, -5, -12f), new Vector3(35, 85, 6f)))).AddNewComponent<Collider>();
            (new GameObject("Wall3_5", new Vector3(120, -10, -16f), Quaternion.Identity, Vector3.One, scene, room3, new BoundingBox(new Vector3(-20f, -5, -70f), new Vector3(35, 85, 90f)))).AddNewComponent<Collider>();
            (new GameObject("Wall3_6", new Vector3(60, 5, 73f), Quaternion.Identity, Vector3.One, scene, room3, new BoundingBox(new Vector3(-40f, -5, -7.5f), new Vector3(50, 85, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall4_1", new Vector3(0, 5, 80f), Quaternion.Identity, Vector3.One, scene, room4, new BoundingBox(new Vector3(-40f, -5, -7.5f), new Vector3(50, 85, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_1", new Vector3(-220, -65, 40f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-10f, -5, 0f), new Vector3(0, 85, 90f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_2", new Vector3(-175, -65, 40f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-10f, -5, 0f), new Vector3(0, 85, 60f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_3", new Vector3(-220, -65, 150f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-10f, -5, 0f), new Vector3(0, 85, 80f)))).AddNewComponent<Collider>();    
            (new GameObject("Wall5_4", new Vector3(-160, -65, 215f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-90f, -5, 0f), new Vector3(0, 85, 10f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_5", new Vector3(-100, -65, 215f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-35f, -5, 0f), new Vector3(65f, 85, 10f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_6", new Vector3(-90, -65, 165f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-36f, -5, -1f), new Vector3(96f, 85, 11f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_7", new Vector3(-178, -65, 155f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-6f, -5, -1f), new Vector3(1, 85, 21f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_8", new Vector3(-160, -65, 170f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-21f, -5, -1f), new Vector3(1, 85, 6f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_9", new Vector3(-120, -65, 110f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-10f, -5, 0f), new Vector3(0, 85, 70f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_10", new Vector3(-175, -65, 95f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-10f, -5, 0f), new Vector3(70, 85, 15f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_11", new Vector3(-205, -65, 30f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-15f, -5, 0f), new Vector3(0, 85, 10f)))).AddNewComponent<Collider>();
            (new GameObject("Wall5_12", new Vector3(10, -65, 214f), Quaternion.Identity, Vector3.One, scene, room5, new BoundingBox(new Vector3(-25f, -5, 0f), new Vector3(0, 85, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_1", new Vector3(10, -170, 235f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-254f, -5, -1f), new Vector3(0, 85, 385f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_2", new Vector3(10, -170, 155f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-495f, -5, -301f), new Vector3(0, 85, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_3", new Vector3(-230, -170, 235f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-257f, -5, -1f), new Vector3(-110, 85, 335f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_4", new Vector3(-465, -170, 222f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-292f, -5, 45f), new Vector3(-100, 85, 250f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_5", new Vector3(-755, -170, -83f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_6", new Vector3(-755, -170, 222f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_7", new Vector3(-335, -170, 520f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_8", new Vector3(-595, -170, 520f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_9", new Vector3(-845, -170, 520f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_10", new Vector3(-1088, -170, 520f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-99, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_11", new Vector3(-335, -170, 766f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_12", new Vector3(-595, -170, 766f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_13", new Vector3(-845, -170, 766f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_14", new Vector3(-1088, -170, 766f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-99, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_15", new Vector3(-1110, -170, 495f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-505f, -5, -601f), new Vector3(0, 85, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_16", new Vector3(-650, -170, -120f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-505f, -5, -601f), new Vector3(200, 85, -590f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_17", new Vector3(-1410, -170, 740f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-505f, -5, -601f), new Vector3(0, 85, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_18", new Vector3(-1410, -170, 1415f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-505f, -5, -601f), new Vector3(0, 85, 7.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_19", new Vector3(-650, -170, 1650f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-755f, -5, -601f), new Vector3(300, 85, -590f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_20", new Vector3(135, -170, 915f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-490f, -5, -301f), new Vector3(0, 85, 212.5f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_21", new Vector3(-465, -170, -73f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 49f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_22", new Vector3(-465, -170, 118f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-98, 85, 49f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_23", new Vector3(-465, -170, -78f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 44f), new Vector3(-289, 85, 251f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_23", new Vector3(-270, -170, 38f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 5f), new Vector3(-289, 85, 130f)))).AddNewComponent<Collider>();
            (new GameObject("Wall8_24", new Vector3(-270, -170, -28f), Quaternion.Identity, Vector3.One, scene, room8, new BoundingBox(new Vector3(-294f, -5, 0f), new Vector3(-289, 85, 35f)))).AddNewComponent<Collider>();
            (new GameObject("Trigger", new Vector3(330, 20, -40), Quaternion.Identity, Vector3.One, scene, room2, new BoundingBox(-20 * Vector3.One, 20 * Vector3.One))).AddComponent(new Collider(true, false, new TriggerAction(() =>
            {
                if (p_dialog1 == 0)
                {
                    p_dialog1 = 1;
                    //Dialogues.PlayDialogue(dialog1, 0.5f);
                    
                }
                
                Dialogues.PlayDialogue("Poruszaj się klawiszami W, S, A, D", 1, 5);
                Dialogues.PlayDialogue("Przytrzymaj Shift, by pobiec", 6.5f, 2.5f);
                Dialogues.PlayDialogue("Przytrzymaj C, by się skradać", 9.5f, 2);
                Dialogues.PlayDialogue("Strzel naciskając LPM", 12, 5);
                Dialogues.PlayDialogue("Naciśnij R, by przeładować broń", 17.5f, 4);
                Dialogues.PlayDialogue("Klawiszami 1, 2, 3 wybierz miejsce na hologram", 22, 5);
                Dialogues.PlayDialogue("Naciśnij Q by nagrać hologram na wybranym miejscu", 27.5f, 3);
                Dialogues.PlayDialogue("Naciśnij Q ponownie by zakończyć nagrywanie", 31, 3);
                Dialogues.PlayDialogue("Naciskając Z uruchomisz podgląd hologramu", 34.5f, 8);
                Dialogues.PlayDialogue("Naciśnij E by uruchomić hologram", 43, 3);
                Dialogues.PlayDialogue("Naciśnij E ponownie by zakonćzyć hologram", 46.5f, 5);
                Dialogues.PlayDialogue("Przytrzymaj Tab, by zobaczyć swój cel", 52, 7);
            })));

            doors.Add(new GameObject("Door1", new Vector3(330, 20, -40), Quaternion.Identity, Vector3.One, scene, room2));

            player = new GameObject("Player", new Vector3(330, 20, -40), Quaternion.Identity, Vector3.One, scene, room2);

            player.AddNewComponent<PlayerController>();
            player.AddComponent(new Rigidbody(80, 1.5f));
            player.GetComponent<Rigidbody>().GravityEnabled = false;
            Collider playerCol = player.AddNewComponent<Collider>();
            playerCol.bound = new Engine.Bounding_Volumes.BoundingBox(playerCol, new Vector3(0, -8f, 0), new Vector3(3, 9f, 6));
            GameObject camera = new GameObject("Camera", new Vector3(0, 0, 0), Quaternion.Identity, Vector3.One, scene, player, null, false);
            Camera cameraComp = new Camera(45, graphics.GraphicsDevice.Viewport.AspectRatio, 1, 1000);
            camera.AddComponent(cameraComp);
            scene.Camera = camera;
            weapons.Add(new GameObject("Pistol", new Vector3(20, 18, -40), Quaternion.Identity, 0.5f * Vector3.One, scene, room2));
            weaponColliders.Add(weapons[0].AddNewComponent<Collider>());
            weaponColliders[0].bound = new Engine.Bounding_Volumes.BoundingBox(weaponColliders[0], Vector3.Zero, new Vector3(0.5f, 0.75f, 2f));
            weapons[0].AddNewComponent<WeaponInteraction>();
            weapons.Add(new GameObject("MachineGun", new Vector3(40, 18, -40), Quaternion.Identity, Vector3.One, scene, room2));
            weaponColliders.Add(weapons[1].AddNewComponent<Collider>());
            weaponColliders[1].bound = new Engine.Bounding_Volumes.BoundingBox(weaponColliders[1], new Vector3(0, 0, -2f), new Vector3(0.5f, 1.5f, 2.5f));
            weapons[1].AddNewComponent<WeaponInteraction>();
            weapons.Add(new GameObject("MachineGun2", new Vector3(40, 18, -40), Quaternion.Identity, Vector3.One, scene, room5));
            weaponColliders.Add(weapons[2].AddNewComponent<Collider>());
            weaponColliders[2].bound = new Engine.Bounding_Volumes.BoundingBox(weaponColliders[2], new Vector3(0, 0, -2f), new Vector3(0.5f, 1.5f, 2.5f));
            weapons[2].AddNewComponent<WeaponInteraction>();
            weapons.Add(new GameObject("MachineGun3", new Vector3(40, 18, -40), Quaternion.Identity, Vector3.One, scene, room2));
            weaponColliders.Add(weapons[3].AddNewComponent<Collider>());
            weaponColliders[3].bound = new Engine.Bounding_Volumes.BoundingBox(weaponColliders[2], new Vector3(0, 0, -2f), new Vector3(0.5f, 1.5f, 2.5f));
            weapons[3].AddNewComponent<WeaponInteraction>();

            weapons[0].AddComponent(new Weapon(WeaponTypes.Pistol, 12, 28, 12, 240, 1000, new Vector3(2.5f, -1.5f, -5.75f)));
            weapons[1].AddComponent(new Weapon(WeaponTypes.MachineGun, 32, 72, 32, 640, 1000, new Vector3(1.5f, -2f, -5.5f)));
            weapons[2].AddComponent(new Weapon(WeaponTypes.MachineGun, 32, 72, 32, 640, 1000, new Vector3(1.5f, -2f, -5.5f)));
            weapons[3].AddComponent(new Weapon(WeaponTypes.MachineGun, 32, 72, 32, 640, 1000, new Vector3(1.5f, -2f, -5.5f)));
            gunfires.Add(new GameObject("Pistol_Gunfire", new Vector3(0, 0.6f, -4), Quaternion.Identity, Vector3.One * 0.5f, scene, weapons[0]));
            gunfires.Add(new GameObject("MachineGun_Gunfire", new Vector3(0, 0.15f, -8.5f), Quaternion.Identity, Vector3.One, scene, weapons[1]));
            gunfires.Add(new GameObject("MachineGun_Gunfire", new Vector3(0, 0.15f, -8.5f), Quaternion.Identity, Vector3.One, scene, weapons[2]));
            gunfires.Add(new GameObject("MachineGun_Gunfire", new Vector3(0, 0.15f, -8.5f), Quaternion.Identity, Vector3.One, scene, weapons[3]));
            enemy = new GameObject("Enemy", new Vector3(67, 20, -20), Quaternion.Identity, Vector3.One, scene, room2);
            enemy2 = new GameObject("Enemy2", new Vector3(-165, -45, 242), Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(-85)), Vector3.One, scene, room5);
            enemy3 = new GameObject("Enemy3", new Vector3(-38, 20, -96), Quaternion.Identity, Vector3.One, scene, room2);
            enemy.AddComponent(new EnemyController(weapons[1]));
            enemy2.AddComponent(new EnemyController(weapons[2]));
            enemy3.AddComponent(new EnemyController(weapons[3]));
            enemy.AddComponent(new Rigidbody(80));
            enemy2.AddComponent(new Rigidbody(80));
            enemy3.AddComponent(new Rigidbody(80));
            enemy.GetComponent<Rigidbody>().GravityEnabled = false;
            enemy2.GetComponent<Rigidbody>().GravityEnabled = false;
            enemy3.GetComponent<Rigidbody>().GravityEnabled = false;
            Collider enemyCol = enemy.AddNewComponent<Collider>();
            Collider enemyCol2 = enemy2.AddNewComponent<Collider>();
            Collider enemyCol3 = enemy3.AddNewComponent<Collider>();
            enemyCol.bound = new Engine.Bounding_Volumes.BoundingBox(enemyCol, new Vector3(0, -8f, 0), new Vector3(2, 9f, 2));
            enemyCol2.bound = new Engine.Bounding_Volumes.BoundingBox(enemyCol2, new Vector3(0, -8f, 0), new Vector3(2, 9f, 2));
            enemyCol3.bound = new Engine.Bounding_Volumes.BoundingBox(enemyCol3, new Vector3(0, -8f, 0), new Vector3(2, 9f, 2));
            //emitterTimer = 0;
            objectiveTimer = 2;
            player.GetComponent<PlayerController>().addWeapon(weapons[0]);
            particles = new List<SpriteInstance>();
            particleFireEmitter = new GameObject("Fire_Emitter", new Vector3(45, 12, -50), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)), Vector3.One * 2, scene, room2);
            particleExplosionEmitter = new GameObject("Explosion_Emitter", new Vector3(25, 18, -60), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)), Vector3.One, scene, room2);
            particleSmokeEmitter = new GameObject("Smoke_Emitter", new Vector3(60, 6, -60), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)), Vector3.One, scene, room2);
            particleBloodEmitter = new GameObject("Blood_Emitter", new Vector3(20, 18, -40), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)), Vector3.One, scene, room2);
            Physics.Initialize();

            Vector3 deskOffset = new Vector3(19, 0, 23);
            Vector3 couchOffset = new Vector3(0, 0, 0);
            propsRoom5.Add(new GameObject("Chair5_1", new Vector3(-253, -63.5f, 140), Quaternion.CreateFromYawPitchRoll(-(float)Math.PI / 2.0f, 0, 0), Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Desk5_1", new Vector3(-260, -64, 140) + Vector3.Transform(deskOffset, Matrix.CreateFromAxisAngle(Vector3.Up, -(float)Math.PI / 2)), Quaternion.CreateFromYawPitchRoll(-(float)Math.PI / 2.0f, 0, 0), Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Chair5_2", new Vector3(-148, -63.5f, 253), Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Desk5_2", new Vector3(-148, -64, 260) + deskOffset, Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Chair5_3", new Vector3(-33, -63.5f, 253), Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Desk5_3", new Vector3(-33, -64, 260) + deskOffset, Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Couch5_1", new Vector3(-142.5f, -64, 135) + couchOffset, Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Couch5_2", new Vector3(-130, -64, 145) + Vector3.Transform(couchOffset, Matrix.CreateFromAxisAngle(Vector3.Up, -(float)Math.PI / 2)), Quaternion.CreateFromYawPitchRoll(-(float)Math.PI / 2.0f, 0, 0), Vector3.One, scene, room5));

            bench = new GameObject("Bench", new Vector3(138, 100.5f, -220), Quaternion.Identity, Vector3.One, scene, room);
            bench1 = new GameObject("Bench1", new Vector3(90, 100.5f, -118), Quaternion.Identity, Vector3.One, scene, room);
            bench2 = new GameObject("Bench2", new Vector3(90, 100.5f, -275), Quaternion.Identity, Vector3.One, scene, room);
            bench3 = new GameObject("Bench3", new Vector3(0, 100.5f, -220), Quaternion.Identity, Vector3.One, scene, room);
            bench4 = new GameObject("Bench4", new Vector3(10, 100.5f, -118), Quaternion.Identity, Vector3.One, scene, room);
            column2 = new GameObject("column2", new Vector3(10, 100.5f, -275), Quaternion.Identity, Vector3.One, scene, room);
            column2.AddComponent(new KeypadInteraction("1337", null));
            bench7 = new GameObject("Bench7", new Vector3(138, 100.5f, -148), Quaternion.Identity, Vector3.One, scene, room);
            bench8 = new GameObject("Bench8", new Vector3(-80, 100.5f, -118), Quaternion.Identity, Vector3.One, scene, room);
            bench9 = new GameObject("Bench9", new Vector3(-80, 100.5f, -275), Quaternion.Identity, Vector3.One, scene, room);

            biurko1 = new GameObject("Biurko1", new Vector3(70,3,-5), Quaternion.Identity, Vector3.One, scene, room2);
            krzesełko = new GameObject("Krzeselko", new Vector3(50,3,-15), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(180)), Vector3.One, scene, room2);
            mirror = new GameObject("Lustro", new Vector3(70, 0, -414), Quaternion.Identity, Vector3.One, scene, room2);
            Collider columnCol1 = bench.AddNewComponent<Collider>();
            Collider columnCol2 = bench1.AddNewComponent<Collider>();
            Collider columnCol3 = bench2.AddNewComponent<Collider>();
            Collider columnCol4 = bench3.AddNewComponent<Collider>();
            Collider columnCol5 = bench4.AddNewComponent<Collider>();
            Collider columnCol6 = bench7.AddNewComponent<Collider>();
            Collider columnCol7 = bench8.AddNewComponent<Collider>();
            Collider columnCol8 = bench9.AddNewComponent<Collider>();
            Collider columnCol9 = column2.AddNewComponent<Collider>();

            columnCol1.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol1, new Vector3(0, 0f, 0), new Vector3(7.5f, 95f, 7.5f));
            columnCol2.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol2, new Vector3(0, 0f, 0), new Vector3(7.5f, 95f, 7.5f));
            columnCol3.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol3, new Vector3(0, 0f, 0), new Vector3(7.5f, 95f, 7.5f));
            columnCol4.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol4, new Vector3(0, 0f, 0), new Vector3(7.5f, 95f, 7.5f));
            columnCol5.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol5, new Vector3(0, 0f, 0), new Vector3(7.5f, 95f, 7.5f));
            columnCol6.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol6, new Vector3(0, 0f, 0), new Vector3(7.5f, 95f, 7.5f));
            columnCol7.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol7, new Vector3(0, 0f, 0), new Vector3(7.5f, 95f, 7.5f));
            columnCol8.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol8, new Vector3(0, 0f, 0), new Vector3(7.5f, 95f, 7.5f));
            columnCol9.bound = new Engine.Bounding_Volumes.BoundingBox(columnCol9, new Vector3(0, 0f, 0), new Vector3(7.5f, 95f, 7.5f));

            int minimapOffset = (int)(graphics.PreferredBackBufferWidth * 0.0075f);
            int minimapSize = (int)(graphics.PreferredBackBufferWidth * 0.15f);
            Minimap.Initialize(new Point(minimapSize), new Point(minimapOffset), new List<Vector2>()
            {
                new Vector2(-1000,1000)
            }, player);
            Minimap.Objectives.Add(new Vector3(-660, -140, 50));
            Minimap.Enemies.Add(enemy);
            Minimap.Enemies.Add(enemy2);
            stepsSounds = new List<SoundEffectInstance>();
            hologramRecordingMaxTime = 5.0f;
        }

        public void LoadGame()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            hologramRecordingShader = Content.Load<Effect>("FX/HologramRecording");
            hologramRecordingShader.Parameters["RecordingTime"].SetValue(0.0f);
            hologramRecordingShader.Parameters["RecordingTimeLimit"].SetValue(hologramRecordingMaxTime);
            hitShader = Content.Load<Effect>("FX/Saturate");
            healthShader = Content.Load<Effect>("FX/Health");
            healthShader.Parameters["Health"].SetValue(100.0f);
            healthShaderTexture = Content.Load<Texture2D>("Textures/Blood_Screen");
            bloomShader = Content.Load<Effect>("FX/Bloom");
            screenLightingShader = Content.Load<Effect>("FX/ScreenLighting");
            pauseMenuShader = Content.Load<Effect>("FX/PauseMenu");
            gameOverShader = Content.Load<Effect>("FX/GameOver");
            Effect highlightShader = Content.Load<Effect>("FX/HighlightSkinnedColor");
            gameMenu.LoadContent(Content);
            Minimap.LoadContent(Content);
            Model columnModel = Content.Load<Model>("Models/kolumna");
            Model jeden = Content.Load<Model>("Models/jeden");
            Model m_48 = Content.Load<Model>("Models/48");
            Model mirrorModel = Content.Load<Model>("Models/mirror");
            floorTexture = Content.Load<Texture2D>("Textures/Ground");
            gunfireTexture = Content.Load<Texture2D>("Textures/Gunfire");
            crosshair = Content.Load<Texture2D>("Textures/Crosshair");
            dialogueFont = Content.Load<SpriteFont>("Font/Dialogue");
            interfaceFont = Content.Load<SpriteFont>("Font/Interface");
            shot = Content.Load<SoundEffect>("Sounds/Pistol");
            dialog1 = Content.Load<SoundEffect>("Sounds/Dialogi/Dialog_korytarz");
            dialog2 = Content.Load<SoundEffect>("Sounds/Dialogi/Dialog_hol");
            dialog3a = Content.Load<SoundEffect>("Sounds/Dialogi/Dialog_hologram");
            dialog3b = Content.Load<SoundEffect>("Sounds/Dialogi/Dialog_hologram_spalony");
            dialog3c = Content.Load<SoundEffect>("Sounds/Dialogi/Dialog_no_holo");
            dialog4a = Content.Load<SoundEffect>("Sounds/Dialogi/Korytarz_dobra");
            dialog4b = Content.Load<SoundEffect>("Sounds/Dialogi/Korytarz_zła");
            stepsSounds.Add(Content.Load<SoundEffect>("Sounds/Steps_Walk").CreateInstance());
            stepsSounds.Add(Content.Load<SoundEffect>("Sounds/Steps_Run").CreateInstance());
            injurySound = Content.Load<SoundEffect>("Sounds/Injury").CreateInstance();
            player.GetComponent<PlayerController>().StepsSounds = stepsSounds;
            player.GetComponent<PlayerController>().InjurySound = injurySound;
            weapons[0].GetComponent<Weapon>().GunshotSound = shot;
            weapons[1].GetComponent<Weapon>().GunshotSound = shot;

            Model levelModel = Content.Load<Model>("Models/level3");
            level.AddComponent(new MeshInstance(levelModel));

            Model playerModel = Content.Load<Model>("Models/new/HD/BONE_3");
            Model playerPreviewModel = Content.Load<Model>("Models/new/HD/BONE_3_PREVIEW");
            Model playerHologramModel = Content.Load<Model>("Models/new/HD/BONE_3_HOLOGRAM");
            Model playerRunAnim = Content.Load<Model>("Models/new/HD/BONE_RUN_2");
            Model playerWalkAnim = Content.Load<Model>("Models/new/HD/BONE_WALK");
            Model playerDeathAnim = Content.Load<Model>("Models/new/HD/BONE_DEATH");
            Model playerJumpAnim = Content.Load<Model>("Models/new/HD/BONE_JUMP");
            Model playerCrouchAnim = Content.Load<Model>("Models/new/HD/BONE_CROUCH");
            Model playerRecordingStandingAnim = Content.Load<Model>("Models/new/HD/BONE_RECORDING_STANDING");
            Model playerRecordingCrouchingAnim = Content.Load<Model>("Models/new/HD/BONE_RECORDING_CROUCH");

            foreach (ModelMesh mesh in playerHologramModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Effect is BasicEffect)
                    {
                        (part.Effect as BasicEffect).Alpha = 0.5f;
                    }
                    else if (part.Effect is SkinnedEffect)
                    {
                        SkinnedEffect seffect = part.Effect as SkinnedEffect;
                        seffect.Alpha = 0.5f;
                    }
                }
            }

            foreach (ModelMesh mesh in playerPreviewModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if (part.Effect is BasicEffect)
                    {
                        (part.Effect as BasicEffect).Alpha = 0.1f;
                    }
                    else if (part.Effect is SkinnedEffect)
                    {
                        SkinnedEffect seffect = part.Effect as SkinnedEffect;
                        seffect.Alpha = 0.1f;
                    }
                }
            }

            player.GetComponent<PlayerController>().PlayerMesh = new MeshInstance(playerModel);
            player.GetComponent<PlayerController>().HologramMesh = new MeshInstance(playerHologramModel);
            player.GetComponent<PlayerController>().HologramMesh.Model.PreCustomSkinnedShaders.Add(highlightShader);
            player.GetComponent<PlayerController>().PreviewMesh = new MeshInstance(playerPreviewModel);
            player.GetComponent<PlayerController>().PreviewMesh.Model.PreCustomSkinnedShaders.Add(highlightShader);
            AnimationClip runClip = (playerRunAnim.Tag as ModelExtra).Clips[0];
            AnimationClip walkClip = (playerWalkAnim.Tag as ModelExtra).Clips[0];
            AnimationClip deathClip = (playerDeathAnim.Tag as ModelExtra).Clips[0];
            AnimationClip jumpClip = (playerJumpAnim.Tag as ModelExtra).Clips[0];
            AnimationClip crouchClip = (playerCrouchAnim.Tag as ModelExtra).Clips[0];
            AnimationClip recordingStandingClip = (playerRecordingStandingAnim.Tag as ModelExtra).Clips[0];
            AnimationClip recordingCrouchingClip = (playerRecordingCrouchingAnim.Tag as ModelExtra).Clips[0];
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(runClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(walkClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(deathClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(jumpClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(crouchClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(recordingStandingClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(recordingCrouchingClip);
            player.GetComponent<PlayerController>().PlayerMesh.Offset = new Vector3(0, -17, 0);
            player.GetComponent<PlayerController>().HologramMesh.Model.Clips.Add(runClip);
            player.GetComponent<PlayerController>().HologramMesh.Model.Clips.Add(walkClip);
            player.GetComponent<PlayerController>().HologramMesh.Model.Clips.Add(deathClip);
            player.GetComponent<PlayerController>().HologramMesh.Model.Clips.Add(jumpClip);
            player.GetComponent<PlayerController>().HologramMesh.Model.Clips.Add(crouchClip);
            player.GetComponent<PlayerController>().HologramMesh.Offset = new Vector3(0, -17, 0);
            player.GetComponent<PlayerController>().PreviewMesh.Model.Clips.Add(runClip);
            player.GetComponent<PlayerController>().PreviewMesh.Model.Clips.Add(walkClip);
            player.GetComponent<PlayerController>().PreviewMesh.Model.Clips.Add(deathClip);
            player.GetComponent<PlayerController>().PreviewMesh.Model.Clips.Add(jumpClip);
            player.GetComponent<PlayerController>().PreviewMesh.Model.Clips.Add(crouchClip);
            player.GetComponent<PlayerController>().PreviewMesh.Offset = new Vector3(0, -17, 0);

            Model enemyModel = Content.Load<Model>("Models/cop/cop_t_pose");
            Model enemyRunAnim = Content.Load<Model>("Models/cop/cop_run");
            Model enemyDeathAnim = Content.Load<Model>("Models/cop/cop_death");
            Model enemyShootAnim = Content.Load<Model>("Models/cop/cop_shoot");
            Model enemyHitAnim = Content.Load<Model>("Models/cop/cop_hit");
            AnimationClip enemyRunClip = (enemyRunAnim.Tag as ModelExtra).Clips[0];
            AnimationClip enemyDeathClip = (enemyDeathAnim.Tag as ModelExtra).Clips[0];
            AnimationClip enemyShootClip = (enemyShootAnim.Tag as ModelExtra).Clips[0];
            AnimationClip enemyHitClip = (enemyHitAnim.Tag as ModelExtra).Clips[0];

            enemy.AddComponent(new MeshInstance(enemyModel));
            enemy.GetComponent<MeshInstance>().Model.PreCustomSkinnedShaders.Add(highlightShader);
            enemy.GetComponent<MeshInstance>().Offset = new Vector3(0, -17, 0);
            enemy.GetComponent<MeshInstance>().Model.Clips.Add(enemyRunClip);
            enemy.GetComponent<MeshInstance>().Model.Clips.Add(enemyDeathClip);
            enemy.GetComponent<MeshInstance>().Model.Clips.Add(enemyShootClip);
            enemy.GetComponent<MeshInstance>().Model.Clips.Add(enemyHitClip);
            enemy.AddNewComponent<AnimationController>();
            enemy.GetComponent<AnimationController>().SetBindPose(enemyShootClip);
            enemy.GetComponent<AnimationController>().BindAnimation("run", 1, true);
            enemy.GetComponent<AnimationController>().BindAnimation("death", 2, false);
            enemy.GetComponent<AnimationController>().BindAnimation("hit", 3, false);
            enemy2.AddComponent(new MeshInstance(enemyModel));
            enemy2.GetComponent<MeshInstance>().Model.PreCustomSkinnedShaders.Add(highlightShader);
            enemy2.GetComponent<MeshInstance>().Offset = new Vector3(0, -17, 0);
            enemy2.GetComponent<MeshInstance>().Model.Clips.Add(enemyRunClip);
            enemy2.GetComponent<MeshInstance>().Model.Clips.Add(enemyDeathClip);
            enemy2.GetComponent<MeshInstance>().Model.Clips.Add(enemyShootClip);
            enemy2.GetComponent<MeshInstance>().Model.Clips.Add(enemyHitClip);
            enemy2.AddNewComponent<AnimationController>();
            enemy2.GetComponent<AnimationController>().SetBindPose(enemyShootClip);
            enemy2.GetComponent<AnimationController>().BindAnimation("run", 1, true);
            enemy2.GetComponent<AnimationController>().BindAnimation("death", 2, false);
            enemy2.GetComponent<AnimationController>().BindAnimation("hit", 3, false);

            enemy3.AddComponent(new MeshInstance(enemyModel));
            enemy3.GetComponent<MeshInstance>().Model.PreCustomSkinnedShaders.Add(highlightShader);
            enemy3.GetComponent<MeshInstance>().Offset = new Vector3(0, -17, 0);
            enemy3.GetComponent<MeshInstance>().Model.Clips.Add(enemyRunClip);
            enemy3.GetComponent<MeshInstance>().Model.Clips.Add(enemyDeathClip);
            enemy3.GetComponent<MeshInstance>().Model.Clips.Add(enemyShootClip);
            enemy3.GetComponent<MeshInstance>().Model.Clips.Add(enemyHitClip);
            enemy3.AddNewComponent<AnimationController>();
            enemy3.GetComponent<AnimationController>().SetBindPose(enemyShootClip);
            enemy3.GetComponent<AnimationController>().BindAnimation("run", 1, true);
            enemy3.GetComponent<AnimationController>().BindAnimation("death", 2, false);
            enemy3.GetComponent<AnimationController>().BindAnimation("hit", 3, false);

            Model doorModel = Content.Load<Model>("Models/door2");
            doors[0].AddComponent(new MeshInstance(doorModel));

            Model pistolModel = Content.Load<Model>("Models/Pistol");
            weapons[0].AddComponent(new MeshInstance(pistolModel));
            gunfires[0].AddComponent(new SpriteInstance(gunfireTexture, new Vector3(0, 5, 5), 1, 1, graphics));
            gunfires[0].GetComponent<SpriteInstance>().Enabled = false;
            Model machineGunModel = Content.Load<Model>("Models/Machine_Gun");
            weapons[1].AddComponent(new MeshInstance(machineGunModel));
            gunfires[1].AddComponent(new SpriteInstance(gunfireTexture, new Vector3(0, 5, 5), 1, 1, graphics));
            gunfires[1].GetComponent<SpriteInstance>().Enabled = false;
            weapons[2].AddComponent(new MeshInstance(machineGunModel));
            gunfires[2].AddComponent(new SpriteInstance(gunfireTexture, new Vector3(0, 5, 5), 1, 1, graphics));
            gunfires[2].GetComponent<SpriteInstance>().Enabled = false;

            weapons[3].AddComponent(new MeshInstance(machineGunModel));
            gunfires[3].AddComponent(new SpriteInstance(gunfireTexture, new Vector3(0, 5, 5), 1, 1, graphics));
            gunfires[3].GetComponent<SpriteInstance>().Enabled = false;
            for (int i = 1; i < 4; i++)
                particles.Add(new SpriteInstance(Content.Load<Texture2D>("Textures/Particle" + i + " [Fire]"), new Vector3(0, 1, 1), 1, 0.5f, graphics));
            for (int i = 1; i < 4; i++)
                particles.Add(new SpriteInstance(Content.Load<Texture2D>("Textures/Particle" + i + " [Explosion]"), new Vector3(0, 5, 5), 1, 1, graphics));
            for (int i = 1; i < 4; i++)
                particles.Add(new SpriteInstance(Content.Load<Texture2D>("Textures/Particle" + i + " [Smoke]"), new Vector3(0, 5, 5), 1, 1, graphics));
            for (int i = 1; i < 4; i++)
                particles.Add(new SpriteInstance(Content.Load<Texture2D>("Textures/Particle" + i + " [Blood]"), new Vector3(0, 2, 2), 1, 1, graphics));
            particleFireEmitter.AddComponent(new ParticleSystem(ParticleSystemType.Fire, 400, 2, particles.GetRange(0, 3), 1));
            particleExplosionEmitter.AddComponent(new ParticleSystem(ParticleSystemType.Explosion, 6, 2, particles.GetRange(3, 3), 0.01f));
            particleSmokeEmitter.AddComponent(new ParticleSystem(ParticleSystemType.Smoke, 100, 2, particles.GetRange(6, 3), 1));
            particleBloodEmitter.AddComponent(new ParticleSystem(ParticleSystemType.Jet, 12, 0.25f, particles.GetRange(9, 3), 0.5f));
            /*particleFireEmitter.GetComponent<ParticleSystem>().Init();
            particleExplosionEmitter.GetComponent<ParticleSystem>().Init();
            particleSmokeEmitter.GetComponent<ParticleSystem>().Init();
            particleBloodEmitter.GetComponent<ParticleSystem>().Init();*/

            Model chairModel = Content.Load<Model>("Models/krzeselko");
            Model deskModel = Content.Load<Model>("Models/biurko");
            Model couchModel = Content.Load<Model>("Models/kanapa2");
            propsRoom5[0].AddComponent(new MeshInstance(chairModel));
            propsRoom5[1].AddComponent(new MeshInstance(deskModel));
            propsRoom5[2].AddComponent(new MeshInstance(chairModel));
            propsRoom5[3].AddComponent(new MeshInstance(deskModel));
            propsRoom5[4].AddComponent(new MeshInstance(chairModel));
            propsRoom5[5].AddComponent(new MeshInstance(deskModel));
            propsRoom5[6].AddComponent(new MeshInstance(couchModel));
            propsRoom5[7].AddComponent(new MeshInstance(couchModel));

            bench.AddComponent(new MeshInstance(columnModel));
            bench1.AddComponent(new MeshInstance(columnModel));
            bench2.AddComponent(new MeshInstance(columnModel));
            bench3.AddComponent(new MeshInstance(columnModel));
            bench4.AddComponent(new MeshInstance(columnModel));
            column2.AddComponent(new MeshInstance(columnModel));
            bench7.AddComponent(new MeshInstance(columnModel));
            bench8.AddComponent(new MeshInstance(columnModel));
            bench9.AddComponent(new MeshInstance(columnModel));

            biurko1.AddComponent(new MeshInstance(deskModel));
            krzesełko.AddComponent(new MeshInstance(chairModel));
            mirror.AddComponent(new MeshInstance(mirrorModel));
        }

        protected Texture2D DrawSceneToTexture(RenderTarget2D currentRenderTarget, GameTime gameTime)
        {
            RenderTargetBinding[] oldRenderTarget = GraphicsDevice.GetRenderTargets();
            // Set the render target
            GraphicsDevice.SetRenderTarget(currentRenderTarget);

            // Draw the scene
            GraphicsDevice.Clear(Color.CornflowerBlue);

            scene.Draw(gameTime);

#if DRAW_DEBUG_WIREFRAME
            RasterizerState originalState = GraphicsDevice.RasterizerState;
            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizerState;
            BasicEffect effect = new BasicEffect(graphics.GraphicsDevice);
            effect.TextureEnabled = false;
            effect.World = Matrix.Identity;
            effect.View = scene.Camera.GetComponent<Camera>().ViewMatrix;
            effect.Projection = scene.Camera.GetComponent<Camera>().ProjectionMatrix;
            effect.CurrentTechnique.Passes[0].Apply();

            scene.DrawDebug(gameTime, graphics);

            //Ray
            VertexPosition[] line = new VertexPosition[2];
            line[0].Position = player.GlobalPosition - new Vector3(0, 1, 0);
            line[1].Position = line[0].Position + scene.Camera.LocalToWorldMatrix.Forward * 100.0f;
            graphics.GraphicsDevice.DrawUserPrimitives<VertexPosition>(PrimitiveType.LineList, line, 0, 1);

            GraphicsDevice.RasterizerState = originalState;
#endif

            // Drop the render target
            GraphicsDevice.SetRenderTargets(oldRenderTarget);
            // Return the texture in the render target
            return currentRenderTarget;
        }

        private void ChangeGamma(PressingActionArgs args)
        {
            if (args.action == GameAction.GAMMA_UP) gammaValue += (float)args.gameTime.ElapsedGameTime.TotalSeconds;
            if (args.action == GameAction.GAMMA_DOWN) gammaValue -= (float)args.gameTime.ElapsedGameTime.TotalSeconds;
            if (gammaValue < 0.0f) gammaValue = 0.0f;
            if (gammaValue > 5.0f) gammaValue = 5.0f;
        }

        private void ChangeBrightness(PressingActionArgs args)
        {
            if (args.action == GameAction.BRIGHTNESS_UP) brightnessValue +=  0.2f * (float)args.gameTime.ElapsedGameTime.TotalSeconds;
            if (args.action == GameAction.BRIGHTNESS_DOWN) brightnessValue -= 0.2f * (float)args.gameTime.ElapsedGameTime.TotalSeconds;
            if (brightnessValue < 0.0f) brightnessValue = 0.0f;
            if (brightnessValue > 1.0f) brightnessValue = 1.0f;
        }

        private void ChangeContrast(PressingActionArgs args)
        {
            if (args.action == GameAction.CONTRAST_UP) contrastValue +=  0.1f * (float)args.gameTime.ElapsedGameTime.TotalSeconds;
            if (args.action == GameAction.CONTRAST_DOWN) contrastValue -= 0.1f * (float)args.gameTime.ElapsedGameTime.TotalSeconds;
            if (contrastValue < -1) contrastValue = -1;
            if (contrastValue > 1) contrastValue = 1;
        }

        private void SaveGame(PressedActionArgs args)
        {
            if (gameState.Equals(GameState.Game))
            {
                device = null;
                gameState = GameState.Pause;
                StorageDevice.BeginShowSelector(PlayerIndex.One, GetDeviceForSaving, null);
            }
        }

        private void LoadGame(PressedActionArgs args)
        {
            if(gameState.Equals(GameState.Game))
            {
                device = null;
                gameState = GameState.Pause;
                StorageDevice.BeginShowSelector(PlayerIndex.One, GetDeviceForLoading, null);
            }
        }

        private void GetDeviceForSaving(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);
            if (device != null && device.IsConnected)
            {
                DoSaveGame();
            }
        }

        private void GetDeviceForLoading(IAsyncResult result)
        {
            device = StorageDevice.EndShowSelector(result);
            if (device != null && device.IsConnected)
            {
                DoLoadGame();
            }
        }

        private void DoSaveGame()
        {
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer("Storage", null, null);
            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            // Close the wait handle.
            result.AsyncWaitHandle.Close();

            string filename = "savegame.sav";
            // Check to see whether the save exists.
            if (container.FileExists(filename))
                // Delete it so that we can create one fresh.
                container.DeleteFile(filename);

            // Create the file.
            Stream stream = container.CreateFile(filename);

            DataContractSerializer serializer = new DataContractSerializer(scene.GetType(), knownTypes,
                Int32.MaxValue, false, true, null);
            using (XmlWriter writer = XmlWriter.Create(stream, new XmlWriterSettings { Indent = true, IndentChars = "\t" }))
                serializer.WriteObject(writer, scene);
            // Close the file.
            stream.Close();
            // Dispose the container, to commit changes.
            container.Dispose();

            gameState = GameState.Game;
        }

        private void DoLoadGame()
        {
            // Open a storage container.
            IAsyncResult result =
                device.BeginOpenContainer("Storage", null, null);
            // Wait for the WaitHandle to become signaled.
            result.AsyncWaitHandle.WaitOne();
            StorageContainer container = device.EndOpenContainer(result);
            // Close the wait handle.
            result.AsyncWaitHandle.Close();

            string filename = "savegame.sav";
            // Check to see whether the save exists.
            if (!container.FileExists(filename))
            {
                // If not, dispose of the container and return.
                container.Dispose();
                return;
            }

            // Open the file.
            Stream stream = container.OpenFile(filename, FileMode.Open);

            DataContractSerializer serializer = new DataContractSerializer(scene.GetType(), knownTypes,
                0xFFFF, false, true, null);
            Scene newScene = null;
            using (XmlReader reader = XmlReader.Create(stream, new XmlReaderSettings { IgnoreWhitespace = true }))
                newScene = (Scene)serializer.ReadObject(reader);

            if (newScene != null) InitializeNewScene(newScene);

            gameState = GameState.Game;
        }

        private void InitializeNewScene(Scene newScene)
        {
            List<GameObject> newObjects = newScene.GetAllObjects();
            List<GameObject> objects = scene.GetAllObjects();

            foreach(GameObject newGo in newObjects)
            {
                GameObject go = objects.Find(x => x.Name == newGo.Name);
                List<Component> components = newGo.GetInactiveComponents<Component>();
                foreach(Component comp in components)
                {
                    if(comp is MeshInstance) (comp as MeshInstance).Model = go.GetInactiveComponent<MeshInstance>().Model;
                    if(comp is PlayerController)
                    {
                        (comp as PlayerController).PlayerMesh = go.GetInactiveComponent<PlayerController>().PlayerMesh;
                        (comp as PlayerController).PreviewMesh = go.GetInactiveComponent<PlayerController>().PreviewMesh;
                        (comp as PlayerController).HologramMesh = go.GetInactiveComponent<PlayerController>().HologramMesh;
                    }
                    if(comp is Weapon) (comp as Weapon).GunshotSound = go.GetInactiveComponent<Weapon>().GunshotSound;
                    if(comp is SpriteInstance)
                    {
                        (comp as SpriteInstance).Graphics = graphics;
                        (comp as SpriteInstance).Texture = go.GetInactiveComponent<SpriteInstance>().Texture;
                    }
                }

                if (newGo.Name == "Player") player = newGo;
                if (newGo.Name == "Enemy") enemy = newGo;
                if (newGo.Name == "Enemy2") enemy2 = newGo;
                if (newGo.Name == "Enemy3") enemy3 = newGo;
            }

            

            scene = newScene;
        }
    }
}
