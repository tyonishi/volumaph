using System;
using System.ComponentModel;

namespace VoluMaph.UI.ViewModels;

public sealed class VisualizationBookmark : INotifyPropertyChanged
{
    private string _name;
    private VisualizationState _state;

    public VisualizationBookmark(string name, VisualizationState state)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Bookmark name cannot be null or whitespace.", nameof(name));
        }

        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        _name = name;
        _state = state;
        CreatedAt = DateTime.Now;
    }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Bookmark name cannot be null or whitespace.", nameof(value));
            }

            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public VisualizationState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }
    }

    public DateTime CreatedAt { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
