namespace Imas.UI
{
    [SerialisationDerivedType(1)]
    public class VisibilityAnimation : Animation
    {
        [SerialiseField(100)]
        public float time;

        [SerialiseField(101)]
        public int visibility;
    }
}
