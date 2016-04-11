using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.Bounding_Volumes
{
    public class BoundingCylinder : BoundingVolume
    {
        protected override int IsOverlappingBox(BoundingBox other)
        {
            throw new NotImplementedException();
        }

        protected override int IsOverlappingCylinder(BoundingCylinder other)
        {
            throw new NotImplementedException();
        }

        protected override int IsOverlappingSphere(BoundingSphere other)
        {
            throw new NotImplementedException();
        }
    }
}
