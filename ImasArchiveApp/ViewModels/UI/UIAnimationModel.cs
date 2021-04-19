using Imas.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImasArchiveApp
{
    public class UIAnimationModel : FileModel
    {
        private Animation animation;
        public UIAnimationModel(IReport parent, Animation animation) : base(parent, animation.ToString())
        {
            this.animation = animation;
        }
    }
}
