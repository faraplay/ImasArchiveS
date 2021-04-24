using System.Collections.Generic;

namespace Imas.UI
{
    [SerialisationDerivedType(10)]
    public class SpriteCollection : Control
    {
        [SerialiseProperty(100, IsCountOf = nameof(ChildSpriteGroups))]
        public int ChildSpriteGroupCount { get; set; }

        [SerialiseProperty(101)]
        [Listed(101)]
        public int DefaultSpriteIndex { get; set; }
        [SerialiseProperty(102)]
        [Listed(102)]
        public uint E2 { get; set; }

        [SerialiseProperty(103, CountProperty = nameof(ChildSpriteGroupCount))]
        public List<SpriteGroup> ChildSpriteGroups { get; set; }
    }
}
