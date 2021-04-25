using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Imas.UI
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class AnimationGroup
    {
        public string FileName { get; set; }

        [SerialiseProperty(0)]
		[Listed(0)]
        public uint Header { get; set; }

        [SerialiseProperty(1)]
		[Listed(1)]
        public int A1 { get; set; }
        [SerialiseProperty(2)]
		[Listed(2)]
        public int A2 { get; set; }
        [SerialiseProperty(3)]
		[Listed(3)]
        public int A3 { get; set; }
        [SerialiseProperty(4)]
		[Listed(4)]
        public int A4 { get; set; }
        [SerialiseProperty(5)]
		[Listed(5)]
        public int A5 { get; set; }

        [SerialiseProperty(6)]
        public int ControlAnimationCount
        {
            get => ControlAnimations.Count;
            set
            {
                if (value == ControlAnimations.Count)
                    return;
                if (value < ControlAnimations.Count && value >= 0)
                {
                    ControlAnimations.RemoveRange(value, ControlAnimations.Count - value);
                    return;
                }
                if (value > ControlAnimations.Count)
                {
                    ControlAnimations.AddRange(Enumerable.Repeat<ControlAnimationsList>(null, value - ControlAnimations.Count));
                    return;
                }
            }
        }
        [SerialiseProperty(7)]
        public List<ControlAnimationsList> ControlAnimations { get; set; } = new List<ControlAnimationsList>();

        private string GetDebuggerDisplay()
        {
            return FileName;
        }
    }
}
