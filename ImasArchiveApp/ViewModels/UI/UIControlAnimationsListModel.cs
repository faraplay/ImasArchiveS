using Imas.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
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

        private void BuildTimeline()
        {
            foreach (Animation animation in animationsList.animations)
            {
                switch (animation)
                {
                    case VisibilityAnimation visibilityAnimation:
                        AddVisibilityAnimation(visibilityAnimation);
                        break;
                    case PositionAnimation positionAnimation:
                        AddPositionAnimation(positionAnimation);
                        break;
                    case OpacityAnimation opacityTransformation:
                        AddOpacityAnimation(opacityTransformation);
                        break;
                    case ScaleAnimation scaleAnimation:
                        AddScaleAnimation(scaleAnimation);
                        break;
                }
            }

            if (VisibilityTimeline.KeyFrames.Count > 0)
            {
                var firstFrame = VisibilityTimeline.KeyFrames[0];
                if (firstFrame.KeyTime.TimeSpan.TotalSeconds > 1 / 120.0f)
                {
                    VisibilityTimeline.KeyFrames.Add(
                        new DiscreteDoubleKeyFrame(
                            1 - firstFrame.Value,
                            KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)))
                        );
                }
            }

            Timeline.Children.Add(VisibilityTimeline);
            Timeline.Children.Add(PositionXTimeline);
            Timeline.Children.Add(PositionYTimeline);
            Timeline.Children.Add(OpacityTimeline);
            Timeline.Children.Add(ScaleXTimeline);
            Timeline.Children.Add(ScaleYTimeline);
        }

        private readonly DoubleAnimationUsingKeyFrames VisibilityTimeline = new DoubleAnimationUsingKeyFrames();
        private readonly DoubleAnimationUsingKeyFrames PositionXTimeline = new DoubleAnimationUsingKeyFrames();
        private readonly DoubleAnimationUsingKeyFrames PositionYTimeline = new DoubleAnimationUsingKeyFrames();
        private readonly DoubleAnimationUsingKeyFrames OpacityTimeline = new DoubleAnimationUsingKeyFrames();
        private readonly DoubleAnimationUsingKeyFrames ScaleXTimeline = new DoubleAnimationUsingKeyFrames();
        private readonly DoubleAnimationUsingKeyFrames ScaleYTimeline = new DoubleAnimationUsingKeyFrames();

        private void AddVisibilityAnimation(VisibilityAnimation animation)
        {
            VisibilityTimeline.KeyFrames.Add(
                new DiscreteDoubleKeyFrame(
                    animation.visibility == 3 ? 1 : 0,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.time)))
                );
        }
        private void AddPositionAnimation(PositionAnimation animation)
        {
            for (int i = 0; i < animation.pointCount; i++)
            {
                PositionXTimeline.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        animation.points[i].x,
                        i == 0 ? KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.startTime)) :
                        i == animation.pointCount - 1 ? KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.endTime)) :
                        KeyTime.Uniform));
                PositionYTimeline.KeyFrames.Add(
                    new LinearDoubleKeyFrame(
                        animation.points[i].y,
                        i == 0 ? KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.startTime)) :
                        i == animation.pointCount - 1 ? KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.endTime)) :
                        KeyTime.Uniform));
            }
        }
        private void AddOpacityAnimation(OpacityAnimation animation)
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
        private void AddScaleAnimation(ScaleAnimation animation)
        {
            ScaleXTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.startXScale,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.startTime)))
                );
            ScaleYTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.startYScale,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.startTime)))
                );
            ScaleXTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.endXScale,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.endTime)))
                );
            ScaleYTimeline.KeyFrames.Add(
                new LinearDoubleKeyFrame(
                    animation.endYScale,
                    KeyTime.FromTimeSpan(TimeSpan.FromSeconds(animation.endTime)))
                );
        }

        public void ApplyAnimations(ClockGroup clockGroup)
        {
            Control.VisibilityClock = (AnimationClock)clockGroup.Children[0];
            Control.PositionTransform.ApplyAnimationClock(TranslateTransform.XProperty, (AnimationClock)clockGroup.Children[1]);
            Control.PositionTransform.ApplyAnimationClock(TranslateTransform.YProperty, (AnimationClock)clockGroup.Children[2]);
            Control.OpacityClock = (AnimationClock)clockGroup.Children[3];
            Control.ScaleTransform.ApplyAnimationClock(ScaleTransform.ScaleXProperty, (AnimationClock)clockGroup.Children[4]);
            Control.ScaleTransform.ApplyAnimationClock(ScaleTransform.ScaleYProperty, (AnimationClock)clockGroup.Children[5]);
        }

        public void RemoveAnimations()
        {
            Control.VisibilityClock = null;
            Control.PositionTransform.ApplyAnimationClock(TranslateTransform.XProperty, null);
            Control.PositionTransform.ApplyAnimationClock(TranslateTransform.YProperty, null);
            Control.OpacityClock = null;
            Control.ScaleTransform.ApplyAnimationClock(ScaleTransform.ScaleXProperty, null);
            Control.ScaleTransform.ApplyAnimationClock(ScaleTransform.ScaleYProperty, null);
        }
    }
}
