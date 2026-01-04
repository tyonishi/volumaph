using System;
using System.ComponentModel;
using VoluMaph.UI.ViewModels;
using Xunit;

namespace VoluMaph.UI.Tests.ViewModels;

public sealed class VisualizationHistoryTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        var history = new VisualizationHistory();

        Assert.NotNull(history);
        Assert.Equal(0, history.Count);
        Assert.Equal(-1, history.CurrentIndex);
        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Constructor_WithCustomMaxHistorySize_SetsMaxHistorySize()
    {
        const int customMaxSize = 10;
        var history = new VisualizationHistory(customMaxSize);

        Assert.Equal(customMaxSize, history.MaxHistorySize);
    }

    [Fact]
    public void MaxHistorySize_DefaultIs50()
    {
        var history = new VisualizationHistory();

        Assert.Equal(50, history.MaxHistorySize);
    }

    [Fact]
    public void MaxHistorySize_WhenSet_LessThan1_ThrowsArgumentOutOfRangeException()
    {
        var history = new VisualizationHistory();

        Assert.Throws<ArgumentOutOfRangeException>(() => history.MaxHistorySize = 0);
    }

    [Fact]
    public void AddState_WhenFirstAdded_SetsCurrentIndexTo0()
    {
        var history = new VisualizationHistory();
        var state = new VisualizationState(1.0, 0.0, 0.0, null);

        history.AddState(state);

        Assert.Equal(1, history.Count);
        Assert.Equal(0, history.CurrentIndex);
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void AddState_WhenMultipleAdded_IncreasesCurrentIndex()
    {
        var history = new VisualizationHistory();
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);

        history.AddState(state1);
        history.AddState(state2);

        Assert.Equal(2, history.Count);
        Assert.Equal(1, history.CurrentIndex);
    }

    [Fact]
    public void AddState_WhenAddedAfterUndo_ClearsRedoHistory()
    {
        var history = new VisualizationHistory();
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);
        var state3 = new VisualizationState(3.0, 20.0, 20.0, null);

        history.AddState(state1);
        history.AddState(state2);
        history.Undo();
        history.AddState(state3);

        Assert.Equal(2, history.Count);
        Assert.Equal(1, history.CurrentIndex);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void AddState_WhenExceedsMaxHistorySize_RemovesOldestEntry()
    {
        var history = new VisualizationHistory(3);

        for (int i = 0; i < 5; i++)
        {
            history.AddState(new VisualizationState(i + 1.0, 0.0, 0.0, null));
        }

        Assert.Equal(3, history.Count);
        Assert.Equal(2, history.CurrentIndex);
    }

    [Fact]
    public void AddState_WhenSameAsCurrent_DoesNotAddDuplicate()
    {
        var history = new VisualizationHistory();
        var state = new VisualizationState(1.0, 0.0, 0.0, null);

        history.AddState(state);
        history.AddState(state);

        Assert.Equal(1, history.Count);
        Assert.Equal(0, history.CurrentIndex);
    }

    [Fact]
    public void AddState_WhenAdded_RaisesPropertyChangedForCount()
    {
        var history = new VisualizationHistory();
        var state = new VisualizationState(1.0, 0.0, 0.0, null);
        var propertyChanged = false;

        history.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationHistory.Count))
            {
                propertyChanged = true;
            }
        };

        history.AddState(state);

        Assert.True(propertyChanged);
    }

    [Fact]
    public void AddState_WhenAdded_RaisesPropertyChangedForCanUndo()
    {
        var history = new VisualizationHistory();
        var state = new VisualizationState(1.0, 0.0, 0.0, null);
        var propertyChanged = false;

        history.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationHistory.CanUndo))
            {
                propertyChanged = true;
            }
        };

        history.AddState(state);

        Assert.True(propertyChanged);
    }

    [Fact]
    public void Undo_WhenNoHistory_ThrowsInvalidOperationException()
    {
        var history = new VisualizationHistory();

        Assert.Throws<InvalidOperationException>(() => history.Undo());
    }

    [Fact]
    public void Undo_WhenAtInitialState_SetsCanUndoToFalse()
    {
        var history = new VisualizationHistory();
        var state = new VisualizationState(1.0, 0.0, 0.0, null);

        history.AddState(state);
        history.Undo();

        Assert.Equal(-1, history.CurrentIndex);
        Assert.False(history.CanUndo);
        Assert.True(history.CanRedo);
    }

    [Fact]
    public void Undo_WhenExecuted_DecreasesCurrentIndex()
    {
        var history = new VisualizationHistory();
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);

        history.AddState(state1);
        history.AddState(state2);
        history.Undo();

        Assert.Equal(0, history.CurrentIndex);
        Assert.True(history.CanUndo);
        Assert.True(history.CanRedo);
    }

    [Fact]
    public void Undo_WhenExecuted_RaisesPropertyChangedForCanUndoAndCanRedo()
    {
        var history = new VisualizationHistory();
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);
        var canUndoChanged = false;
        var canRedoChanged = false;

        history.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationHistory.CanUndo))
            {
                canUndoChanged = true;
            }
            if (e.PropertyName == nameof(VisualizationHistory.CanRedo))
            {
                canRedoChanged = true;
            }
        };

        history.AddState(state1);
        history.AddState(state2);
        history.Undo();

        Assert.True(canUndoChanged);
        Assert.True(canRedoChanged);
    }

    [Fact]
    public void Redo_WhenNoFutureState_ThrowsInvalidOperationException()
    {
        var history = new VisualizationHistory();
        var state = new VisualizationState(1.0, 0.0, 0.0, null);

        history.AddState(state);

        Assert.Throws<InvalidOperationException>(() => history.Redo());
    }

    [Fact]
    public void Redo_WhenAtLatestState_SetsCanRedoToFalse()
    {
        var history = new VisualizationHistory();
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);

        history.AddState(state1);
        history.AddState(state2);
        history.Undo();
        history.Redo();

        Assert.Equal(1, history.CurrentIndex);
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Redo_WhenExecuted_IncreasesCurrentIndex()
    {
        var history = new VisualizationHistory();
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);

        history.AddState(state1);
        history.AddState(state2);
        history.Undo();
        history.Redo();

        Assert.Equal(1, history.CurrentIndex);
        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Redo_WhenExecuted_RaisesPropertyChangedForCanUndoAndCanRedo()
    {
        var history = new VisualizationHistory();
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);
        var canUndoChanged = false;
        var canRedoChanged = false;

        history.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationHistory.CanUndo))
            {
                canUndoChanged = true;
            }
            if (e.PropertyName == nameof(VisualizationHistory.CanRedo))
            {
                canRedoChanged = true;
            }
        };

        history.AddState(state1);
        history.AddState(state2);
        history.Undo();
        history.Redo();

        Assert.True(canUndoChanged);
        Assert.True(canRedoChanged);
    }

    [Fact]
    public void Clear_WhenExecuted_ResetsHistory()
    {
        var history = new VisualizationHistory();
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);

        history.AddState(state1);
        history.AddState(state2);
        history.Clear();

        Assert.Equal(0, history.Count);
        Assert.Equal(-1, history.CurrentIndex);
        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Clear_WhenExecuted_RaisesPropertyChangedForAllAffectedProperties()
    {
        var history = new VisualizationHistory();
        var state = new VisualizationState(1.0, 0.0, 0.0, null);
        var countChanged = false;
        var canUndoChanged = false;
        var canRedoChanged = false;

        history.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(VisualizationHistory.Count))
            {
                countChanged = true;
            }
            if (e.PropertyName == nameof(VisualizationHistory.CanUndo))
            {
                canUndoChanged = true;
            }
            if (e.PropertyName == nameof(VisualizationHistory.CanRedo))
            {
                canRedoChanged = true;
            }
        };

        history.AddState(state);
        history.Clear();

        Assert.True(countChanged);
        Assert.True(canUndoChanged);
        Assert.True(canRedoChanged);
    }

    [Fact]
    public void GetCurrentState_WhenNoHistory_ReturnsNull()
    {
        var history = new VisualizationHistory();

        Assert.Null(history.GetCurrentState());
    }

    [Fact]
    public void GetCurrentState_WhenHistoryExists_ReturnsCurrentState()
    {
        var history = new VisualizationHistory();
        var state = new VisualizationState(1.0, 0.0, 0.0, null);

        history.AddState(state);

        Assert.NotNull(history.GetCurrentState());
        Assert.Equal(1.0, history.GetCurrentState()!.ZoomLevel);
    }

    [Fact]
    public void GetCurrentState_AfterUndo_ReturnsPreviousState()
    {
        var history = new VisualizationHistory();
        var state1 = new VisualizationState(1.0, 0.0, 0.0, null);
        var state2 = new VisualizationState(2.0, 10.0, 10.0, null);

        history.AddState(state1);
        history.AddState(state2);
        history.Undo();

        var currentState = history.GetCurrentState();
        Assert.NotNull(currentState);
        Assert.Equal(1.0, currentState!.ZoomLevel);
    }
}
