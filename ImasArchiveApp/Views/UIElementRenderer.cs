using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace ImasArchiveApp
{
    public class UIElementRenderer : FrameworkElement
    {

        public static readonly DependencyProperty ElementModelProperty =
            DependencyProperty.Register("ElementModel", typeof(UIElementModel), typeof(UIElementRenderer), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public UIElementModel ElementModel
        {
            get { return (UIElementModel)GetValue(ElementModelProperty); }
            set { SetValue(ElementModelProperty, value); }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            ElementModel?.RenderElement(drawingContext);
        }
    }
}
