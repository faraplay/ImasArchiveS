using System.Collections.Generic;

namespace Imas.UI
{
    [SerialisationDerivedType(10)]
    public class SpriteCollection : Control
    {
        [SerialiseField(100, IsCountOf = nameof(childSpriteGroups))]
        public int childSpriteGroupCount;

        [SerialiseField(101)]
        public int defaultSpriteIndex;
        [Listed(101)]
        public int DefaultSpriteIndex { get => defaultSpriteIndex; set => defaultSpriteIndex = value; }
        [SerialiseField(102)]
        public uint e2;
        [Listed(102)]
        public uint E2 { get => e2; set => e2 = value; }

        [SerialiseField(103, CountField = nameof(childSpriteGroupCount))]
        public List<SpriteGroup> childSpriteGroups;
    }
}
