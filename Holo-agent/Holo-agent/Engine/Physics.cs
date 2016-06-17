using Microsoft.Xna.Framework;

namespace Engine
{
    public static class Physics
    {
        private static Vector3 gravitationalAcceleration;
        private static float meterScale;
        public static Vector3 GravitationalAcceleration
        {
            get
            {
                return gravitationalAcceleration;
            }
        }
        public static float MeterScale
        {
            get
            {
                return meterScale;
            }
        }
        public static void Initialize()
        {
            gravitationalAcceleration = new Vector3(0,-98.1f,0);
            meterScale = 10;
        }
    }
}
