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

        protected override CollisionResult IsOverlappingBox(BoundingBox other)
        {
            // First trivial case.
            #region SPHERE_CHECK
            BoundingSphere sphere = new BoundingSphere();
            sphere.Center = other.GlobalCenter();
            sphere.Radius = other.HalfDiagonal();

            CollisionResult resSphere = sphere.IsOverlapping(this);
            if (!resSphere.CollisionDetected) return resSphere;
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
                        return new CollisionResult(true, planes[i], intersect);
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
                            if ((X - intersect).LengthSquared() <= interRadius)
                                return new CollisionResult(true, planes[i], X);
                        }
                        // Check vertices of the edge.
                        if (AP.LengthSquared() <= interRadius)
                            return new CollisionResult(true, planes[i], faceCorners[ind[4 * i + j]]);
                        if ((faceCorners[ind[4 * i + ((j + 1) % 4)]] - intersect).LengthSquared() <= interRadius)
                            return new CollisionResult(true, planes[i], faceCorners[ind[4 * i + ((j + 1) % 4)]]);
                        #endregion
                    }
                }
            }

            // The sphere can still be inside the box without touching it.
            if(inside)
            {
                Vector3 otherCenter = other.GlobalCenter();
                Vector3 centerDiff = center - otherCenter;
                centerDiff.Normalize();
                return new CollisionResult(true, new Plane(centerDiff, Vector3.Dot(otherCenter, centerDiff)), otherCenter);
            }
            else
            {
                return new CollisionResult();
            }

        }

        protected override CollisionResult IsOverlappingCylinder(BoundingCylinder other)
        {
            throw new NotImplementedException();
        }

        protected override CollisionResult IsOverlappingSphere(BoundingSphere other)
        {
            double radiusSumSquared = (Radius + other.Radius) * (Radius + other.Radius);
            Vector3 otherCenter = other.GlobalCenter();
            Vector3 centerDiff = GlobalCenter() - otherCenter;
            if (radiusSumSquared > centerDiff.LengthSquared())
            {
                centerDiff.Normalize();
                CollisionResult res;
                res.CollisionDetected = true;
                res.CollisionPoint = otherCenter + (float)other.Radius * centerDiff;
                res.CollisionPlane = new Plane(centerDiff, Vector3.Dot(res.CollisionPoint.Value, centerDiff));
                return res;
            }
            else return new CollisionResult();
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
