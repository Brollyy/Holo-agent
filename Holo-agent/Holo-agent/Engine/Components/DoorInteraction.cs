using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Engine.Components
{
    [DataContract]
    public class DoorInteraction : Interaction 
    {
        private static Quaternion DEFAULT_OPENED = Quaternion.CreateFromYawPitchRoll(MathHelper.ToRadians(90), 0, 0);
        private static Quaternion DEFAULT_CLOSED = Quaternion.Identity;
        [DataMember]
        private bool open = false;
        [DataMember]
        private bool locked = false;
        private bool interacting = false;
        [DataMember]
        private Quaternion openRotation;
        [DataMember]
        private Quaternion closedRotation;
        private float t = 0.0f;
        [DataMember]
        private float time = 1.0f;

        [IgnoreDataMember]
        public bool IsOpen { get { return open; } }

        [IgnoreDataMember]
        public bool IsLocked { get { return locked; } set { locked = value; } }

        public DoorInteraction() : this(null, null, 1)
        {
        }

        public DoorInteraction(Quaternion? opened = null, Quaternion? closed = null, float time = 1.0f)
        {
            openRotation = (opened == null ? DEFAULT_OPENED :  opened.Value);
            closedRotation = (closed == null ? DEFAULT_CLOSED : closed.Value);
            this.time = time;
        }

        protected override void InitializeNewOwner(GameObject newOwner)
        {
            base.InitializeNewOwner(newOwner);
            if(newOwner != null && newOwner.Parent != null) newOwner.Parent.LocalQuaternionRotation = (open ? openRotation : closedRotation);
        }

        public override void Update(GameTime gameTime)
        {
            if(interacting)
            {
                if(t < time)
                {
                    t += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    if(Owner != null && Owner.Parent != null)
                        Owner.Parent.LocalQuaternionRotation = Quaternion.Lerp(openRotation, closedRotation, (open ? (1.0f - t / time) : t / time));
                }
                else
                {
                    interacting = false;
                }
            }
        }

        public override void Interact(GameObject go, Vector3 point)
        {
            if(!locked && !interacting)
            {
                interacting = true;
                t = 0.0f;
                open = !open;
            }
        }

    }
}
