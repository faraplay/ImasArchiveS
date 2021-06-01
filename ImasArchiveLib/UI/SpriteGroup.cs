using System.Collections.Generic;
using System.Linq;

namespace Imas.UI
{
    public class SpriteGroup : UIElement
    {
        [SerialiseProperty(0)]
        public int SpriteCount
        {
            get => Sprites.Count;
            set
            {
                if (value == Sprites.Count)
                    return;
                if (value < Sprites.Count && value >= 0)
                {
                    Sprites.RemoveRange(value, Sprites.Count - value);
                    return;
                }
                if (value > Sprites.Count)
                {
                    Sprites.AddRange(Enumerable.Repeat<Sprite>(null, value - Sprites.Count));
                    return;
                }
            }
        }
        [SerialiseProperty(1)]
        public List<Sprite> Sprites { get; set; } = new List<Sprite>();
    }
}
