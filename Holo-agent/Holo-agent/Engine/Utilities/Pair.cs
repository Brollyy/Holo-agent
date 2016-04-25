namespace Engine.Utilities
{
    public class Pair<T1, T2>
    {
        public T1 First { get; set; }
        public T2 Second { get; set; }

        public Pair(T1 item1, T2 item2)
        {
            First = item1;
            Second = item2;
        }
    }
}
