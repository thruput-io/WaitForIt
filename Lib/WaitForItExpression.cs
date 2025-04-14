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

    public async Task<T> Until<T>(Func<T> supplier, Func<T, bool> predicate)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < _timeout)
        {
            if (_cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            var result = supplier();
            if (predicate(result))
                return result;

            await Task.Delay(_pollInterval, _cancellationToken);
        }

        throw new TimeoutException(_timeoutMessage);
    }

    public async Task Until(Func<bool> condition)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < _timeout)
        {
            if (_cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            if (condition())
                return;

            await Task.Delay(_pollInterval, _cancellationToken);
        }

        throw new TimeoutException(_timeoutMessage);
    }

    public async Task UntilAsync(Func<Task<bool>> condition)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < _timeout)
        {
            if (_cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            if (await condition())
                return;

            await Task.Delay(_pollInterval, _cancellationToken);
        }

        throw new TimeoutException(_timeoutMessage);
    }

    public async Task<T> UntilAsync<T>(Func<Task<T>> supplier, Func<T, bool> predicate)
    {
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed < _timeout)
        {
            if (_cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();

            var result = await supplier();
            if (predicate(result))
                return result;

            await Task.Delay(_pollInterval, _cancellationToken);
        }

        throw new TimeoutException(_timeoutMessage);
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

    public async Task<T> UntilNotNullAsync<T>(Func<Task<T?>> supplier) where T : class
    {
        T? result = null;
        await UntilAsync(async () =>
        {
            result = await supplier();
            return result != null;
        });
        return result!;
    }

    public async Task<T> UntilNotNull<T>(Func<T?> supplier) where T : class
    {
        T? result = null;
        await Until(() =>
        {
            result = supplier();
            return result != null;
        });
        return result!;
    }

    public async Task<T> UntilEqualsAsync<T>(Func<Task<T>> supplier, T expectedValue)
    {
        return await UntilAsync(supplier, actual => EqualityComparer<T>.Default.Equals(actual, expectedValue));
    }

    public async Task<T> UntilEquals<T>(Func<T> supplier, T expectedValue)
    {
        return await Until(supplier, actual => EqualityComparer<T>.Default.Equals(actual, expectedValue));
    }
}