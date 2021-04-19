using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ImasArchiveApp
{
    public class UIAnimationGroupModel : FileModel
    {
        private AnimationGroup animationGroup;
        public ObservableCollection<UIControlAnimationsListModel> ListModels { get; }
        public UIAnimationGroupModel(IReport parent, AnimationGroup animationGroup) : base(parent, animationGroup.FileName)
        {
            this.animationGroup = animationGroup;
            ListModels = new ObservableCollection<UIControlAnimationsListModel>();
            foreach (ControlAnimationsList animationsList in animationGroup.controlAnimations)
            {
                ListModels.Add(new UIControlAnimationsListModel(parent, animationsList));
            }
        }
    }
}
