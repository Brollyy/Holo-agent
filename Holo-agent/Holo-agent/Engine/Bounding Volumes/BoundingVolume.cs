using Microsoft.Xna.Framework;
using Engine.Components;
using System;

namespace Engine.Bounding_Volumes
{
    public abstract class BoundingVolume
    {
        public Collider Collider
        {
            get;
            set;
        }

        public Vector3 Center
        {
            get;
            set;
        }

        public Vector3 GlobalCenter()
        {
            if(Collider == null || Collider.Owner == null)
            {
                return Center;
            }
            else
            {
                return Vector3.Transform(Center, Collider.Owner.LocalToWorldMatrix);
            }
        }

        public int IsOverlapping(BoundingVolume other)
        {
            if (other is BoundingSphere)
            {
                return IsOverlappingSphere(other as BoundingSphere);
            }
            else if (other is BoundingBox)
            {
                return IsOverlappingBox(other as BoundingBox);
            }
            else if (other is BoundingCylinder)
            {
                return IsOverlappingCylinder(other as BoundingCylinder);
            }
            else if(other == null)
            {
                return 0;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected abstract int IsOverlappingSphere(BoundingSphere other);
        protected abstract int IsOverlappingBox(BoundingBox other);
        protected abstract int IsOverlappingCylinder(BoundingCylinder other);

        public BoundingVolume() : this(null, Vector3.Zero)
        {
        }

        public BoundingVolume(Collider collider, Vector3 center)
        {
            Collider = collider;
            Center = center;
        }
    }
}
