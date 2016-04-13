﻿using System;
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
                corners[i] = Vector3.Transform(new Vector3(HalfLength * (i % 4 == 0 || i % 4 == 3 ? 1 : -1),
                                                           HalfHeight * (i / 2 == 0 || i / 2 == 2 ? 1 : -1),
                                                           HalfWidth * (i / 4 == 0 ? -1 : 1)) + Center, world);
            }
            return corners;
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
            Vector3 normal = Vector3.TransformNormal(Vector3.Forward, world);
            normal.Normalize();
            return new Plane(normal, Vector3.Dot(normal, Vector3.Transform(halfLengths.Z * Vector3.Forward, world)));
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
            Vector3 normal = Vector3.TransformNormal(Vector3.Backward, world);
            normal.Normalize();
            return new Plane(normal, Vector3.Dot(normal, Vector3.Transform(halfLengths.Z * Vector3.Backward, world)));
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
            Vector3 normal = Vector3.TransformNormal(Vector3.Left, world);
            normal.Normalize();
            return new Plane(normal, Vector3.Dot(normal, Vector3.Transform(halfLengths.X * Vector3.Left, world)));
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
            Vector3 normal = Vector3.TransformNormal(Vector3.Right, world);
            normal.Normalize();
            return new Plane(normal, Vector3.Dot(normal, Vector3.Transform(halfLengths.X * Vector3.Right, world)));
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
            Vector3 normal = Vector3.TransformNormal(Vector3.Up, world);
            normal.Normalize();
            return new Plane(normal, Vector3.Dot(normal, Vector3.Transform(halfLengths.Y * Vector3.Up, world)));
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
            Vector3 normal = Vector3.TransformNormal(Vector3.Down, world);
            normal.Normalize();
            return new Plane(normal, Vector3.Dot(normal, Vector3.Transform(halfLengths.Y * Vector3.Down, world)));
        }

        protected override int IsOverlappingBox(BoundingBox other)
        {
            Vector3[] cornersFirst = Corners();
            Vector3[] cornersSecond = other.Corners();
            // First check for trivial case.
            #region AXES_CAST_CHECK
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
            #endregion

            #region PLANES_VERTICES_DEFINITION
            // We need to check if there's a pair of faces from boxes that intersect.
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
                for(int j = 0; j < 6; ++j)
                {
                    Vector3 N = Vector3.Cross(planes1[i].Normal, planes2[j].Normal);
                    // Find common part of the planes.
                    if (N.LengthSquared() < 0.0001f)
                    {
                        // Planes are parallel.
                        if ((planes1[i].Normal - planes2[j].Normal).LengthSquared() < 0.0001f && Math.Abs(planes1[i].D - planes2[j].D) < 0.0001f || 
                            Math.Abs(planes1[i].D + planes2[j].D) < 0.0001f)
                        {
                            #region PARALLEL_PLANES_SAT
                            // Planes of both faces are the same.
                            // This is case of 2D intersection. Use SAT to detect intersection.
                            bool intersectSAT = true;
                            Vector3[] separatingAxes = new Vector3[4]
                            {
                                planes1[(i+2)%6].Normal, planes1[(i+4)%6].Normal,
                                planes2[(j+2)%6].Normal, planes2[(j+4)%6].Normal
                            };
                            for(int k = 0; k < 4; ++k)
                            {
                                float minProj1 = float.PositiveInfinity, minProj2 = float.PositiveInfinity;
                                float maxProj1 = float.NegativeInfinity, maxProj2 = float.NegativeInfinity;
                                for(int l = 0; l < 4; ++l)
                                {
                                    float proj1 = Vector3.Dot(separatingAxes[k], cornersFirst[ind[4 * i + l]]);
                                    float proj2 = Vector3.Dot(separatingAxes[k], cornersSecond[ind[4 * j + l]]);
                                    if (proj1 < minProj1) minProj1 = proj1;
                                    if (proj1 > maxProj1) maxProj1 = proj1;
                                    if (proj2 < minProj2) minProj2 = proj2;
                                    if (proj2 > maxProj2) maxProj2 = proj2;
                                }

                                if (Math.Max(minProj1, minProj2) <= Math.Min(maxProj1, maxProj2))
                                {
                                    intersectSAT = false;
                                    break;
                                }
                            }

                            if (intersectSAT) return 6 * i + j + 1;
                            #endregion
                        }
                        else
                        {
                            // Faces are on two different parallel planes, there's no intersection.
                            return 0;
                        }
                    }
                    else
                    {
                        // Find the line that comes from intersection of two planes.
                        // To get the point on the line, we add third plane with normal N and D = 0 and find the point of 3 planes intersection.
                        Vector3 linePoint = Vector3.Cross(planes2[j].D * planes1[i].Normal - planes1[i].D * planes2[j].Normal, N) / N.LengthSquared();

                        float[,] inter1 = new float[4,2];
                        float[,] inter2 = new float[4,2];
                        // We need to find intersections of the line with edges of the faces.
                        for(int k = 0; k < 4; ++k)
                        {
                            #region EDGE_LINE_INTERSECTION
                            // Edges of first face.
                            Vector3 A = cornersFirst[ind[4 * i + k]];
                            Vector3 AB = cornersFirst[ind[4 * i + (k + 1) % 4]] - A;
                            Vector3 u = Vector3.Cross(N, AB);
                            if(Math.Abs(Vector3.Dot(u, linePoint - A)) < 0.0001f)
                            {
                                // Lines cross at one point.
                                u = Vector3.Cross(N, u);
                                inter1[k, 0] = Vector3.Dot(u, linePoint - A) / Vector3.Dot(u, AB);
                                inter1[k, 1] = float.NaN;
                            }
                            else
                            {
                                // Lines are parallel.
                                if (Vector3.Cross(linePoint - A, N).LengthSquared() < 0.0001f) 
                                {
                                    // Lines are essentially the same, so we'll take the intersection as the whole edge of the face.
                                    inter1[k, 0] = 0.0f;
                                    inter1[k, 1] = 1.0f;
                                }
                                else
                                {
                                    // Lines do not intersect.
                                    inter1[k, 0] = float.NaN;
                                    inter1[k, 1] = float.NaN;
                                }
                            }

                            //Edges of second face.
                            A = cornersSecond[ind[4 * j + k]];
                            AB = cornersSecond[ind[4 * j + (k + 1) % 4]] - A;
                            u = Vector3.Cross(N, AB);
                            if (Math.Abs(Vector3.Dot(u, linePoint - A)) < 0.0001f)
                            {
                                // Lines cross at one point.
                                u = Vector3.Cross(N, u);
                                inter2[k, 0] = Vector3.Dot(u, linePoint - A) / Vector3.Dot(u, AB);
                                inter2[k, 1] = float.NaN;
                            }
                            else
                            {
                                // Lines are parallel.
                                if (Vector3.Cross(linePoint - A, N).LengthSquared() < 0.0001f)
                                {
                                    // Lines are essentially the same, so we'll take the intersection as the whole edge of the face.
                                    inter2[k, 0] = 0.0f;
                                    inter2[k, 1] = 1.0f;
                                }
                                else
                                {
                                    // Lines do not intersect.
                                    inter2[k, 0] = float.NaN;
                                    inter2[k, 1] = float.NaN;
                                }
                            }
                            #endregion
                        }

                        // Check if faces share any intersecting points.
                        for(int k = 0; k < 4; ++k)
                        {
                            for(int l = 0; l < 4; ++l)
                            {
                                if (float.IsNaN(inter1[k, 0]) || float.IsNaN(inter2[l, 0])) continue;

                                #region EDGE_ON_EDGE_INTERSECTION_CHECK
                                if (!float.IsNaN(inter1[k, 1]) && !float.IsNaN(inter2[l, 1])) 
                                {
                                    // Check between two ranges of points.
                                    // Since these ranges can only contain edges, this is basically checking if these two edges
                                    // overlap on the line.
                                    float A1 = Vector3.Dot(linePoint - cornersFirst[ind[4 * i + k]], N);
                                    float B1 = Vector3.Dot(linePoint - cornersFirst[ind[4 * i + (k + 1) % 4]], N);
                                    float A2 = Vector3.Dot(linePoint - cornersSecond[ind[4 * j + l]], N);
                                    float B2 = Vector3.Dot(linePoint - cornersSecond[ind[4 * j + (l + 1) % 4]], N);
                                    // Order them properly.
                                    if(A1 > B1)
                                    {
                                        float temp = A1;
                                        A1 = B1;
                                        B1 = temp;
                                    }
                                    if(A2 > B2)
                                    {
                                        float temp = A2;
                                        A2 = B2;
                                        B2 = temp;
                                    }

                                    if (Math.Max(A1, A2) < Math.Min(B1, B2)) return 6 * i + j + 1;
                                }
                                #endregion

                                #region POINT_ON_EDGE_INTERSECTION_CHECK
                                if(float.IsNaN(inter1[k,1]) && !float.IsNaN(inter2[l, 1]))
                                {
                                    // Check between one point and range of points.
                                    if (inter1[k, 0] < 0.0f || inter1[k, 0] > 1.0f) continue;

                                    float A2 = Vector3.Dot(linePoint - cornersSecond[ind[4 * j + l]], N);
                                    float B2 = Vector3.Dot(linePoint - cornersSecond[ind[4 * j + (l + 1) % 4]], N);
                                    Vector3 C = cornersFirst[ind[4 * i + k]] + inter1[k, 0] * (cornersFirst[ind[4 * i + (k+1)%4]] - cornersFirst[ind[4 * i + k]]);
                                    float C1 = Vector3.Dot(C, N);
                                    if (Math.Min(A2, B2) < C1 && Math.Max(A2, B2) > C1) return 6 * i + j + 1;
                                }
                                #endregion

                                #region EDGE_ON_POINT_INTERSECTION_CHECK
                                if (!float.IsNaN(inter1[k, 1]) && float.IsNaN(inter2[l, 1]))
                                {
                                    // Check between one point and range of points.
                                    if (inter2[l, 0] < 0.0f || inter2[l, 0] > 1.0f) continue;

                                    float A1 = Vector3.Dot(linePoint - cornersFirst[ind[4 * i + k]], N);
                                    float B1 = Vector3.Dot(linePoint - cornersFirst[ind[4 * i + (k + 1) % 4]], N);
                                    Vector3 C = cornersSecond[ind[4 * j + l]] + inter2[l, 0] * (cornersSecond[ind[4 * j + (l + 1) % 4]] - cornersSecond[ind[4 * j + l]]);
                                    float C1 = Vector3.Dot(C, N);
                                    if (Math.Min(A1, B1) < C1 && Math.Max(A1, B1) > C1) return 6 * i + j + 1;
                                }
                                #endregion

                                #region POINT_ON_POINT_INTERSECTION_CHECK
                                if (float.IsNaN(inter1[k, 1]) && float.IsNaN(inter2[l, 1]))
                                {
                                    // Check between two points.
                                    Vector3 C1 = cornersFirst[ind[4 * i + k]] + inter1[k, 0] * (cornersFirst[ind[4 * i + (k + 1) % 4]] - cornersFirst[ind[4 * i + k]]);
                                    Vector3 C2 = cornersSecond[ind[4 * j + l]] + inter2[l, 0] * (cornersSecond[ind[4 * j + (l + 1) % 4]] - cornersSecond[ind[4 * j + l]]);
                                    if ((C2 - C1).LengthSquared() < 0.0001f) return 6 * i + j + 1;
                                }
                                #endregion
                            }
                        }
                    }
                }
            }

            // No pair of faces intersect, but one box can still be within the second one.
            // When this happens, center of one box must be inside the other one.
            #region BOX_INSIDE_BOX
            Vector3 center1 = GlobalCenter(), center2 = other.GlobalCenter();
            bool inside1 = true, inside2 = true;
            for (int i = 0; i < 6; ++i)
            {
                if (planes1[i].D - Vector3.Dot(planes1[i].Normal, center2) < 0.0f)
                {
                    inside1 = false;
                    break;
                }

                if (planes2[i].D - Vector3.Dot(planes2[i].Normal, center1) < 0.0f)
                {
                    inside2 = false;
                    break;
                }
            }

            #endregion
            return (inside1 || inside2) ? 7 : 0;
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
