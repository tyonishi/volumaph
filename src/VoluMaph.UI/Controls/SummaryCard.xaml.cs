using System.Windows;
using System.Windows.Controls;

namespace VoluMaph.UI.Controls
{
    /// <summary>
    /// A user control that displays a summary card with title, value, and subtitle.
    /// </summary>
    public partial class SummaryCard : UserControl
    {
        /// <summary>
        /// Identifies the Title dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(SummaryCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(SummaryCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the Subtitle dependency property.
        /// </summary>
        public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
            nameof(Subtitle), typeof(string), typeof(SummaryCard), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the card title.
        /// </summary>
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the card value.
        /// </summary>
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Gets or sets the card subtitle.
        /// </summary>
        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryCard"/> class.
        /// </summary>
        public SummaryCard()
        {
            InitializeComponent();
        }
    }
}
