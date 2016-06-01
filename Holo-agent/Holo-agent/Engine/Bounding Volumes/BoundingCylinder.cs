using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Bounding_Volumes
{
    public class BoundingCylinder : BoundingVolume
    {
        protected override CollisionResult IsOverlappingBox(BoundingBox other)
        {
            throw new NotImplementedException();
        }

        protected override CollisionResult IsOverlappingCylinder(BoundingCylinder other)
        {
            throw new NotImplementedException();
        }

        protected override CollisionResult IsOverlappingSphere(BoundingSphere other)
        {
            throw new NotImplementedException();
        }
    }
}
