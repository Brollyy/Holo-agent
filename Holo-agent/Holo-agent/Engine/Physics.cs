namespace Engine
{
    public static class Physics
    {
        private static float gravitationalAcceleration;
        private static float meterScale;
        public static float GravitationalAcceleration
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
            gravitationalAcceleration = -9.81f;
            meterScale = 10;
        }
    }
}
