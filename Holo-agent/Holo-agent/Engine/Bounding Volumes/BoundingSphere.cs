using System;
using Microsoft.Xna.Framework;
using Engine.Components;

namespace Engine.Bounding_Volumes
{
    public class BoundingSphere : BoundingVolume
    {
        private double radius;

        public double Radius
        {
            get
            {
                return radius;
            }
            set
            {
                if (value > 0.0) radius = value;
            }
        }

        protected override int IsOverlappingBox(BoundingBox other)
        {
            // First trivial case.
            BoundingSphere sphere = new BoundingSphere();
            sphere.Center = other.GlobalCenter();
            sphere.Radius = other.HalfDiagonal();

            if (sphere.IsOverlapping(this) == 0) return 0;

            // Need to check if any of the faces of the cube are intersecting sphere.
            bool inside = true;
            // First find intersection point with plane of the face, then check if point lies on face.
            Vector3[] faceCorners = other.Corners();
            Vector3 center = GlobalCenter();
            Plane[] planes = new Plane[6] 
            {
                other.FrontFace(), other.BackFace(),
                other.LeftFace(), other.RightFace(),
                other.UpFace(), other.DownFace()
            };
            // Corners: 0 - right upper front, 1 - left upper front, 2 - left lower front,
            // 3 - right lower front, 4 - right upper back, 5 - left upper back,
            // 6 - left lower back, 7 - right lower back.
            int[] ind = new int[18]
            {
                1, 0, 2,
                5, 4, 6,
                1, 2, 5,
                0, 3, 4,
                0, 1, 4,
                2, 3, 6
            };
            for (int i = 0; i < 6; ++i)
            {
                float t = planes[i].D - Vector3.Dot(planes[i].Normal, center);
                if (t > 0.0f) inside = false;
                if (Math.Abs(t) <= radius)
                {
                    Vector3 intersect = center + t * planes[i].Normal;
                    float dot1 = Vector3.Dot(intersect, faceCorners[ind[3 * i + 1]] - faceCorners[ind[3*i]]);
                    float dot2 = Vector3.Dot(intersect, faceCorners[ind[3 * i + 2]] - faceCorners[ind[3 * i]]);
                    if (Vector3.Dot(faceCorners[ind[3 * i]], faceCorners[ind[3 * i + 1]] - faceCorners[ind[3 * i]]) <= dot1 &&
                        Vector3.Dot(faceCorners[ind[3 * i + 1]], faceCorners[ind[3 * i + 1]] - faceCorners[ind[3 * i]]) >= dot1 &&
                        Vector3.Dot(faceCorners[ind[3 * i]], faceCorners[ind[3 * i + 2]] - faceCorners[ind[3 * i]]) <= dot2 &&
                        Vector3.Dot(faceCorners[ind[3 * i + 2]], faceCorners[ind[3 * i + 2]] - faceCorners[ind[3 * i]]) >= dot2)
                    {
                        return i+1;
                    }
                }
            }

            // The sphere can still be inside the box without touching it.
            return (inside ? 7 : 0);

        }

        protected override int IsOverlappingCylinder(BoundingCylinder other)
        {
            throw new NotImplementedException();
        }

        protected override int IsOverlappingSphere(BoundingSphere other)
        {
            double radiusSquared = (Radius + other.Radius) * (Radius + other.Radius);
            return (radiusSquared.CompareTo((GlobalCenter() - other.GlobalCenter()).LengthSquared()) >= 0 ? 1 : 0);
        }

        public BoundingSphere() : this(null, Vector3.Zero, 1.0f)
        {
        }

        public BoundingSphere(Collider collider, Vector3 center, float radius) : base(collider, center)
        {
            Radius = radius;
        }
    }
}
