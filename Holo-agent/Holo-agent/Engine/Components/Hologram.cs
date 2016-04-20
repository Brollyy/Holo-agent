using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace Engine.Components
{
    public class HologramRecorder : Component
    {

    }

    public class HologramPlayback : Component
    {

    }

    public struct HologramPath
    {
        public List<Vector3> GlobalPositions;
        public List<Quaternion> GlobalRotations;
        public List<Tuple<float,float?,GameAction>> Actions;
    }
}
