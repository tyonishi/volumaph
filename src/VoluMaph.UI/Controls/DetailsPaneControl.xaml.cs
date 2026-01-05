using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace VoluMaph.UI.Controls
{
    public partial class DetailsPaneControl : UserControl
    {
        public DetailsPaneControl()
        {
            InitializeComponent();
        }

        public DataGrid MainDataGrid => InnerDataGrid;

        public event Action<string, bool>? ColumnVisibilityChanged;
        public event EventHandler<DataGridSortingEventArgs>? SortingRequested;

        private void InnerDataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            SortingRequested?.Invoke(this, e);
        }

    }
}
