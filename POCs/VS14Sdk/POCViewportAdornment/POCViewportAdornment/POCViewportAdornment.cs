using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;
using System.Windows;
using Microsoft.VisualStudio.Text;
using System.Linq;
using System.Windows.Input;

namespace POCViewportAdornment
{
    /// <summary>
    /// Adornment class that draws a square box in the top right hand corner of the viewport
    /// </summary>
    class POCViewportAdornment
    {
        private Image _image;
        private IWpfTextView _view;
        private IAdornmentLayer _adornmentLayer;
        private StackPanel _adornPanel;
        private string _text = null;

        /// <summary>
        /// Creates a square image and attaches an event handler to the layout changed event that
        /// adds the the square in the upper right-hand corner of the TextView via the adornment layer
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        public POCViewportAdornment(IWpfTextView view)
        {
            _view = view;

            Brush brush = new SolidColorBrush(Colors.BlueViolet);
            brush.Freeze();
            Brush penBrush = new SolidColorBrush(Colors.Red);
            penBrush.Freeze();
            Pen pen = new Pen(penBrush, 0.5);
            pen.Freeze();

            //draw a square with the created brush and pen
            System.Windows.Rect r = new System.Windows.Rect(0, 0, 30, 30);
            Geometry g = new RectangleGeometry(r);
            GeometryDrawing drawing = new GeometryDrawing(brush, pen, g);
            drawing.Freeze();

            DrawingImage drawingImage = new DrawingImage(drawing);
            drawingImage.Freeze();

            _image = new Image();
            _image.Source = drawingImage;

            _adornPanel = new StackPanel()
            {
                Cursor = Cursors.Arrow,
            };

            _adornPanel.Children.Add(_image);

            var myButton = new Button()
            {
                Content = "Just a button!!!",
                Cursor = Cursors.Hand,
            };

            myButton.Click += (sender, e) =>
            {
                Window _win = new Window();

                _win.Content = new Label()
                {
                    Content = string.Format("{0}", _text),
                };

                _win.SizeToContent = SizeToContent.WidthAndHeight;

                _win.ShowDialog();
            };

            _adornPanel.Children.Add(myButton);

            //Grab a reference to the adornment layer that this adornment should be added to
            _adornmentLayer = view.GetAdornmentLayer("POCViewportAdornment");

            _view.Selection.SelectionChanged += Selection_SelectionChanged;
        }

        private void Selection_SelectionChanged(object sender, System.EventArgs e)
        {
            //clear the adornment layer of previous adornments
            _adornmentLayer.RemoveAllAdornments();

            var _textViewLine = _view.GetTextViewLineContainingBufferPosition(_view.Selection.End.Position);

            double _left = _textViewLine.GetCharacterBounds(_view.Selection.End.Position).Left;
            double _top = _textViewLine.GetCharacterBounds(_view.Selection.End.Position).Top;

            _text = _view.Selection.StreamSelectionSpan.SnapshotSpan.GetText();

            //Place the image in the top right hand corner of the Viewport
            Canvas.SetLeft(_adornPanel, _left);
            Canvas.SetTop(_adornPanel, _top);

            //add the image to the adornment layer and make it relative to the viewport
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.TextRelative, _view.Selection.StreamSelectionSpan.SnapshotSpan, null, _adornPanel, null);
        }
    }
}
