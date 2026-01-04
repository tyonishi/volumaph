using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace VoluMaph.UI.Controls;

/// <summary>
/// A user control that displays a toast notification.
/// </summary>
public partial class ToastNotification : UserControl
{
    private readonly DispatcherTimer _autoCloseTimer;

    /// <summary>
    /// Identifies the Message dependency property.
    /// </summary>
    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(ToastNotification), new PropertyMetadata(string.Empty));

    /// <summary>
    /// Identifies the Icon dependency property.
    /// </summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(ToastNotification), new PropertyMetadata("\xE8FB"));

    /// <summary>
    /// Identifies the IconColor dependency property.
    /// </summary>
    public static readonly DependencyProperty IconColorProperty =
        DependencyProperty.Register(nameof(IconColor), typeof(Brush), typeof(ToastNotification), new PropertyMetadata(new SolidColorBrush(Colors.Green)));

    /// <summary>
    /// Gets or sets the toast message text.
    /// </summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Gets or sets the toast icon character.
    /// </summary>
    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the toast icon color.
    /// </summary>
    public Brush IconColor
    {
        get => (Brush)GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToastNotification"/> class.
    /// </summary>
    public ToastNotification()
    {
        InitializeComponent();

        _autoCloseTimer = new DispatcherTimer();
        _autoCloseTimer.Interval = TimeSpan.FromSeconds(3);
        _autoCloseTimer.Tick += AutoCloseTimer_Tick;
        _autoCloseTimer.Start();

        Loaded += (s, e) => { };
        Unloaded += (s, e) => _autoCloseTimer.Stop();
    }

    private void AutoCloseTimer_Tick(object? sender, EventArgs e)
    {
        _autoCloseTimer.Stop();
        Close_Click(sender, new RoutedEventArgs());
    }

    /// <summary>
    /// Handles the close button click event.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void Close_Click(object sender, RoutedEventArgs e)
    {
        _autoCloseTimer.Stop();

        var storyboard = new Storyboard();
        var translateAnimation = new DoubleAnimation
        {
            To = -100,
            Duration = TimeSpan.FromMilliseconds(300)
        };
        Storyboard.SetTarget(translateAnimation, this);
        Storyboard.SetTargetProperty(translateAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));

        storyboard.Children.Add(translateAnimation);
        storyboard.Completed += (s, a) =>
        {
            if (Parent is Panel panel)
            {
                panel.Children.Remove(this);
            }
        };
        storyboard.Begin();
    }
}
