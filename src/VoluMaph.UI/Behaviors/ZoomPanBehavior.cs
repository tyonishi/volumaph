using System.Windows;
using System.Windows.Input;

namespace VoluMaph.UI.Behaviors;

public sealed class ZoomPanBehavior
{
    private const double MinZoom = 0.1;
    private const double MaxZoom = 20.0;
    private const double ZoomStep = 1.1;

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(ZoomPanBehavior),
            typeof(UIElement),
            typeof(bool),
            new PropertyMetadata(false, new PropertyChangedCallback(OnIsEnabledChanged)));

    public static bool GetIsEnabled(DependencyObject obj) =>
        (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) =>
        obj.SetValue(IsEnabledProperty, value);

    private static readonly DependencyProperty ZoomTargetProperty =
        DependencyProperty.RegisterAttached(
            "ZoomTarget",
            typeof(ZoomPanBehavior),
            typeof(UIElement),
            typeof(UIElement),
            null,
            new PropertyMetadata(null));

    public static UIElement? GetZoomTarget(DependencyObject obj) =>
        (UIElement?)obj.GetValue(ZoomTargetProperty);

    public static void SetZoomTarget(DependencyObject obj, UIElement? value) =>
        obj.SetValue(ZoomTargetProperty, value);

    private static readonly DependencyProperty ScaleTransformProperty =
        DependencyProperty.RegisterAttached(
            "ScaleTransform",
            typeof(ZoomPanBehavior),
            typeof(UIElement),
            typeof(ScaleTransform),
            null,
            new PropertyMetadata(null));

    private static ScaleTransform GetScaleTransform(DependencyObject obj) =>
        (ScaleTransform)obj.GetValue(ScaleTransformProperty);

    private static void SetScaleTransform(DependencyObject obj, ScaleTransform value) =>
        obj.SetValue(ScaleTransformProperty, value);

    private static readonly DependencyProperty TranslateTransformProperty =
        DependencyProperty.RegisterAttached(
            "TranslateTransform",
            typeof(ZoomPanBehavior),
            typeof(UIElement),
            typeof(TranslateTransform),
            null,
            new PropertyMetadata(null));

    private static TranslateTransform GetTranslateTransform(DependencyObject obj) =>
        (TranslateTransform)obj.GetValue(TranslateTransformProperty);

    private static void SetTranslateTransform(DependencyObject obj, TranslateTransform value) =>
        obj.SetValue(TranslateTransformProperty, value);

    private static readonly Dictionary<UIElement, ZoomPanState> _states = new();

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
        {
            Attach(element);
        }
        else
        {
            Detach(element);
        }
    }

    private static void Attach(UIElement element)
    {
        var state = new ZoomPanState
        {
            TransformGroup = new TransformGroup(),
            IsDragging = false,
            LastMousePosition = default
        };

        _states[element] = state;

        element.MouseWheel += OnMouseWheel;
        element.MouseLeftButtonDown += OnMouseLeftButtonDown;
        element.MouseLeftButtonUp += OnMouseLeftButtonUp;
        element.MouseMove += OnMouseMove;
        element.MouseRightButtonDown += OnMouseRightButtonDown;
        element.MouseRightButtonUp += OnMouseRightButtonUp;
        element.PreviewMouseWheel += OnPreviewMouseWheel;

        var zoomTarget = GetZoomTarget(element) ?? element;

        state.Transform = new TransformGroup();
        state.TranslateTransform = new TranslateTransform();
        state.ScaleTransform = new ScaleTransform
        {
            ScaleX = 1.0,
            ScaleY = 1.0,
            ScaleY = 1.0
        };

        zoomTarget.RenderTransform = state.Transform;
        zoomTarget.RenderTransformOrigin = new Point(0.5, 0.5);

        RenderOptions.SetBitmapCachingScope(zoomTarget, BitmapCachingScope.Inherit);
    }

    private static void Detach(UIElement element)
    {
        if (!_states.TryGetValue(element, out var state))
            return;

        _states.Remove(element);

        element.MouseWheel -= OnMouseWheel;
        element.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        element.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        element.MouseMove -= OnMouseMove;
        element.MouseRightButtonDown -= OnMouseRightButtonDown;
        element.MouseRightButtonUp -= OnMouseRightButtonUp;
        element.PreviewMouseWheel -= OnPreviewMouseWheel;

        var zoomTarget = GetZoomTarget(element) ?? element;
        zoomTarget.RenderTransform = null;
    }

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not UIElement element ||
            !_states.TryGetValue(element, out var state))
            return;

        if (!state.IsDragging)
            return;

        e.Handled = true;

        var position = e.GetPosition(element);
        var delta = e.Delta > 0 ? ZoomStep : 1.0 / ZoomStep;

        ZoomAt(state, position.X, position.Y, delta, element);
    }

    private static void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not UIElement element ||
            !_states.TryGetValue(element, out var state))
            return;

        if (state.IsDragging)
            return;

        e.Handled = true;

        var position = e.GetPosition(element);
        var delta = e.Delta > 0 ? ZoomStep : 1.0 / ZoomStep;

        ZoomAt(state, position.X, position.Y, delta, element);
    }

    private static void ZoomAt(ZoomPanState state, double x, double y, double factor, UIElement element)
    {
        var zoomTarget = GetZoomTarget(element) ?? element;
        var transform = state.Transform;

        var currentScale = transform.ScaleX;
        var newScale = currentScale * factor;

        newScale = Math.Clamp(newScale, MinZoom, MaxZoom);

        if (Math.Abs(newScale - currentScale) < 0.001)
            return;

        var zoomCenter = new Point(x, y);
        var transformToElement = element.TransformToVisual(zoomTarget).Transform;

        var centeredTransform = new MatrixTransform();
        centeredTransform.SetOriginToZoom(zoomCenter, transformToElement.Value, newScale);

        var animation = new DoubleAnimation
        {
            To = newScale,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        centeredTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        centeredTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);

        animation.Completed += (s, e) =>
        {
            state.ScaleTransform.ScaleX = newScale;
            state.ScaleTransform.ScaleY = newScale;
            centeredTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            centeredTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        };
    }

    private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement element ||
            !_states.TryGetValue(element, out var state))
            return;

        if (e.ClickCount == 1)
        {
            state.IsDragging = true;
            state.LastMousePosition = e.GetPosition(element);
            element.CaptureMouse();
            e.Handled = true;
        }
    }

    private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement ||
            !_states.TryGetValue(element, out var state))
            return;

        state.IsDragging = false;
        element.ReleaseMouseCapture();
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not UIElement ||
            !_states.TryGetValue(element, out var state) ||
            !state.IsDragging)
            return;

        var currentPosition = e.GetPosition(element);
        var deltaX = currentPosition.X - state.LastMousePosition.X;
        var deltaY = currentPosition.Y - state.LastMousePosition.Y;

        state.TranslateTransform.X += deltaX;
        state.TranslateTransform.Y += deltaY;

        state.LastMousePosition = currentPosition;
        e.Handled = true;
    }

    private static void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement element)
            return;

        element.CaptureMouse();
        e.Handled = true;
    }

    private static void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement element)
            return;

        element.ReleaseMouseCapture();
    }

    private static void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not UIElement)
            return;

        element.ReleaseMouseCapture();
    }

    private sealed class ZoomPanState
    {
        public TransformGroup Transform { get; set; }
        public TranslateTransform TranslateTransform { get; set; }
        public ScaleTransform ScaleTransform { get; set; }
        public bool IsDragging { get; set; }
        public Point LastMousePosition { get; set; }
    }
}
