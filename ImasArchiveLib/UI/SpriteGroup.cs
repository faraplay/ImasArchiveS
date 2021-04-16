using System.Collections.Generic;

namespace Imas.UI
{
    public class SpriteGroup : UIElement
    {
        [SerialiseField(0, IsCountOf = nameof(sprites))]
        public int spriteCount;
        [SerialiseField(1, CountField = nameof(spriteCount))]
        public List<Sprite> sprites = new List<Sprite>();
    }
}
