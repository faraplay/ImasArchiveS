using System.Collections.Generic;

namespace Imas.UI
{
    [SerialisationDerivedType(4)]
    public class GroupControl : Control
    {
        [SerialiseField(100, IsCountOf = nameof(childControls))]
        public int childCount;
        [SerialiseField(101, CountField = nameof(childCount))]
        public List<Control> childControls;
    }
}
