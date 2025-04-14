using System.Diagnostics;

namespace Lib;

public class WaitForItExpression
{
    TimeSpan _timeout;
    TimeSpan _pollInterval = TimeSpan.FromMilliseconds(100);
    CancellationToken _cancellationToken = CancellationToken.None;
    Action<Exception>? _logException;
    string _timeoutMessage = Defaults.TimeoutMessage;

    internal WaitForItExpression()
    {
    }

    public WaitForItExpression AtMost(TimeSpan timeout)
    {
        _timeout = timeout;
        return this;
    }

    public WaitForItExpression PollInterval(TimeSpan pollInterval)
    {
        _pollInterval = pollInterval;
        return this;
    }

    public WaitForItExpression WithTimeoutMessage(string timeoutMessage)
    {
        _timeoutMessage = timeoutMessage;
        return this;
    }

    public WaitForItExpression WithCancellation(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        return this;
    }


    public async Task UntilAssertedAsync(Func<Task> assertion)
    {
        var sw = Stopwatch.StartNew();
        Exception? lastException = null;

        while (sw.Elapsed < _timeout)
        {
            if (_cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            try
            {
                await assertion();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                _logException?.Invoke(ex);
            }

            await Task.Delay(_pollInterval, _cancellationToken);
        }

        throw new TimeoutException(
            $"Assertion did not pass within {_timeout.TotalSeconds} seconds. Last exception: {lastException?.Message}",
            lastException);
    }

    public async Task UntilAsserted(Action assertion)
    {
        var sw = Stopwatch.StartNew();
        Exception? lastException = null;

        while (sw.Elapsed < _timeout)
        {
            if (_cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            try
            {
                assertion();
                return;
            }
            catch (Exception? ex)
            {
                lastException = ex;
                _logException?.Invoke(ex);
            }

            await Task.Delay(_pollInterval, _cancellationToken);
        }

        throw new TimeoutException(
            $"{_timeoutMessage}. Last exception: {lastException?.Message}",
            lastException);
    }
}