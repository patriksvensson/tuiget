namespace Tuiget;

public sealed class Debouncer : IDisposable
{
    private CancellationTokenSource? _debounceCancellationTokenSource;
    private bool _disposed;

    public async Task<T?> DebounceAsync<T>(int millisecondsDelay, Func<CancellationToken, Task<T>> actionAsync)
        where T : class
    {
        try
        {
            if (_debounceCancellationTokenSource != null)
            {
                await _debounceCancellationTokenSource.CancelAsync();
                _debounceCancellationTokenSource.Dispose();
            }

            _debounceCancellationTokenSource = new CancellationTokenSource();

            await Task.Delay(millisecondsDelay, _debounceCancellationTokenSource.Token);
            return await actionAsync(_debounceCancellationTokenSource.Token);
        }
        catch (TaskCanceledException)
        {
        }

        return null;
    }

    public void Dispose()
    {
        Dispose(disposing: true);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            if (_debounceCancellationTokenSource != null)
            {
                _debounceCancellationTokenSource?.Cancel();
                _debounceCancellationTokenSource?.Dispose();
            }
        }

        _disposed = true;
    }
}