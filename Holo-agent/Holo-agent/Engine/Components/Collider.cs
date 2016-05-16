using Engine.Bounding_Volumes;
using Microsoft.Xna.Framework;

namespace Engine.Components
{
    public class Collider : Component
    {
        public BoundingVolume bound;
        public int Collide(Collider other)
        {
            if (bound == null || other == null) return 0;
            return bound.IsOverlapping(other.bound);
        }

        public Collider() : this(null)
        {
        }

        public Collider(BoundingVolume bound)
        {
            this.bound = bound;
        }
    }
}
