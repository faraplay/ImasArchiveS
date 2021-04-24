using System.Collections.Generic;

namespace Imas.UI
{
    [SerialisationDerivedType(4)]
    public class GroupControl : Control
    {
        [SerialiseProperty(100, IsCountOf = nameof(ChildControls))]
        public int ChildCount { get; set; }
        [SerialiseProperty(101, CountProperty = nameof(ChildCount))]
        public List<Control> ChildControls { get; set; }
    }
}
