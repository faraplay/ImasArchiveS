using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public class UIElementRenderer : FrameworkElement
    {

        public static readonly DependencyProperty SpriteModelProperty =
            DependencyProperty.Register("SpriteModel", typeof(UISpriteModel), typeof(UIElementRenderer), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public UISpriteModel SpriteModel
        {
            get { return (UISpriteModel)GetValue(SpriteModelProperty); }
            set { SetValue(SpriteModelProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            SpriteModel?.RenderSprite(drawingContext);
        }
    }
}
