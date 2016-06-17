using Engine.Bounding_Volumes;
using Engine.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Components
{
    public class Collider : Component
    {
        public BoundingVolume bound;
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

                VertexPosition[] vertices = new VertexPosition[8];
                for (int j = 0; j < 8; ++j)
                {
                    vertices[j].Position = corners[j];
                }

                graphics.GraphicsDevice.DrawUserIndexedPrimitives<VertexPosition>(PrimitiveType.LineList, vertices, 0, 8, indexes, 0, 12);
            }
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
