using System.Collections.Generic;
using System.Diagnostics;

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

        [SerialiseProperty(6, IsCountOf = nameof(ControlAnimations))]
        public int ControlAnimationCount { get; set; }
        [SerialiseProperty(7, CountProperty = nameof(ControlAnimationCount))]
        public List<ControlAnimationsList> ControlAnimations { get; set; }

        private string GetDebuggerDisplay()
        {
            return FileName;
        }
    }
}
