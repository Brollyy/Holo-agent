using Engine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Components
{
    [DataContract]
    public abstract class CharacterController : Component
    {
        [DataMember]
        protected float walkSpeed;
        [DataMember]
        protected float walkVolume;
        [DataMember]
        protected float runSpeed;
        [DataMember]
        protected float runVolume;
        [DataMember]
        protected float crouchSpeed;
        [DataMember]
        protected float crouchVolume;
        protected Movement movement;

        [DataMember]
        protected float? closestObjectDistance;
        [DataMember]
        protected GameObject closestObject;

        public Movement Movement { get { return movement; } }

        [DataMember]
        protected float health = 100;
        [DataMember]
        protected float maxHealth = 100;
        protected float regenRate = 20;
        protected float regenCooldown = 3;
        protected float regenTimer = 3;
        private List<SoundEffectInstance> ouchSounds;
        public List<SoundEffectInstance> OuchSounds
        {
            set
            {
                ouchSounds = value;
            }
        }

        public float Health { get { return health; } }

        public GameObject ClosestObject
        {
            get
            {
                return closestObject;
            }
        }
        public float? ClosestObjectDistance
        {
            get
            {
                return closestObjectDistance;
            }
        }

        public virtual void DealDamage(float amount, Weapon causer)
        {
            if (ouchSounds != null && ouchSounds.Count > 0)
            {
                Random random = new Random();
                int index = random.Next(0, ouchSounds.Count);
                if(!ouchSounds.Any(ouch => ouch.State.Equals(SoundState.Playing)))
                    ouchSounds[index].Play();
            }
            health -= amount;
            regenTimer = 0;
            if (health <= 0) HandleDeath();
        }

        protected virtual void Ray(float maxDistance, List<GameObject> objects, Vector3 direction)
        {
            closestObjectDistance = null;
            closestObject = null;
            Raycast ray = new Raycast(Owner.GlobalPosition, direction, maxDistance);
            foreach (GameObject go in objects)
            {
                if (go == Owner) continue;
                Collider col = go.GetComponent<Collider>();
                if (go.IsVisible && go.Name != "Level")
                {
                    float? distance = (col != null ? ray.Intersect(col.bound) : ray.Intersect(go.Bound));
                    if (distance != null)
                    {
                        if (closestObjectDistance == null || distance < closestObjectDistance)
                        {
                            closestObjectDistance = distance;
                            closestObject = go;
                        }
                    }
                }
            }
        }

        protected virtual void HandleDeath()
        {
            Owner.IsVisible = false;
            Owner.Scene.Destroy(Owner);
        }

        public override void Update(GameTime gameTime)
        {
            if(regenTimer < regenCooldown)
            {
                regenTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            else if (health < maxHealth)
            {
                health += (float)gameTime.ElapsedGameTime.TotalSeconds * regenRate;
                if (health > maxHealth) health = maxHealth;
            }
        }

        public CharacterController() : 
            this(80.0f, 0.5f, 150.0f, 1.0f, 40.0f, 0.0f)
        {
        }

        public CharacterController(float walkSpeed, float walkVolume, float runSpeed, float runVolume, float crouchSpeed, float crouchVolume)
        {
            this.walkSpeed = walkSpeed;
            this.walkVolume = walkVolume;
            this.runSpeed = runSpeed;
            this.runVolume = runVolume;
            this.crouchSpeed = crouchSpeed;
            this.crouchVolume = crouchVolume;
            this.movement = Movement.WALK;
        }
    }

    public enum Movement
    {
        WALK = 0,
        RUN,
        CROUCH,
        IDLE
    }
}
