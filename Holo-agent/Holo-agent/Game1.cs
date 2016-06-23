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
        Effect postProcessingEffect;
        Effect color_time;
        Effect cameraShader;
        Effect healthShader;
        Effect bloomShader;
        Texture2D healthShaderTexture;
        Texture2D crosshair;
        SpriteFont font;
        FrameCounter frameCounter;
        Scene scene;
        GameObject level;
        GameObject player;
        GameObject enemy;
        GameObject enemy2;
        GameObject gunfire;
        GameObject bench;
        GameObject bench1;
        GameObject bench2;
        GameObject bench3;
        GameObject bench4;
        GameObject column2;
        GameObject bench7;
        List<GameObject> propsRoom5;
        List<SpriteInstance> particles;
        GameObject particleFireEmitter, particleExplosionEmitter, particleSmokeEmitter, particleBloodEmitter;
        List<GameObject> weapons;
        List<GameObject> gunfires;
        List<Collider> weaponColliders;
        Weapon weapon;
        float /*emitterTimer,*/ objectiveTimer;
        float special_timer = 0.0f;
        SoundEffect shot;
        List<SoundEffectInstance> stepsSounds, ouchSounds;
        Texture2D gunfireTexture;
        Texture2D floorTexture;
        GameState gameState = GameState.Menu;
        GameMenu gameMenu;
        Vector2 objectivePosition = Vector2.UnitY * -500;
        string objectiveString = "[Some objective]";
        private double? startTime;

        StorageDevice device;
        Type[] knownTypes;

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
            /*graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            graphics.IsFullScreen = true;
            graphics.ApplyChanges();*/
            Input.Initialize();
            Input.BindActionPress(GameAction.SAVE, SaveGame);
            Input.BindActionPress(GameAction.LOAD, LoadGame);
            // TODO: Add your initialization logic here
            frameCounter = new FrameCounter();
            weapons = new List<GameObject>();
            gunfires = new List<GameObject>();
            weaponColliders = new List<Collider>();
            propsRoom5 = new List<GameObject>();
            renderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                GraphicsDevice.PresentationParameters.BackBufferFormat,
                DepthFormat.Depth24);
            scene = new Scene();
            GameObject roomTemp = new GameObject("RoomTemp", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-10000, -10000, -10000), new Vector3(10000, 10000, 10000)));
            GameObject room = new GameObject("Room1", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(175, -5, -230), new Vector3(400, 100, 100)));
            GameObject room2 = new GameObject("Room2", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-140, -5, -365), new Vector3(190, 100, -75)));
            GameObject room3 = new GameObject("Room3", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-185, -5, -65), new Vector3(10, 40, 40)));
            GameObject room4 = new GameObject("Room4", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-265, -65, -10), new Vector3(-180, 40, 40)));
            GameObject room5 = new GameObject("Room5", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-300, -65, 35), new Vector3(-180, -25, 230)));
            GameObject room6 = new GameObject("Room6", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-178, -65, 100), new Vector3(5, -25, 300)));
            GameObject room7 = new GameObject("Room7", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-3, -170, 170), new Vector3(40, -25, 215)));
            GameObject room8 = new GameObject("Room8", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-195, -170, 165), new Vector3(5, -130, 220)));
            GameObject room9 = new GameObject("Room9", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-175, -250, 218), new Vector3(-110, -130, 265)));
            GameObject room10 = new GameObject("Room10", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-255, -250, 260), new Vector3(-10, -170, 420)));
            GameObject room11 = new GameObject("Room11", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-20, -250, 255), new Vector3(160, -210, 510)));
            GameObject room12 = new GameObject("Room12", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-60, -250, 440), new Vector3(120, -170, 570)));
            GameObject room13 = new GameObject("Room13", Vector3.Zero, Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(new Vector3(-160, -250, 400), new Vector3(-55, -170, 620)));
            level = new GameObject("Level", new Vector3(-100, -226, 550), Quaternion.Identity, Vector3.One, scene, null, new BoundingBox(-1000*Vector3.One, 1000*Vector3.One));

            scene.AddRoomConnection(room, room2, new BoundingBox());
            scene.AddRoomConnection(room2, room3, new BoundingBox());
            scene.AddRoomConnection(room3, room4, new BoundingBox());
            scene.AddRoomConnection(room4, room5, new BoundingBox());
            scene.AddRoomConnection(room5, room6, new BoundingBox());
            scene.AddRoomConnection(room6, room7, new BoundingBox());
            scene.AddRoomConnection(room7, room8, new BoundingBox());
            scene.AddRoomConnection(room8, room9, new BoundingBox());
            scene.AddRoomConnection(room9, room10, new BoundingBox());
            scene.AddRoomConnection(room10, room11, new BoundingBox());
            scene.AddRoomConnection(room11, room12, new BoundingBox());
            scene.AddRoomConnection(room12, room13, new BoundingBox());
            scene.AddRoomConnection(room13, room10, new BoundingBox());

            (new GameObject("Floor1", new Vector3(10, -5f, -160), Quaternion.Identity, Vector3.One, scene, room2, new BoundingBox(new Vector3(-180, -5f, -155), new Vector3(180, 6f, 155)))).AddNewComponent<Collider>();

            player = new GameObject("Player", new Vector3(330, 20, 15), Quaternion.Identity, Vector3.One, scene, room2);
            player.AddNewComponent<PlayerController>();
            player.AddComponent(new Rigidbody(80, 1.5f));
            player.GetComponent<Rigidbody>().GravityEnabled = false;
            Collider playerCol = player.AddNewComponent<Collider>();
            playerCol.bound = new Engine.Bounding_Volumes.BoundingBox(playerCol, new Vector3(0, -8f, 0), new Vector3(2, 9f, 2));
            GameObject camera = new GameObject("Camera", new Vector3(0, 0, 0), Quaternion.Identity, Vector3.One, scene, player, null, false);
            Camera cameraComp = new Camera(45, graphics.GraphicsDevice.Viewport.AspectRatio, 1, 1000);
            camera.AddComponent(cameraComp);
            scene.Camera = camera;
            weapons.Add(new GameObject("Pistol", new Vector3(20, 18, -40), Quaternion.Identity, 0.5f*Vector3.One, scene, room2));
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
            weapons[0].AddComponent(new Weapon(WeaponTypes.Pistol, 12, 28, 12, 240, 1000, new Vector3(2.5f, -1.5f, -5.75f)));
            weapons[1].AddComponent(new Weapon(WeaponTypes.MachineGun, 32, 72, 32, 640, 1000, new Vector3(2f, -1.5f, -5.5f)));
            weapons[2].AddComponent(new Weapon(WeaponTypes.MachineGun, 32, 72, 32, 640, 1000, new Vector3(2f, -1.5f, -5.5f)));
            gunfires.Add(new GameObject("Pistol_Gunfire", new Vector3(0, 0.6f, -4), Quaternion.Identity, Vector3.One * 0.5f, scene, weapons[0]));
            gunfires.Add(new GameObject("MachineGun_Gunfire", new Vector3(0, 0.15f, -8.5f), Quaternion.Identity, Vector3.One, scene, weapons[1]));
            gunfires.Add(new GameObject("MachineGun_Gunfire", new Vector3(0, 0.15f, -8.5f), Quaternion.Identity, Vector3.One, scene, weapons[2]));
            enemy = new GameObject("Enemy", new Vector3(30, 20, -150), Quaternion.Identity, Vector3.One, scene, room2);
            enemy2 = new GameObject("Enemy2", new Vector3(-165, -40, 242), Quaternion.CreateFromAxisAngle(Vector3.Up, MathHelper.ToRadians(-135)), Vector3.One, scene, room5);
            enemy.AddComponent(new EnemyController(weapons[1], new List<Vector3>()
            {
                new Vector3(-120, 20, -250), new Vector3(-120, 20, -100),
                new Vector3(170, 20, -100), new Vector3(170, 20, -300)
            }));
            enemy2.AddComponent(new EnemyController(weapons[2]));
            enemy.AddComponent(new Rigidbody(80));
            enemy2.AddComponent(new Rigidbody(80));
            enemy.GetComponent<Rigidbody>().GravityEnabled = false;
            enemy2.GetComponent<Rigidbody>().GravityEnabled = false;
            Collider enemyCol = enemy.AddNewComponent<Collider>();
            Collider enemyCol2 = enemy2.AddNewComponent<Collider>();
            enemyCol.bound = new Engine.Bounding_Volumes.BoundingBox(enemyCol, new Vector3(0, -8f, 0), new Vector3(2, 9f, 2));
            enemyCol2.bound = new Engine.Bounding_Volumes.BoundingBox(enemyCol2, new Vector3(0, -8f, 0), new Vector3(2, 9f, 2));
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
            Vector3 couchOffset = new Vector3(10, 0, 20);
            propsRoom5.Add(new GameObject("Chair5_1", new Vector3(-253, -57, 140), Quaternion.CreateFromYawPitchRoll(-(float)Math.PI/2.0f,0,0), Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Desk5_1", new Vector3(-260, -57, 140) + Vector3.Transform(deskOffset, Matrix.CreateFromAxisAngle(Vector3.Up, -(float)Math.PI / 2)), Quaternion.CreateFromYawPitchRoll(-(float)Math.PI / 2.0f, 0, 0), Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Chair5_2", new Vector3(-148, -57, 253), Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Desk5_2", new Vector3(-148, -57, 260) + deskOffset, Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Chair5_3", new Vector3(-33, -57, 253), Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Desk5_3", new Vector3(-33, -57, 260) + deskOffset, Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Couch5_1", new Vector3(-152, -60, 116) + couchOffset, Quaternion.Identity, Vector3.One, scene, room5));
            propsRoom5.Add(new GameObject("Couch5_2", new Vector3(-130, -60, 141) + Vector3.Transform(couchOffset, Matrix.CreateFromAxisAngle(Vector3.Up, -(float)Math.PI / 2)), Quaternion.CreateFromYawPitchRoll(-(float)Math.PI / 2.0f, 0, 0), Vector3.One, scene, room5));

            bench = new GameObject("Bench", new Vector3(115, 0, -170), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(180)), Vector3.One, scene, room, new BoundingBox(new Vector3(150, 0, -200), new Vector3(190, 35, -220)));
            bench1 = new GameObject("Bench1", new Vector3(105, 0, -255), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(270)), Vector3.One, scene, room, new BoundingBox(new Vector3(150, 0, -200), new Vector3(190, 35, -220)));
            bench2 = new GameObject("Bench2", new Vector3(10, 0, -255), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(270)), Vector3.One, scene, room, new BoundingBox(new Vector3(150, 0, -200), new Vector3(190, 35, -220)));
            bench3 = new GameObject("Bench3", new Vector3(105, 0, -156), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)), Vector3.One, scene, room, new BoundingBox(new Vector3(150, 0, -200), new Vector3(190, 35, -220)));
            bench4 = new GameObject("Bench4", new Vector3(-20, 0, -156), Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathHelper.ToRadians(90)), Vector3.One, scene, room, new BoundingBox(new Vector3(150, 0, -200), new Vector3(190, 35, -220)));
            column2 = new GameObject("column2", new Vector3(25, 88, -220), Quaternion.Identity, Vector3.One, scene, room);
            bench7 = new GameObject("Bench7", new Vector3(-64, 0, -210), Quaternion.Identity, Vector3.One, scene, room, new BoundingBox(new Vector3(150, 0, -200), new Vector3(190, 35, -220)));


            int minimapOffset = (int)(graphics.PreferredBackBufferWidth * 0.0075f);
            int minimapSize = (int)(graphics.PreferredBackBufferWidth * 0.15f);
            Minimap.Initialize(new Point(minimapSize), new Point(minimapOffset), new List<Vector2>()
            {
                new Vector2(-1000,1000)
            }, player);
            Minimap.Objectives.Add(new Vector3(50, 100001, 90));
            Minimap.Objectives.Add(new Vector3(70, 0, -100));
            Minimap.Enemies.Add(enemy);
            Minimap.Enemies.Add(enemy2);
            stepsSounds = new List<SoundEffectInstance>();
            ouchSounds = new List<SoundEffectInstance>();
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
            postProcessingEffect = Content.Load<Effect>("FX/PostProcess");
            color_time = Content.Load<Effect>("FX/Changing_color");
            color_time.Parameters["Timer"].SetValue(0.0f);
            color_time.Parameters["Color"].SetValue(Color.White.ToVector4());
            cameraShader = Content.Load<Effect>("FX/Shader1");
            healthShader = Content.Load<Effect>("FX/Health");
            healthShader.Parameters["Health"].SetValue(100.0f);
            healthShaderTexture = Content.Load<Texture2D>("Textures/Blood_Screen");
            bloomShader = Content.Load<Effect>("FX/Bloom");
            gameMenu.LoadContent(Content);
            Minimap.LoadContent(Content);
            Model columnModel = Content.Load<Model>("Models/kolumna");
            floorTexture = Content.Load<Texture2D>("Textures/Ground");
            gunfireTexture = Content.Load<Texture2D>("Textures/Gunfire");
            crosshair = Content.Load<Texture2D>("Textures/Crosshair");
            font = Content.Load<SpriteFont>("Textures/Arial");
            shot = Content.Load<SoundEffect>("Sounds/Pistol");
            stepsSounds.Add(Content.Load<SoundEffect>("Sounds/Steps_Walk").CreateInstance());
            stepsSounds.Add(Content.Load<SoundEffect>("Sounds/Steps_Run").CreateInstance());
            ouchSounds.Add(Content.Load<SoundEffect>("Sounds/Ouch_1").CreateInstance());
            ouchSounds.Add(Content.Load<SoundEffect>("Sounds/Ouch_2").CreateInstance());
            player.GetComponent<PlayerController>().StepsSounds = stepsSounds;
            player.GetComponent<PlayerController>().OuchSounds = ouchSounds;
            weapons[0].GetComponent<Weapon>().GunshotSound = shot;
            weapons[1].GetComponent<Weapon>().GunshotSound = shot;

            Model levelModel = Content.Load<Model>("Models/Level");
            level.AddComponent(new MeshInstance(levelModel));

            Model playerModel = Content.Load<Model>("Models/new/HD/BONE_2");
            Model playerPreviewModel = Content.Load<Model>("Models/new/HD/BONE_2_PREVIEW");
            Model playerHologramModel = Content.Load<Model>("Models/new/HD/BONE_2_HOLOGRAM");
            Model playerRunAnim = Content.Load<Model>("Models/new/HD/BONE_RUN_2");
            Model playerWalkAnim = Content.Load<Model>("Models/new/HD/BONE_WALK");
            Model playerDeathAnim = Content.Load<Model>("Models/new/HD/BONE_DEATH");
            Model playerJumpAnim = Content.Load<Model>("Models/new/HD/BONE_JUMP");
            Model playerCrouchAnim = Content.Load<Model>("Models/new/HD/BONE_CROUCH");
            
            
            foreach (ModelMesh mesh in playerHologramModel.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    if(part.Effect is BasicEffect)
                    {
                        (part.Effect as BasicEffect).Alpha = 0.5f;
                    }
                    else if(part.Effect is SkinnedEffect)
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
            player.GetComponent<PlayerController>().PreviewMesh = new MeshInstance(playerPreviewModel);
            AnimationClip runClip = (playerRunAnim.Tag as ModelExtra).Clips[0];
            AnimationClip walkClip = (playerWalkAnim.Tag as ModelExtra).Clips[0];
            AnimationClip deathClip = (playerDeathAnim.Tag as ModelExtra).Clips[0];
            AnimationClip jumpClip = (playerJumpAnim.Tag as ModelExtra).Clips[0];
            AnimationClip crouchClip = (playerCrouchAnim.Tag as ModelExtra).Clips[0];
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(runClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(walkClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(deathClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(jumpClip);
            player.GetComponent<PlayerController>().PlayerMesh.Model.Clips.Add(crouchClip);
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

            Model doorModel = Content.Load<Model>("Models/door_001");
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
            Model couchModel = Content.Load<Model>("Models/kanapa");
            propsRoom5[0].AddComponent(new MeshInstance(chairModel));
            propsRoom5[1].AddComponent(new MeshInstance(deskModel));
            propsRoom5[2].AddComponent(new MeshInstance(chairModel));
            propsRoom5[3].AddComponent(new MeshInstance(deskModel));
            propsRoom5[4].AddComponent(new MeshInstance(chairModel));
            propsRoom5[5].AddComponent(new MeshInstance(deskModel));
            propsRoom5[6].AddComponent(new MeshInstance(couchModel));
            propsRoom5[7].AddComponent(new MeshInstance(couchModel));

            bench.AddComponent(new MeshInstance(couchModel));
            bench1.AddComponent(new MeshInstance(couchModel));
            bench2.AddComponent(new MeshInstance(couchModel));
            bench3.AddComponent(new MeshInstance(couchModel));
            bench4.AddComponent(new MeshInstance(couchModel));
            column2.AddComponent(new MeshInstance(columnModel));
            bench7.AddComponent(new MeshInstance(couchModel));
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
            gameMenu.Update(ref gameState, this);
            if (gameState == GameState.Menu)
            {
                if (!IsMouseVisible)
                    IsMouseVisible = true;
            }
            if (gameState == GameState.GameRunning)
            {
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    Exit();
                    return;
                }
                if (IsMouseVisible)
                {
                    IsMouseVisible = false;
                    Mouse.SetPosition(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
                }
                frameCounter.Update(gameTime);
                Input.Update(gameTime, graphics);
                scene.Update(gameTime);

                if (objectiveTimer > 0)
                    objectiveTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                else
                    objectivePosition = Vector2.Lerp(objectivePosition, new Vector2(objectivePosition.X, -250), 0.05f);
                if (player.GetComponent<PlayerController>() != null)
                    weapon = player.GetComponent<PlayerController>().getWeapon();
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
            if (gameState == GameState.Menu)
            {
                GraphicsDevice.Clear(Color.CornflowerBlue);
                gameMenu.Draw(spriteBatch, graphics);
            }
            if (gameState == GameState.GameRunning)
            {
                if (startTime == null) startTime = gameTime.TotalGameTime.TotalSeconds;
                GraphicsDevice.Clear(Color.Black);
                Texture2D texture = DrawSceneToTexture(renderTarget, gameTime);

                if (player.GetComponent<PlayerController>() == null)
                {
                    if (special_timer < 5.0f)
                    {
                        special_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        color_time.Parameters["Timer"].SetValue(special_timer / 5.0f);
                    }
                }
                else if (special_timer > 0.0f)
                {
                    special_timer = 0.0f;
                    color_time.Parameters["Timer"].SetValue(0.0f);
                }
                
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, bloomShader);
                spriteBatch.Draw(texture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                spriteBatch.End();

                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, /*(special_timer > 0.0f ? */color_time/* : cameraShader)*/);
                spriteBatch.Draw(texture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                spriteBatch.End();

                if (player.GetComponent<PlayerController>() != null)
                {
                    healthShader.Parameters["Health"].SetValue(player.GetComponent<PlayerController>().Health);
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone, healthShader);
                    spriteBatch.Draw(healthShaderTexture, new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                    spriteBatch.End();
                }

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicWrap, DepthStencilState.Default, RasterizerState.CullNone);
                Point w = new Point(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                DrawTutorialTips(spriteBatch, font, w, Color.Orange, gameTime);
                if(enemy.GetComponent<EnemyController>() != null)
                {
                    Weapon enemyWeapon = enemy.GetComponent<EnemyController>().Weapon.GetComponent<Weapon>();
                    if(enemyWeapon != null)
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
                if (player.GetComponent<PlayerController>() != null)
                {
                    if (weapon != null)
                    {
                        string weaponInfo = weapon.getMagazine() + "/" + weapon.getAmmo();
                        Vector2 weaponInfoSize = 0.5f*font.MeasureString(weaponInfo);
                        spriteBatch.DrawString(font, weaponInfo, new Vector2(w.X - 1.05f*weaponInfoSize.X, w.Y - 1.05f*weaponInfoSize.Y), Color.Red, 0, Vector2.Zero, 0.5f, SpriteEffects.None, 0);
                        /*if (weapon.info != null)
                            spriteBatch.DrawString(font, weapon.info, new Vector2(50, 60), Color.SeaGreen);*/
                        GameObject gunfireInstance = weapon.getGunfireInstance();
                        if(gunfireInstance != null) gunfireInstance.GetInactiveComponent<SpriteInstance>().Draw(gameTime);
                    }
                    int selectedPath = player.GetComponent<PlayerController>().SelectedPath;
                    int selectedPlaying = player.GetComponent<PlayerController>().PlayingPath;
                    bool previewing = player.GetComponent<PlayerController>().HologramPreviewing;
                    bool playing = player.GetComponent<PlayerController>().HologramPlaying;
                    for(int i = 0; i < 3; ++i)
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
                        
                        if(selectedPath == i)
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
                        Vector2 descSize = 0.2f*font.MeasureString(desc);
                        spriteBatch.DrawString(font, desc, new Vector2(0.02f*w.X, w.Y - (0.2f*descSize.Y + 1.05f*(3-i)*descSize.Y)), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
                    }
                    spriteBatch.Draw(crosshair, new Vector2((graphics.PreferredBackBufferWidth / 2) - (crosshair.Width / 2), (graphics.PreferredBackBufferHeight / 2) - (crosshair.Height / 2)), player.GetComponent<PlayerController>().CrosshairColor);
                    if(player.GetComponent<PlayerController>().CrosshairColor == Color.Lime)
                    {
                        string message = "Press F to ";
                        Interaction inter = player.GetComponent<PlayerController>().ClosestObject.GetComponent<Interaction>();
                        if (inter is DoorInteraction) message += "open the door.";
                        if (inter is WeaponInteraction) message += "pick up the gun.";
                        spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.25f * font.MeasureString(message).X / 2, 0.6f*w.Y), Color.Orange, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
                    }
                    Minimap.Draw(ref spriteBatch);
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Tab))
                {
                    Vector2 objectiveSize = 0.25f*font.MeasureString(objectiveString);
                    objectivePosition = new Vector2(w.X / 2 - objectiveSize.X / 2, 0.2f*w.Y - objectiveSize.Y / 2);
                    objectiveTimer = 2;
                }
                spriteBatch.DrawString(font, objectiveString, objectivePosition, Color.Orange, 0, Vector2.Zero, 0.25f, SpriteEffects.None, 0);
#if DRAW_DEBUG_WIREFRAME
                spriteBatch.DrawString(font, player.GlobalPosition.ToString(), new Vector2(5, 5), Color.Green, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
#endif
                /*spriteBatch.DrawString(font, frameCounter.AverageFramesPerSecond.ToString(), new Vector2(50, 45), Color.Black);
                if (particleFireEmitter.GetComponent<ParticleSystem>().getParticlesCount() != null)
                    spriteBatch.DrawString(font, "Fire Particles: " + particleFireEmitter.GetComponent<ParticleSystem>().getParticlesCount().ToString(), new Vector2(50, 75), Color.Purple);
                if (particleExplosionEmitter.GetComponent<ParticleSystem>().getParticlesCount() != null)
                    spriteBatch.DrawString(font, "Explosion Particles: " + particleExplosionEmitter.GetComponent<ParticleSystem>().getParticlesCount().ToString(), new Vector2(50, 90), Color.Purple);
                if (particleSmokeEmitter.GetComponent<ParticleSystem>().getParticlesCount() != null)
                    spriteBatch.DrawString(font, "Smoke Particles: " + particleSmokeEmitter.GetComponent<ParticleSystem>().getParticlesCount().ToString(), new Vector2(50, 105), Color.Purple);
                if (particleBloodEmitter.GetComponent<ParticleSystem>().getParticlesCount() != null)
                    spriteBatch.DrawString(font, "Blood Particles: " + particleBloodEmitter.GetComponent<ParticleSystem>().getParticlesCount().ToString(), new Vector2(50, 120), Color.Purple);
                if (player != null)
                    spriteBatch.DrawString(font, "Player Y position: " + player.GlobalPosition.Y.ToString() + ", velocity: " + player.GetComponent<Rigidbody>().Velocity.ToString(), new Vector2(50, 15), Color.DarkGreen);
                if (testBall != null)
                    spriteBatch.DrawString(font, "TestBall position: " + testBall.GlobalPosition.ToString() + ", velocity: " + testBall.GetComponent<Rigidbody>().Velocity.ToString(), new Vector2(50, 135), Color.DarkGreen);
                if (testBox != null)
                    spriteBatch.DrawString(font, "TestBox position: " + testBox.GlobalPosition.ToString() + ", velocity: " + testBox.GetComponent<Rigidbody>().Velocity.ToString(), new Vector2(50, 150), Color.DarkGreen);*/
                spriteBatch.End();
                //
            }
            base.Draw(gameTime);
        }

        private void DrawTutorialTips(SpriteBatch spriteBatch, SpriteFont font, Point w, Color color, GameTime gameTime)
        {
            double time = gameTime.TotalGameTime.TotalSeconds;
            double start = startTime.Value;
            if (time - start > 1 && time - start < 6)
            {
                string message = "Hold W, S, A and D to move";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 6.5 && time - start < 11.5)
            {
                string message = "Hold Shift to run and C to crouch";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 12 && time - start < 17)
            {
                string message = "Press Left Mouse Button to shoot";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 17.5 && time - start < 21.5)
            {
                string message = "Press R to reload your weapon";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 22 && time - start < 27)
            {
                string message = "Press 1, 2, 3 to select the hologram slot";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 27.5 && time - start < 30.5)
            {
                string message = "Press Q to record hologram into selected slot";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 31 && time - start < 34)
            {
                string message = "Press Q again to stop recording earlier";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 34.5 && time - start < 42.5)
            {
                string message = "Press Z to preview the path hologram will take";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 43 && time - start < 46)
            {
                string message = "Press E to start the hologram";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 46.5 && time - start < 51.5)
            {
                string message = "Press E again to stop the hologram";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }

            if (time - start > 52 && time - start < 59)
            {
                string message = "Hold Tab to see your objective";
                Vector2 size = font.MeasureString(message);
                spriteBatch.DrawString(font, message, new Vector2(w.X / 2 - 0.2f * size.X / 2, 0.85f * w.Y), color, 0, Vector2.Zero, 0.2f, SpriteEffects.None, 0);
            }
        }

        protected Texture2D DrawSceneToTexture(RenderTarget2D currentRenderTarget, GameTime gameTime)
        {
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
            line[1].Position = line[0].Position + player.LocalToWorldMatrix.Forward * 100.0f;
            graphics.GraphicsDevice.DrawUserPrimitives<VertexPosition>(PrimitiveType.LineList, line, 0, 1);

            GraphicsDevice.RasterizerState = originalState;
#endif

            // Drop the render target
            GraphicsDevice.SetRenderTarget(null);
            // Return the texture in the render target
            return currentRenderTarget;
        }

        private void SaveGame(PressedActionArgs args)
        {
            if (gameState == GameState.GameRunning)
            {
                device = null;
                gameState = GameState.Pause;
                StorageDevice.BeginShowSelector(PlayerIndex.One, GetDeviceForSaving, null);
            }
        }

        private void LoadGame(PressedActionArgs args)
        {
            if(gameState == GameState.GameRunning)
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

            gameState = GameState.GameRunning;
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

            gameState = GameState.GameRunning;
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
            }

            

            scene = newScene;
        }
    }
}
