namespace Imas.UI
{
    [SerialisationDerivedType(9)]
    public class Control9 : GroupControl
    {
        [SerialiseField(200)]
        public int e1;
        [Listed(200)]
        public int E1 { get => e1; set => e1 = value; }
    }
}
