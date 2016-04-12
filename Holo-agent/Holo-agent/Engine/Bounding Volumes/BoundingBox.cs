using System;
using Microsoft.Xna.Framework;
using Engine.Components;

namespace Engine.Bounding_Volumes
{
    public class BoundingBox : BoundingVolume
    {
        private Vector3 halfLengths;

        public float HalfWidth
        {
            get { return halfLengths.Z; }
            set { if (value > 0.0) halfLengths.Z = value; }
        }

        public float Width
        {
            get { return halfLengths.Z + halfLengths.Z; }
            set { if (value > 0.0) halfLengths.Z = value / 2.0f; }
        }

        public float HalfLength
        {
            get { return halfLengths.X; }
            set { if (value > 0.0) halfLengths.X = value; }
        }

        public float Length
        {
            get { return halfLengths.X + halfLengths.X; }
            set { if (value > 0.0) halfLengths.X = value / 2.0f; }
        }

        public float HalfHeight
        {
            get { return halfLengths.Y; }
            set { if (value > 0.0) halfLengths.Y = value; }
        }

        public float Height
        {
            get { return halfLengths.Y + halfLengths.Y; }
            set { if (value > 0.0) halfLengths.Y = value / 2.0f; }
        }

        public float HalfDiagonal()
        {
            return halfLengths.Length();
        }

        public Plane FrontFace()
        {
            Matrix world;
            if (Collider == null || Collider.Owner == null)
            {
                world = Matrix.Identity;
            }
            else
            {
                world = Collider.Owner.LocalToWorldMatrix;
            }
            return new Plane(Vector3.TransformNormal(Vector3.Forward, world), halfLengths.Z);
        }

        public Vector3[] Corners()
        {
            Matrix world;
            if (Collider == null || Collider.Owner == null)
            {
                world = Matrix.Identity;
            }
            else
            {
                world = Collider.Owner.LocalToWorldMatrix;
            }
            Vector3[] corners = new Vector3[8];
            for (int i = 0; i < 8; ++i)
            {
                corners[i] = Vector3.Transform(new Vector3(HalfLength * (i%4==0 || i%4==3 ? 1 : -1), 
                                                           HalfHeight * (i/2==0 || i/2==2 ? 1 : -1), 
                                                           HalfWidth * (i/4==0 ? -1 : 1)) + Center, world);
            }
            return corners;
        } 

        public Plane BackFace()
        {
            Matrix world;
            if (Collider == null || Collider.Owner == null)
            {
                world = Matrix.Identity;
            }
            else
            {
                world = Collider.Owner.LocalToWorldMatrix;
            }
            return new Plane(Vector3.TransformNormal(Vector3.Backward, world), -halfLengths.Z);
        }

        public Plane LeftFace()
        {
            Matrix world;
            if (Collider == null || Collider.Owner == null)
            {
                world = Matrix.Identity;
            }
            else
            {
                world = Collider.Owner.LocalToWorldMatrix;
            }
            return new Plane(Vector3.TransformNormal(Vector3.Left, world), halfLengths.X);
        }

        public Plane RightFace()
        {
            Matrix world;
            if (Collider == null || Collider.Owner == null)
            {
                world = Matrix.Identity;
            }
            else
            {
                world = Collider.Owner.LocalToWorldMatrix;
            }
            return new Plane(Vector3.TransformNormal(Vector3.Right, world), -halfLengths.X);
        }

        public Plane UpFace()
        {
            Matrix world;
            if (Collider == null || Collider.Owner == null)
            {
                world = Matrix.Identity;
            }
            else
            {
                world = Collider.Owner.LocalToWorldMatrix;
            }
            return new Plane(Vector3.TransformNormal(Vector3.Up, world), -halfLengths.Y);
        }

        public Plane DownFace()
        {
            Matrix world;
            if (Collider == null || Collider.Owner == null)
            {
                world = Matrix.Identity;
            }
            else
            {
                world = Collider.Owner.LocalToWorldMatrix;
            }
            return new Plane(Vector3.TransformNormal(Vector3.Down, world), halfLengths.Y);
        }

        protected override int IsOverlappingBox(BoundingBox other)
        {
            Vector3[] cornersFirst = Corners();
            Vector3[] cornersSecond = other.Corners();
            // First check for trivial case.
            Vector3 min1 = cornersFirst[0], 
                    min2 = cornersSecond[0], 
                    max1 = cornersFirst[0], 
                    max2 = cornersSecond[0];
            for(int i = 1; i < 8; ++i)
            {
                if (cornersFirst[i].X > max1.X) max1.X = cornersFirst[i].X;
                if (cornersFirst[i].X < min1.X) min1.X = cornersFirst[i].X;
                if (cornersSecond[i].X > max2.X) max2.X = cornersSecond[i].X;
                if (cornersSecond[i].X < min2.X) min2.X = cornersSecond[i].X;
                if (cornersFirst[i].Y > max1.Y) max1.Y = cornersFirst[i].Y;
                if (cornersFirst[i].Y < min1.Y) min1.Y = cornersFirst[i].Y;
                if (cornersSecond[i].Y > max2.Y) max2.Y = cornersSecond[i].Y;
                if (cornersSecond[i].Y < min2.Y) min2.Y = cornersSecond[i].Y;
                if (cornersFirst[i].Z > max1.Z) max1.Z = cornersFirst[i].Z;
                if (cornersFirst[i].Z < min1.Z) min1.Z = cornersFirst[i].Z;
                if (cornersSecond[i].Z > max2.Z) max2.Z = cornersSecond[i].Z;
                if (cornersSecond[i].Z < min2.Z) min2.Z = cornersSecond[i].Z;
            }

            if(Math.Max(min1.X, min2.X) > Math.Min(max1.X, max2.X) &&
               Math.Max(min1.Y, min2.Y) > Math.Min(max1.Y, max2.Y) &&
               Math.Max(min1.Z, min2.Z) > Math.Min(max1.Z, max2.Z))
            {
                return 0;
            }

            // We need to check if there's a pair of faces from boxes that intersect.
            // To do that, we calculate intersecting part of planes from faces
            // and check whether that set of points is part of one of the faces
            Plane[] planes1 = new Plane[6]
            {
                FrontFace(), BackFace(),
                LeftFace(), RightFace(),
                UpFace(), DownFace()
            };
            Plane[] planes2 = new Plane[6]
            {
                other.FrontFace(), other.BackFace(),
                other.LeftFace(), other.RightFace(),
                other.UpFace(), other.DownFace()
            };
            int[] ind = new int[18]
            {
                1, 0, 2,
                5, 4, 6,
                1, 2, 5,
                0, 3, 4,
                0, 1, 4,
                2, 3, 6
            };

            for(int i = 0; i < 6; ++i)
            {
                for(int j = 0; j < 6; ++j)
                {
                    // Find common part of the planes.

                }
            }

            throw new NotImplementedException();
        }

        protected override int IsOverlappingCylinder(BoundingCylinder other)
        {
            throw new NotImplementedException();
        }

        protected override int IsOverlappingSphere(BoundingSphere other)
        {
            //Implemented in BoundingSphere.
            return other.IsOverlapping(this);
        }

        public BoundingBox() : this(null, Vector3.Zero, 0.5f*Vector3.One)
        {
        }

        public BoundingBox(Collider collider, Vector3 center, Vector3 halfSizes) : base(collider, center)
        {
            halfLengths = halfSizes;
        }
    }
}
