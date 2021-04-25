using System.Collections.Generic;
using System.Linq;

namespace Imas.UI
{
    [SerialisationDerivedType(4)]
    public class GroupControl : Control
    {
        [SerialiseProperty(100)]
        public int ChildCount
        {
            get => ChildControls.Count;
            set
            {
                if (value == ChildControls.Count)
                    return;
                if (value < ChildControls.Count && value >= 0)
                {
                    ChildControls.RemoveRange(value, ChildControls.Count - value);
                    return;
                }
                if (value > ChildControls.Count)
                {
                    ChildControls.AddRange(Enumerable.Repeat<Control>(null, value - ChildControls.Count));
                    return;
                }
            }
        }
        [SerialiseProperty(101)]
        public List<Control> ChildControls { get; set; } = new List<Control>();
    }
}
