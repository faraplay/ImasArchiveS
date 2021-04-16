using System.Collections.Generic;

namespace Imas.UI
{
    [SerialisationDerivedType(10)]
    public class SpriteCollection : Control
    {
        [SerialiseField(100, IsCountOf = nameof(childSpriteGroups))]
        public int childSpriteGroupCount;

        [SerialiseField(101)]
        public int e1;
        [Listed(101)]
        public int E1 { get => e1; set => e1 = value; }
        [SerialiseField(102)]
        public int e2;
        [Listed(102)]
        public int E2 { get => e2; set => e2 = value; }

        [SerialiseField(103, CountField = nameof(childSpriteGroupCount))]
        public List<SpriteGroup> childSpriteGroups;
    }
}
