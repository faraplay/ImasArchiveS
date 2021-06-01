using System.Collections.Generic;
using System.Linq;

namespace Imas.UI
{
    [SerialisationDerivedType(10)]
    public class SpriteCollection : Control
    {
        [SerialiseProperty(100)]
        public int ChildSpriteGroupCount
        {
            get => ChildSpriteGroups.Count;
            set
            {
                if (value == ChildSpriteGroups.Count)
                    return;
                if (value < ChildSpriteGroups.Count && value >= 0)
                {
                    ChildSpriteGroups.RemoveRange(value, ChildSpriteGroups.Count - value);
                    return;
                }
                if (value > ChildSpriteGroups.Count)
                {
                    ChildSpriteGroups.AddRange(Enumerable.Repeat<SpriteGroup>(null, value - ChildSpriteGroups.Count));
                    return;
                }
            }
        }

        [SerialiseProperty(101)]
        [Listed(101)]
        public int DefaultSpriteIndex { get; set; } = 0;
        [SerialiseProperty(102)]
        [Listed(102)]
        public uint E2 { get; set; } = 0xD9000000;

        [SerialiseProperty(103)]
        public List<SpriteGroup> ChildSpriteGroups { get; set; } = new List<SpriteGroup>() { new SpriteGroup() };
    }
}
