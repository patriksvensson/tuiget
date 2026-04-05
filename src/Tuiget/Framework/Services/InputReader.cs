namespace Tuiget;

internal sealed class InputReader
{
    private int _lastWidth = Console.WindowWidth;
    private int _lastHeight = Console.WindowHeight;
    private bool _initialSizeSent;

    public ValueTask<TeaMessage?> ReadAsync(CancellationToken ct)
    {
        var width = Console.WindowWidth;
        var height = Console.WindowHeight;

        if (!_initialSizeSent || width != _lastWidth || height != _lastHeight)
        {
            _initialSizeSent = true;
            _lastWidth = width;
            _lastHeight = height;
            return new ValueTask<TeaMessage?>(new ResizeMessage());
        }

        return !Console.KeyAvailable
            ? new ValueTask<TeaMessage?>((TeaMessage?)null)
            : new ValueTask<TeaMessage?>(new KeyMessage(Console.ReadKey(true)));
    }
}