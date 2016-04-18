using Microsoft.Xna.Framework;
using Engine.Bounding_Volumes;
using System;

namespace Engine.Utilities
{
    public class Raycast
    {
        public Vector3 Direction
        {
            get;
            set;
        }

        public Vector3 StartPosition
        {
            get;
            set;
        }

        public float Length
        {
            get;
            set;
        }

        public float? Intersect(BoundingVolume bound)
        {
            if(bound is Bounding_Volumes.BoundingBox)
            {
                return IntersectBox(bound as Bounding_Volumes.BoundingBox);
            }
            else if(bound is Bounding_Volumes.BoundingSphere)
            {
                return IntersectSphere(bound as Bounding_Volumes.BoundingSphere);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private float? IntersectBox(Bounding_Volumes.BoundingBox bound)
        {
            float tfirst = 0.0f, tlast = Length;

            Plane[] planes = new Plane[3]
            {
                bound.RightFace(), bound.UpFace(), bound.BackFace()
            };
            Vector3[] corners = bound.Corners();

            if (!RaySlabIntersect(Vector3.Dot(StartPosition, planes[0].Normal), 
                                  Vector3.Dot(Direction, planes[0].Normal), 
                                  Vector3.Dot(corners[1], planes[0].Normal), 
                                  Vector3.Dot(corners[0], planes[0].Normal), 
                                  ref tfirst, ref tlast)) return null;
            if (!RaySlabIntersect(Vector3.Dot(StartPosition, planes[1].Normal),
                                  Vector3.Dot(Direction, planes[1].Normal),
                                  Vector3.Dot(corners[3], planes[1].Normal),
                                  Vector3.Dot(corners[0], planes[1].Normal),
                                  ref tfirst, ref tlast)) return null;
            if (!RaySlabIntersect(Vector3.Dot(StartPosition, planes[2].Normal),
                                  Vector3.Dot(Direction, planes[2].Normal),
                                  Vector3.Dot(corners[0], planes[2].Normal),
                                  Vector3.Dot(corners[4], planes[2].Normal),
                                  ref tfirst, ref tlast)) return null;

            return tfirst;
        }

        bool RaySlabIntersect(float start, float dir, float min, float max, ref float tfirst, ref float tlast)
        {
            if (Math.Abs(dir) < 0.0001f)
            {
                return (start < max && start > min);
            }

            float tmin = (min - start) / dir;
            float tmax = (max - start) / dir;
            if (tmin > tmax)
            {
                float temp = tmax;
                tmax = tmin;
                tmin = temp;
            }

            if (tmax < tfirst || tmin > tlast)
                return false;

            if (tmin > tfirst) tfirst = tmin;
            if (tmax < tlast) tlast = tmax;
            return true;
        }

        private float? IntersectSphere(Bounding_Volumes.BoundingSphere bound)
        {
            Vector3 L = bound.GlobalCenter() - StartPosition;
            double tca = Vector3.Dot(L, Direction);
            if (tca < 0.0f) return null;
            double d2 = L.LengthSquared() - tca * tca;
            double radius2 = (float)(bound.Radius * bound.Radius);
            if (d2 > radius2) return null;
            double thc = Math.Sqrt(radius2 - d2);
            return (float)(tca - thc);
        }

        public Raycast()
        {
            Direction = Vector3.Forward;
            StartPosition = Vector3.Zero;
            Length = 0.0f;
        }

        public Raycast(Vector3 startingPosition, Vector3 direction, float length)
        {
            if(direction.LengthSquared() < 0.0001f)
            {
                Direction = Vector3.Forward;
            }
            else
            {
                Direction = direction;
            }

            StartPosition = startingPosition;

            if (length > 0.0f) Length = length;
            else Length = 0.0f;
        }
    }
}
