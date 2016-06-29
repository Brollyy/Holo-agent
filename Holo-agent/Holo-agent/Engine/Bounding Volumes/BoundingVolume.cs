using Microsoft.Xna.Framework;
using Engine.Components;
using System;
using System.Runtime.Serialization;

namespace Engine.Bounding_Volumes
{
    public struct CollisionResult
    {
        public bool CollisionDetected;
        public Plane? CollisionPlane;
        public Vector3? CollisionPoint;
        public Plane? CollidedPlane;

        public CollisionResult(bool detect = false, Plane? plane = null, Plane? plane2 = null, Vector3? point = null)
        {
            CollisionDetected = detect;
            CollisionPlane = plane;
            CollisionPoint = point;
            CollidedPlane = plane2;
        }
    }

    [DataContract]
    public abstract class BoundingVolume
    {
        [DataMember]
        public Collider Collider
        {
            get;
            set;
        }

        [DataMember]
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

        public CollisionResult IsOverlapping(BoundingVolume other)
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
                return new CollisionResult();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        protected abstract CollisionResult IsOverlappingSphere(BoundingSphere other);
        protected abstract CollisionResult IsOverlappingBox(BoundingBox other);
        protected abstract CollisionResult IsOverlappingCylinder(BoundingCylinder other);

        public BoundingVolume() : this(null, Vector3.Zero)
        {
        }

        public BoundingVolume(Collider collider, Vector3 center)
        {
            Collider = collider;
            Center = center;
        }

        public BoundingVolume(Vector3 center) : this(null, center)
        {
        }
    }
}
