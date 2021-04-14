using System.Windows;
using System.Windows.Input;
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

        private Matrix matrix = new Matrix(1, 0, 0, 1, 0, 0);

        private static TileBrush _checkerBrush;
        private static TileBrush CheckerBrush
        {
            get
            {
                if (_checkerBrush == null)
                    CreateBrush();
                return _checkerBrush;
            }
        }

        private static void CreateBrush()
        {
            DrawingGroup drawingGroup = new DrawingGroup();
            drawingGroup.Children.Add(new GeometryDrawing(Brushes.LightGray, null, new RectangleGeometry(new Rect(0, 0, 2, 2))));
            drawingGroup.Children.Add(new GeometryDrawing(Brushes.DarkGray, null,
                Geometry.Combine(
                    new RectangleGeometry(new Rect(0, 0, 1, 1)),
                    new RectangleGeometry(new Rect(1, 1, 1, 1)),
                    GeometryCombineMode.Union,
                    null)
                ));
            _checkerBrush = new DrawingBrush(drawingGroup);
            _checkerBrush.ViewportUnits = BrushMappingMode.Absolute;
            _checkerBrush.TileMode = TileMode.Tile;
            CheckerBrush.Viewport = new Rect(new Point(0, 0), new Size(32, 32));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Rect appRect = new Rect(new Size(ActualWidth, ActualHeight));
            Rect gameScreenRect = new Rect(0, 0, 1280, 720);
            drawingContext.PushClip(new RectangleGeometry(appRect));
            drawingContext.DrawRectangle(Brushes.DimGray, null, appRect);
            drawingContext.PushTransform(new MatrixTransform(matrix));
            drawingContext.DrawRectangle(CheckerBrush, null, gameScreenRect);
            drawingContext.DrawRectangle(null, new Pen(Brushes.Yellow, 1), gameScreenRect);
            ElementModel?.RenderElement(drawingContext);
            drawingContext.Pop();
            drawingContext.Pop();
        }

        private int mouseDelta = 0;
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            mouseDelta += e.Delta;
            Point mousePoint = e.GetPosition(this);
            if (mouseDelta >= 120)
            {
                mouseDelta %= 120;
                matrix.ScaleAt(1.1, 1.1, mousePoint.X, mousePoint.Y);
            }
            else if (mouseDelta <= -120)
            {
                mouseDelta %= 120;
                matrix.ScaleAt(1/1.1, 1/1.1, mousePoint.X, mousePoint.Y);
            }
        }

        private Point mousePoint;
        private bool isDragging = false;
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            isDragging = true;
            mousePoint = e.GetPosition(this);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.LeftButton == MouseButtonState.Pressed && isDragging)
            {
                Point newMousePoint = e.GetPosition(this);
                matrix.Translate(newMousePoint.X - mousePoint.X, newMousePoint.Y - mousePoint.Y);
                mousePoint = newMousePoint;
                InvalidateVisual();
            }
            else
            {
                isDragging = false;
            }
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            isDragging = false;
        }
    }
}
