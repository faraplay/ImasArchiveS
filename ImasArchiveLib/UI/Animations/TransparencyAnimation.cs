namespace Imas.UI
{
    [SerialisationDerivedType(5)]
    public class TransparencyAnimation : Animation
    {
        [SerialiseField(100)]
        public float startTime;
        [SerialiseField(101)]
        public float endTime;

        [SerialiseField(102)]
        public int a1;
        [SerialiseField(103)]
        public int a2;

        [SerialiseField(104)]
        public float startTransparency;
        [SerialiseField(105)]
        public float endTransparency;
    }
}
