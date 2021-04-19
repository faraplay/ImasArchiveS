using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media.Animation;

namespace ImasArchiveApp
{
    public class UIControlAnimationsListModel : FileModel
    {
        private ControlAnimationsList animationsList;
        public ObservableCollection<UIAnimationModel> Animations { get; }
        public ParallelTimeline Timeline { get; }
        public UIControlModel Control { get; }
        public UIControlAnimationsListModel(UISubcomponentModel parent, ControlAnimationsList animationsList) : base(parent, animationsList.ControlName)
        {
            this.animationsList = animationsList;
            Animations = new ObservableCollection<UIAnimationModel>();
            Timeline = new ParallelTimeline();
            Control = parent.ControlDictionary[animationsList.ControlName];
            foreach (Animation animation in animationsList.animations)
            {
                Animations.Add(new UIAnimationModel(parent, animation));
            }
            BuildTimeline();
        }

        private DoubleAnimationUsingKeyFrames _opacityTimeline;
        private DoubleAnimationUsingKeyFrames OpacityTimeline
        {
            get
            {
                if (_opacityTimeline == null)
                    _opacityTimeline = new DoubleAnimationUsingKeyFrames();
                return _opacityTimeline;
            }
        }
        private void BuildTimeline()
        {
            foreach (Animation animation in animationsList.animations)
            {
                switch (animation)
                {
                    case OpacityTransformation transparencyAnimation:
                        AddOpacityAnimation(transparencyAnimation);
                        break;
                }
            }

            Timeline.Children.Add(OpacityTimeline);
        }

        private void AddOpacityAnimation(OpacityTransformation animation)
        {
            OpacityTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.startTransparency,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.startTime)))
                );
            OpacityTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.endTransparency,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.endTime)))
                );
        }

        public void ApplyAnimations(ClockGroup clockGroup)
        {
            Clock opacityClock = clockGroup.Children[0];
            Control.OpacityClock = (AnimationClock)opacityClock;
        }

        public void RemoveAnimations()
        {
            Control.OpacityClock = null;
        }
    }
}
