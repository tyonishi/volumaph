using System.Windows.Controls;

namespace VoluMaph.UI.Controls
{
    public partial class CommandBarControl : UserControl
    {
        public CommandBarControl()
        {
            InitializeComponent();
        }

        public void FocusSearch()
        {
            SearchTextBox?.Focus();
        }
    }
}
