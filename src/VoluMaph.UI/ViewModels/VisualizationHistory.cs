using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace VoluMaph.UI.ViewModels;

public sealed class VisualizationHistory : INotifyPropertyChanged
{
    private const int DefaultMaxHistorySize = 50;
    private readonly List<VisualizationState> _history = new();
    private int _currentIndex = -1;
    private int _maxHistorySize = DefaultMaxHistorySize;

    public VisualizationHistory()
    {
    }

    public VisualizationHistory(int maxHistorySize)
    {
        if (maxHistorySize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxHistorySize), "Max history size must be at least 1");
        }

        _maxHistorySize = maxHistorySize;
    }

    public int MaxHistorySize
    {
        get => _maxHistorySize;
        set
        {
            if (value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Max history size must be at least 1");
            }

            if (_maxHistorySize != value)
            {
                _maxHistorySize = value;
                OnPropertyChanged();
                TrimHistory();
            }
        }
    }

    public int Count => _history.Count;

    public int CurrentIndex
    {
        get => _currentIndex;
        private set
        {
            if (_currentIndex != value)
            {
                _currentIndex = value;
                OnPropertyChanged();
                UpdateUndoRedoState();
            }
        }
    }

    public bool CanUndo => CurrentIndex >= 0;

    public bool CanRedo => CurrentIndex < Count - 1;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void AddState(VisualizationState state)
    {
        if (Count > 0 && CurrentIndex >= 0 && _history[CurrentIndex].Equals(state))
        {
            return;
        }

        if (CurrentIndex < Count - 1)
        {
            var oldCount = Count;
            _history.RemoveRange(CurrentIndex + 1, Count - CurrentIndex - 1);
            if (Count != oldCount)
            {
                OnPropertyChanged(nameof(Count));
            }
        }

        _history.Add(state);
        OnPropertyChanged(nameof(Count));
        CurrentIndex++;

        TrimHistory();
    }

    public VisualizationState? GetCurrentState()
    {
        if (CurrentIndex < 0 || CurrentIndex >= Count)
        {
            return null;
        }

        return _history[CurrentIndex];
    }

    public VisualizationState Undo()
    {
        if (CurrentIndex < 0)
        {
            throw new InvalidOperationException("Cannot undo: no history available");
        }

        CurrentIndex--;
        return GetCurrentState()!;
    }

    public VisualizationState Redo()
    {
        if (!CanRedo)
        {
            throw new InvalidOperationException("Cannot redo: no future state available");
        }

        CurrentIndex++;
        return GetCurrentState()!;
    }

    public void Clear()
    {
        var previousCount = Count;
        _history.Clear();
        CurrentIndex = -1;

        if (previousCount > 0)
        {
            OnPropertyChanged(nameof(Count));
            UpdateUndoRedoState();
        }
    }

    private void TrimHistory()
    {
        if (Count > MaxHistorySize)
        {
            var removeCount = Count - MaxHistorySize;
            _history.RemoveRange(0, removeCount);
            CurrentIndex -= removeCount;

            OnPropertyChanged(nameof(Count));
        }
    }

    private void UpdateUndoRedoState()
    {
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
    }

    private void OnPropertyChanged(string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
