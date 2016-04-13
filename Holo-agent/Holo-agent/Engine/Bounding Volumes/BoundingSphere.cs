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
            #region SPHERE_CHECK
            BoundingSphere sphere = new BoundingSphere();
            sphere.Center = other.GlobalCenter();
            sphere.Radius = other.HalfDiagonal();

            if (sphere.IsOverlapping(this) == 0) return 0;
            #endregion

            // Need to check if any of the faces of the cube are intersecting sphere.
            bool inside = true;
            // First find intersection point with plane of the face, then check if casted circle intersects face.
            #region PLANES_VERTICES_DEFINITION
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
            int[] ind = new int[24]
            {
                0, 1, 2, 3,
                4, 5, 6, 7,
                5, 1, 2, 6,
                4, 0, 3, 7,
                0, 1, 5, 4,
                3, 2, 6, 7
            };
            #endregion

            for (int i = 0; i < 6; ++i)
            {
                float t = planes[i].D - Vector3.Dot(planes[i].Normal, center);
                if (t < 0.0f) inside = false;
                if (Math.Abs(t) <= radius)
                {
                    // Check if circle created by casting sphere on plane intersect with box's face.
                    Vector3 intersect = center + t * planes[i].Normal;
                    float interRadius = (float)Math.Sqrt(radius * radius - t * t);
                    // Trivial check.
                    #region CIRCLE_CHECK
                    Vector3 faceCenter = (faceCorners[ind[4 * i]] + faceCorners[ind[4 * i + 2]]) / 2.0f;
                    if (interRadius + Vector3.Distance(faceCenter, faceCorners[ind[4 * i]]) < Vector3.Distance(intersect, faceCenter))
                        continue;
                    #endregion
                    // Intersection if circle's center lies in rectangle.
                    // 0 <= AP*AB <= AB*AB && 0 <= AP*AD <= AD*AD
                    #region CIRCLE_CENTER_INSIDE_FACE
                    Vector3 AP = intersect - faceCorners[ind[4*i]];
                    Vector3 AB = faceCorners[ind[4 * i + 1]] - faceCorners[ind[4 * i]];
                    Vector3 AD = faceCorners[ind[4 * i + 3]] - faceCorners[ind[4 * i]];
                    float APAB = Vector3.Dot(AP, AB);
                    float APAD = Vector3.Dot(AP, AD);
                    if (APAB >= 0 && APAB <= AB.LengthSquared() && APAD >= 0 && APAD <= AD.LengthSquared())
                    {
                        return i + 1;
                    }
                    #endregion
                    // Intersection if any edge lies close enough and shares at least one point with circle.
                    for (int j = 0; j < 4; ++j)
                    {
                        #region EDGE_INTERSECT_CIRCLE
                        AP = intersect - faceCorners[ind[4 * i + j]];
                        AB = faceCorners[ind[4 * i + ((j+1) % 4)]] - faceCorners[ind[4 * i + j]];
                        float t1 = Vector3.Dot(AP, AB) / AB.LengthSquared();
                        if(t1 > 0.0f && t1 < 1.0f)
                        {
                            Vector3 X = faceCorners[ind[4 * i + j]] + t1 * AB;
                            if ((X - intersect).LengthSquared() <= interRadius) return i + 1;
                        }
                        // Check vertices of the edge.
                        if (AP.LengthSquared() <= interRadius) return i + 1;
                        if ((faceCorners[ind[4 * i + ((j + 1) % 4)]] - intersect).LengthSquared() <= interRadius) return i + 1;
                        #endregion
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
