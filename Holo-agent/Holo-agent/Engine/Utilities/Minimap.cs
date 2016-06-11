using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Engine.Utilities
{
    public static class Minimap
    {
        private static Texture2D PlayerIcon;
        private static Texture2D ObjectiveIcon;
        private static Texture2D ObjectiveDirection;
        private static Texture2D EnemyIcon;
        private static Texture2D Frame;
        private static Texture2D[] Maps;
        private static Vector2[] VerticalBounds;
        private static Point Size;
        private static Point Offset;
        private static float Scale; // in units per pixel

        private static Rectangle dest;
        private static Rectangle playerDest;
        private static Point halfSize;
        private static Point halfScaledSize;
        private static Point scaledSize;
        private static Point playerSize;
        private static Point objectiveSize;
        private static Point halfObjectiveSize;
        private static Point enemySize;
        private static Point halfEnemySize;

        private static Vector2 playerRotationOrigin;
        private static Vector2 objectiveRotationOrigin;

        private static GameObject Player;

        public static List<Vector3> Objectives;
        public static List<GameObject> Enemies;

        public static void Initialize(Point size, Point offset, List<Vector2> verticalBounds, GameObject player)
        {
            Size = new Point(size.X, size.Y);
            halfSize = new Point((int)(0.5f * size.X), (int)(0.5f * size.Y));
            Offset = new Point(offset.X, offset.Y);
            dest = new Rectangle(Offset, Size);
            Scale = 1;
            halfScaledSize = new Point((int)(Scale * halfSize.X), (int)(Scale * halfSize.Y));
            scaledSize = new Point((int)(Scale * Size.X), (int)(Scale * Size.Y));
            playerSize = new Point((int)(0.1f * Size.X), (int)(0.1f * Size.Y));
            objectiveSize = new Point((int)(0.07f * Size.X), (int)(0.07f * Size.Y));
            halfObjectiveSize = new Point((int)(0.5f * objectiveSize.X), (int)(0.5f * objectiveSize.Y));
            enemySize = new Point((int)(0.07f * Size.X), (int)(0.07f * Size.Y));
            halfEnemySize = new Point((int)(0.5f * enemySize.X), (int)(0.5f * enemySize.Y));
            playerDest = new Rectangle(Offset + halfSize, playerSize);

            VerticalBounds = new Vector2[verticalBounds.Count];
            for(int i = 0; i < verticalBounds.Count; ++i)
            {
                VerticalBounds[i] = new Vector2(verticalBounds[i].X, verticalBounds[i].Y);
            }
            Maps = new Texture2D[verticalBounds.Count];
            Objectives =  new List<Vector3>();
            Enemies = new List<GameObject>();
            Player = player;
        }

        public static void LoadContent(ContentManager Content)
        {
            PlayerIcon = Content.Load<Texture2D>("Textures/Player_Icon");
            ObjectiveIcon = Content.Load<Texture2D>("Textures/Objective");
            ObjectiveDirection = Content.Load<Texture2D>("Textures/Objective_Direction");
            EnemyIcon = Content.Load<Texture2D>("Textures/Enemy_Icon");
            Frame = Content.Load<Texture2D>("Textures/Minimap_Frame");
            for (int i = 0; i < Maps.Length; ++i)
            {
                Maps[i] = Content.Load<Texture2D>("Textures/Map" + i);
            }

            playerRotationOrigin = new Vector2(0.5f * PlayerIcon.Width, 0.5f * PlayerIcon.Height);
            objectiveRotationOrigin = new Vector2(0.5f * ObjectiveDirection.Width, 0.5f * ObjectiveDirection.Height);
        }

        public static void Draw(ref SpriteBatch batch)
        {
            int i = 0;
            if (Player != null)
            {
                Vector3 pos = Player.GlobalPosition;
                Vector3 playerDir = Matrix.CreateFromQuaternion(Player.GlobalRotation).Forward;
                float playerRot = (float)Math.Atan2(playerDir.X, -playerDir.Z);
                for (; i < VerticalBounds.Length; ++i)
                {
                    if (pos.Y >= VerticalBounds[i].X && pos.Y <= VerticalBounds[i].Y) break;
                }
                Point scaledPlayerPos = new Point((int)(Scale * pos.X), (int)(Scale * pos.Z));
                Point center = Maps[i].Bounds.Center + scaledPlayerPos;
                Rectangle src = new Rectangle(center - halfScaledSize, scaledSize);
                batch.Draw(Maps[i], dest, src, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
                foreach(Vector3 ObjectivePosition in Objectives)
                {
                    bool isOnTheLevel = (ObjectivePosition.Y >= VerticalBounds[i].X && ObjectivePosition.Y <= VerticalBounds[i].Y);
                    Vector2 objectiveDir = (new Vector2(ObjectivePosition.X, ObjectivePosition.Z) - new Vector2(pos.X, pos.Z)) / Scale;
                    if(Math.Abs(objectiveDir.X) <= halfSize.X - halfObjectiveSize.X && Math.Abs(objectiveDir.Y) <= halfSize.Y - halfObjectiveSize.Y)
                    {
                        Point scaledObjectivePos = new Point((int)objectiveDir.X, (int)objectiveDir.Y);
                        Rectangle objectiveDest = new Rectangle(Offset + halfSize + scaledObjectivePos - halfObjectiveSize, objectiveSize);
                        batch.Draw(ObjectiveIcon, objectiveDest, null, (isOnTheLevel ? Color.White : new Color(0.7f, 0.7f, 0.7f)), 0, Vector2.Zero, SpriteEffects.None, 0);
                    }
                    else
                    {
                        float objectiveRot = (float)Math.Atan2(objectiveDir.X, -objectiveDir.Y);

                        if (objectiveDir.X > halfSize.X - halfObjectiveSize.X) objectiveDir.X = halfSize.X - halfObjectiveSize.X;
                        else if (objectiveDir.X < halfObjectiveSize.X - halfSize.X) objectiveDir.X = halfObjectiveSize.X - halfSize.X;
                        if (objectiveDir.Y > halfSize.Y - halfObjectiveSize.Y) objectiveDir.Y = halfSize.Y - halfObjectiveSize.Y;
                        else if (objectiveDir.Y < halfObjectiveSize.Y - halfSize.Y) objectiveDir.Y = halfObjectiveSize.Y - halfSize.Y;

                        Point scaledObjectivePos = new Point((int)objectiveDir.X, (int)objectiveDir.Y);
                        Rectangle objectiveDest = new Rectangle(Offset + halfSize + scaledObjectivePos, objectiveSize);
                        batch.Draw(ObjectiveDirection, objectiveDest, null, (isOnTheLevel ? Color.White : new Color(0.7f, 0.7f, 0.7f)), objectiveRot, objectiveRotationOrigin, SpriteEffects.None, 0);
                    }
                }
                foreach (GameObject Enemy in Enemies)
                {
                    Vector3 EnemyPosition = Enemy.GlobalPosition;
                    bool isOnTheLevel = (EnemyPosition.Y >= VerticalBounds[i].X && EnemyPosition.Y <= VerticalBounds[i].Y);
                    Vector2 enemyDir = (new Vector2(EnemyPosition.X, EnemyPosition.Z) - new Vector2(pos.X, pos.Z)) / Scale;
                    if (Math.Abs(enemyDir.X) <= halfSize.X - halfEnemySize.X && Math.Abs(enemyDir.Y) <= halfSize.Y - halfEnemySize.Y)
                    {
                        Point scaledEnemyPos = new Point((int)enemyDir.X, (int)enemyDir.Y);
                        Rectangle enemyDest = new Rectangle(Offset + halfSize + scaledEnemyPos - halfEnemySize, enemySize);
                        batch.Draw(EnemyIcon, enemyDest, null, (isOnTheLevel ? Color.White : new Color(0.7f, 0.7f, 0.7f)), 0, Vector2.Zero, SpriteEffects.None, 0);
                    }
                }
                batch.Draw(PlayerIcon, playerDest, null, Color.White, playerRot, playerRotationOrigin, SpriteEffects.None, 0);
                batch.Draw(Frame, dest, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            }
        }
    }
}