using System;
using System.Threading;
using System.Windows;

namespace VoluMaph.UI.Tests;

/// <summary>
/// Base class for WPF UI tests that initializes Application.Current with minimal setup.
/// </summary>
public abstract class WpfTestBase : IDisposable
{
    private static readonly object _lock = new();
    private static int _instanceCount;
    private bool _disposed;

    protected WpfTestBase()
    {
        lock (_lock)
        {
            if (_instanceCount == 0)
            {
                InitializeWpfApplication();
            }
            _instanceCount++;
        }
    }

    private static void InitializeWpfApplication()
    {
        // Create Application.Current if it doesn't exist
        if (Application.Current == null)
        {
            var app = new Application();
            app.Resources = new ResourceDictionary();
        }
    }

    /// <summary>
    /// Helper method to run an action on an STA thread (required for WPF).
    /// </summary>
    protected static void RunOnStaThread(Action action)
    {
        Exception? exception = null;

        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
            throw exception!;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            lock (_lock)
            {
                _instanceCount--;

                if (_instanceCount == 0 && Application.Current != null)
                {
                    Application.Current.Shutdown();
                    // Note: Application.Current property cannot be set to null
                    // The WPF framework manages the lifecycle
                }
            }
        }

        _disposed = true;
    }
}
