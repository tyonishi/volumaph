using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VoluMaph.UI.Behaviors;

public sealed class ZoomPanBehavior
{
    internal const double MinZoom = 0.1;
    internal const double MaxZoom = 20.0;
    internal const double ZoomStep = 1.1;

    public static readonly DependencyProperty ZoomLevelProperty =
         DependencyProperty.RegisterAttached(
             "ZoomLevel",
             typeof(double),
             typeof(ZoomPanBehavior),
             new PropertyMetadata(1.0, OnZoomLevelChanged));

    public static double GetZoomLevel(DependencyObject obj) =>
        (double)obj.GetValue(ZoomLevelProperty);

    public static void SetZoomLevel(DependencyObject obj, double value) =>
        obj.SetValue(ZoomLevelProperty, value);

    public static readonly DependencyProperty PanXProperty =
        DependencyProperty.RegisterAttached(
            "PanX",
            typeof(double),
            typeof(ZoomPanBehavior),
            new PropertyMetadata(0.0, OnPanXChanged));

    public static double GetPanX(DependencyObject obj) =>
        (double)obj.GetValue(PanXProperty);

    public static void SetPanX(DependencyObject obj, double value) =>
        obj.SetValue(PanXProperty, value);

    public static readonly DependencyProperty PanYProperty =
        DependencyProperty.RegisterAttached(
            "PanY",
            typeof(double),
            typeof(ZoomPanBehavior),
            new PropertyMetadata(0.0, OnPanYChanged));

    public static double GetPanY(DependencyObject obj) =>
        (double)obj.GetValue(PanYProperty);

    public static void SetPanY(DependencyObject obj, double value) =>
        obj.SetValue(PanYProperty, value);

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(ZoomPanBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) =>
        (bool)obj.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject obj, bool value) =>
        obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty ZoomTargetProperty =
        DependencyProperty.RegisterAttached(
            "ZoomTarget",
            typeof(UIElement),
            typeof(ZoomPanBehavior),
            new PropertyMetadata(null));

    public static UIElement? GetZoomTarget(DependencyObject obj) =>
        (UIElement?)obj.GetValue(ZoomTargetProperty);

    public static void SetZoomTarget(DependencyObject obj, UIElement? value) =>
        obj.SetValue(ZoomTargetProperty, value);

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
            ScaleTransform = new ScaleTransform
            {
                ScaleX = 1.0,
                ScaleY = 1.0
            },
            TranslateTransform = new TranslateTransform(),
            IsDragging = false,
            LastMousePosition = default
        };

        var transformGroup = new TransformGroup();
        transformGroup.Children.Add(state.TranslateTransform);
        transformGroup.Children.Add(state.ScaleTransform);

        state.Transform = transformGroup;

        _states[element] = state;

        // Initialize transforms from attached properties if they were set via binding
        try
        {
            state.ScaleTransform.ScaleX = GetZoomLevel(element);
            state.ScaleTransform.ScaleY = GetZoomLevel(element);
            state.TranslateTransform.X = GetPanX(element);
            state.TranslateTransform.Y = GetPanY(element);
        }
        catch
        {
            // Ignore if values are not yet available
        }

        element.MouseWheel += OnMouseWheel;
        element.MouseLeftButtonDown += OnMouseLeftButtonDown;
        element.MouseLeftButtonUp += OnMouseLeftButtonUp;
        element.MouseMove += OnMouseMove;

        var zoomTarget = GetZoomTarget(element) ?? element;
        zoomTarget.RenderTransform = state.Transform;
        zoomTarget.RenderTransformOrigin = new Point(0.5, 0.5);
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

        var zoomTarget = GetZoomTarget(element) ?? element;
        zoomTarget.RenderTransform = null;
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

    internal static void ZoomAt(ZoomPanState state, double x, double y, double factor, UIElement element)
    {
        var zoomTarget = GetZoomTarget(element) ?? element;
        var transform = state.Transform;

        var currentScale = state.ScaleTransform.ScaleX;
        var newScale = currentScale * factor;

        newScale = Math.Clamp(newScale, MinZoom, MaxZoom);

        if (Math.Abs(newScale - currentScale) < 0.001)
            return;
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
        if (sender is not UIElement element ||
            !_states.TryGetValue(element, out var state))
            return;

        state.IsDragging = false;
        element.ReleaseMouseCapture();
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not UIElement element ||
            !_states.TryGetValue(element, out var state) ||
            !state.IsDragging)
            return;

        var currentPosition = e.GetPosition(element);
        var deltaX = currentPosition.X - state.LastMousePosition.X;
        var deltaY = currentPosition.Y - state.LastMousePosition.Y;

        state.TranslateTransform.X += deltaX;
        state.TranslateTransform.Y += deltaY;

        // Update attached properties so bindings (ViewModel) stay in sync
        SetPanX(element, state.TranslateTransform.X);
        SetPanY(element, state.TranslateTransform.Y);

        state.LastMousePosition = currentPosition;
        e.Handled = true;
    }

    private static void OnZoomLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if (!_states.TryGetValue(element, out var state))
            return;

        var newScale = (double)e.NewValue;
        newScale = Math.Clamp(newScale, MinZoom, MaxZoom);
        var currentScale = state.ScaleTransform.ScaleX;
        if (Math.Abs(newScale - currentScale) < 0.001)
            return;

        var animation = new DoubleAnimation
        {
            To = newScale,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        state.ScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        state.ScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }

    private static void OnPanXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if (!_states.TryGetValue(element, out var state))
            return;

        var newPan = (double)e.NewValue;
        state.TranslateTransform.X = newPan;
    }

    private static void OnPanYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if (!_states.TryGetValue(element, out var state))
            return;

        var newPan = (double)e.NewValue;
        state.TranslateTransform.Y = newPan;
    }

    internal static ZoomPanState CreateZoomState()
    {
        return new ZoomPanState();
    }

    internal sealed class ZoomPanState
    {
        public TransformGroup Transform { get; set; } = null!;
        public TranslateTransform TranslateTransform { get; set; } = null!;
        public ScaleTransform ScaleTransform { get; set; } = null!;
        public bool IsDragging { get; set; }
        public Point LastMousePosition { get; set; }
    }
}
