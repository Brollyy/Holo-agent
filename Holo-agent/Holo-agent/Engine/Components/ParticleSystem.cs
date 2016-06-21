using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Engine.Components
{
    [DataContract]
    class ParticleSystem : Component
    {
        [DataMember]
        private ParticleSystemType type;
        [DataMember]
        private int amount;
        private List<Particle> particles;
        [DataMember]
        private readonly float lifeTime;
        [DataMember]
        private List<SpriteInstance> forms;
        [DataMember]
        private float size;
        private Guid particleID;
        private Random random;
        public float? getParticlesCount() //Delete later.
        {
            if (particles != null)
                return particles.Count;
            else
                return null;
        }
        public ParticleSystem(ParticleSystemType type, int amount, float lifeTime, List<SpriteInstance> forms, float size)
        {
            this.type = type;
            this.amount = amount;
            particles = new List<Particle>();
            this.lifeTime = lifeTime;
            this.forms = forms;
            this.size = size;
            particleID = new Guid();
            random = new Random();
        }
        public void Init()
        {
            if (type == ParticleSystemType.Fire)
            {
                float angle1 = 170;
                for (int i = 0; i <= 3; i++)
                {
                    float angle2 = 170;
                    for (int j = 0; j < 3; j++)
                    {
                        particleID = Guid.NewGuid();
                        CreateParticle(Convert.ToBase64String(particleID.ToByteArray()), new Vector3((float)Math.Sin(MathHelper.ToRadians(angle2)) * (float)Math.Sqrt(30), (float)Math.Cos(MathHelper.ToRadians(angle1)) * (float)Math.Sqrt(30), (float)Math.Sin(MathHelper.ToRadians(angle1)) * (float)Math.Sqrt(30)));
                        angle2 += 10f;
                    }
                    angle1 += 10f;
                }
            }
            if (type == ParticleSystemType.Explosion)
            {
                for (int i = 0; i < amount; i++)
                {
                    particleID = Guid.NewGuid();
                    CreateParticle(Convert.ToBase64String(particleID.ToByteArray()), new Vector3(random.Next(-6, 6), random.Next(-6, 6), random.Next(-6, 6)));
                }
            }
            if (type == ParticleSystemType.Smoke)
            {
                particleID = Guid.NewGuid();
                CreateParticle(Convert.ToBase64String(particleID.ToByteArray()), new Vector3(0, 0, (float)random.NextDouble()));
            }
            if (type == ParticleSystemType.Jet)
            {
                for (int i = 0; i < amount; i++)
                {
                    particleID = Guid.NewGuid();
                    CreateParticle(Convert.ToBase64String(particleID.ToByteArray()), new Vector3((float)Math.Sin(MathHelper.ToRadians(random.Next(0,360))) * (float)Math.Sqrt(0.1f), (float)Math.Cos(MathHelper.ToRadians(random.Next(0, 360)) * Math.Sqrt(0.1f)), (float)Math.Sin(MathHelper.ToRadians(random.Next(0, 360)) * Math.Sqrt(0.1f))));
                }
            }
        }
        private void CreateParticle(string particleID, Vector3 position = default(Vector3))
        {
            int i = 0;
            if (forms.Count > 1)
                i = random.Next(0, forms.Count);
            particles.Add(new Particle(particleID, type, lifeTime, forms[i], forms[i].Alpha, size, position, Owner));
        }
        override public void Update(GameTime gameTime)
        {
            if (particles != null && particles.Count > 0)
            {
                for (int i = 0; i < particles.Count; i++)
                {
                    if (particles[i] != null)
                    {
                        particles[i].Update(gameTime);
                        particles[i].LifeTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (particles[i].LifeTime < 0)
                        {
                            particles[i].Destroy();
                            particles.Remove(particles[i]);
                        }
                    }
                }
                if (type == ParticleSystemType.Fire || type == ParticleSystemType.Smoke)
                {
                    Init();
                    int i = 0;
                    while (particles.Count > amount)
                    {
                        particles[i].Destroy();
                        particles.Remove(particles[i]);
                        i++;
                    }
                }
            }
        }
        public override void Destroy()
        {
            forms = null;
            particles = null;
        }
        private class Particle
        {
            private string ID;
            private ParticleSystemType type;
            private float lifeTime;
            private SpriteInstance form;
            private float alpha;
            private float size;
            private Vector3 position;
            private GameObject body;
            public float LifeTime
            {
                get
                {
                    return lifeTime;
                }
                set
                {
                    lifeTime = value;
                }
            }
            public Particle(string ID, ParticleSystemType type, float lifeTime, SpriteInstance form, float alpha, float size, Vector3 position, GameObject parent)
            {
                this.ID = ID;
                this.type = type;
                this.lifeTime = lifeTime;
                this.form = form;
                this.alpha = alpha;
                this.size = size;
                this.position = position;
                CreateBody(ID, parent);
            }
            private void CreateBody(string ID, GameObject parent)
            {
                body = new GameObject(ID, position, Quaternion.Identity, Vector3.One, parent.Scene, parent);
                body.AddComponent(new SpriteInstance(form));
            }
            public void Update(GameTime gameTime)
            {
                Random random = new Random();
                if (type == ParticleSystemType.Fire)
                {
                    size = MathHelper.Lerp(size, 0.05f, 0.02f);
                    position.X = MathHelper.Lerp(position.X, random.Next(-30, 30), 0.02f);
                    position.Y = MathHelper.Lerp(position.Y, (16 - position.Z * 6), 0.0075f);
                    position.Z = MathHelper.Lerp(position.Z, random.Next(-30, 30), 0.02f);
                }
                if (type == ParticleSystemType.Explosion)
                {
                    alpha = MathHelper.Lerp(alpha, 0, 0.025f / lifeTime);
                    size = MathHelper.Lerp(size, 6, 0.025f);
                }
                if (type == ParticleSystemType.Smoke)
                {
                    size = MathHelper.Lerp(size, 5, 0.01f);
                    position.Y = MathHelper.Lerp(position.Y, 36, 0.005f);
                }
                if (type == ParticleSystemType.Jet)
                {
                    alpha = MathHelper.Lerp(alpha, 0, 0.05f / lifeTime);
                    size = MathHelper.Lerp(size, 0.01f, 0.02f);
                    position.X = MathHelper.Lerp(position.X, position.X * (float)Math.Sqrt(30), 0.01f);
                    position.Y = MathHelper.Lerp(position.Y, position.Y * (float)Math.Sqrt(30), 0.01f);
                    position.Z = MathHelper.Lerp(position.Z, position.Z * (float)Math.Sqrt(30), 0.01f);
                }
                body.LocalPosition = position;
                body.LocalQuaternionRotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateConstrainedBillboard(body.GlobalPosition, body.Scene.Camera.GlobalPosition, Vector3.UnitY, null, null));
                body.LocalScale = new Vector3(size);
                body.GetComponent<SpriteInstance>().Alpha = alpha;
            }
            public void Destroy()
            {
                form.Destroy();
                body.Destroy();
            }
        }
    }
    public enum ParticleSystemType
    {
        Fire = 0,
        Explosion = 1,
        Smoke = 2,
        Jet = 3
    }
}
