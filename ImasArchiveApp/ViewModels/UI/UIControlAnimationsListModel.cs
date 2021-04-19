using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ImasArchiveApp
{
    public class UIControlAnimationsListModel : FileModel
    {
        private ControlAnimationsList animationsList;
        public ObservableCollection<UIAnimationModel> Animations { get; }
        public UIControlAnimationsListModel(IReport parent, ControlAnimationsList animationsList) : base(parent, animationsList.ControlName)
        {
            this.animationsList = animationsList;
            Animations = new ObservableCollection<UIAnimationModel>();
            foreach (Animation animation in animationsList.animations)
            {
                Animations.Add(new UIAnimationModel(parent, animation));
            }
        }
    }
}
