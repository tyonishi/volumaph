using System;
using System.Windows;
using System.Windows.Controls;
using VoluMaph.Core.Model;

namespace VoluMaph.UI.Controls
{
    public partial class FolderTreeControl : UserControl
    {
        public FolderTreeControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            "SelectedItem", typeof(FileSystemNode), typeof(FolderTreeControl), new PropertyMetadata(null));

        public FileSystemNode? SelectedItem
        {
            get => (FileSystemNode?)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public event Action<FileSystemNode?>? SelectedItemChanged;

        private void InnerTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileSystemNode node)
            {
                SelectedItem = node;
                SelectedItemChanged?.Invoke(node);
            }
            else
            {
                SelectedItem = null;
                SelectedItemChanged?.Invoke(null);
            }
        }
    }
}
