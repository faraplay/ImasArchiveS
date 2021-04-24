using System.Collections.Generic;

namespace Imas.UI
{
    public class SpriteGroup : UIElement
    {
        [SerialiseProperty(0, IsCountOf = nameof(Sprites))]
        public int SpriteCount { get; set; }
        [SerialiseProperty(1, CountProperty = nameof(SpriteCount))]
        public List<Sprite> Sprites { get; set; } = new List<Sprite>();
    }
}
