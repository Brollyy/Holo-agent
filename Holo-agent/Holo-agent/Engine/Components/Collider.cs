using Engine.Bounding_Volumes;
using Engine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace Engine.Components
{
    public delegate void TriggerAction();

    [DataContract]
    public class Collider : Component
    {
        [DataMember]
        public BoundingVolume bound;
        [DataMember]
        public bool IsTrigger { get; set; }
        [DataMember]
        private TriggerAction handler = null;
        [DataMember]
        private bool isTriggered = false;
        [DataMember]
        private bool canBeTriggeredMultipleTimes = false;

        public void Trigger()
        {
            if(!isTriggered)
            {
                isTriggered = true;
                if (handler != null) handler();
            }
        }

        public void Untrigger()
        {
            if(isTriggered && canBeTriggeredMultipleTimes)
            {
                isTriggered = false;
            }
        }

        public CollisionResult Collide(Collider other)
        {
            if (other == null) return new CollisionResult();
            if (bound == null) this.bound = new Bounding_Volumes.BoundingBox(this, 
                new Pair<Vector3, Vector3>(Vector3.Transform(Owner.Bound.Min, Owner.WorldToLocalMatrix), Vector3.Transform(Owner.Bound.Max, Owner.WorldToLocalMatrix)));
            if(other.bound == null) other.bound = new Bounding_Volumes.BoundingBox(other,
                new Pair<Vector3, Vector3>(Vector3.Transform(other.Owner.Bound.Min, other.Owner.WorldToLocalMatrix), Vector3.Transform(other.Owner.Bound.Max, other.Owner.WorldToLocalMatrix)));

            return bound.IsOverlapping(other.bound);
        }

        public void DrawDebug(GameTime gameTime, GraphicsDeviceManager graphics)
        {
            short[] indexes = new short[24]
            {
                0, 1, 1, 2, 2, 3, 3, 0,
                4, 5, 5, 6, 6, 7, 7, 4,
                0, 4, 1, 5, 2, 6, 3, 7
            };

            Bounding_Volumes.BoundingBox box = (bound as Bounding_Volumes.BoundingBox);
            if (box != null)
            {
                Vector3[] corners = box.Corners();

                VertexPositionColor[] vertices = new VertexPositionColor[8];
                for (int j = 0; j < 8; ++j)
                {
                    vertices[j] = new VertexPositionColor(corners[j], Color.Green);
                }
                
                graphics.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, vertices, 0, 8, indexes, 0, 12);
            }
        }

        public Collider() : this(null)
        {
        }

        public Collider(BoundingVolume bound)
        {
            this.bound = bound;
        }

        public Collider(bool isTrigger, bool canBeTriggeredMultipleTimes, TriggerAction handler)
        {
            IsTrigger = isTrigger;
            this.handler = handler;
            this.canBeTriggeredMultipleTimes = canBeTriggeredMultipleTimes;
        }
    }
}
