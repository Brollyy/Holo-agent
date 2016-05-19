namespace Engine
{
    public static class Physics
    {
        private static float gravitationalAcceleration;
        public static float GravitationalAcceleration
        {
            get
            {
                return gravitationalAcceleration;
            }
        }
        public static void Initialize()
        {
            gravitationalAcceleration = 9.81f;
        }
    }
}
