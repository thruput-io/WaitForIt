using Shouldly;
using static Lib.WaitForIt;
using static Xunit.TestContext;

namespace UnitTests;

public class WaitForItTests(ITestOutputHelper output)
{
    readonly TimeSpan _200Milliseconds = TimeSpan.FromMilliseconds(200);

    [Fact]
    public async Task Until_WhenAsync_ShouldSucceed()
    {
        var isReady = false;
        _ = Task.Run(async () =>
        {
            await Task.Delay(500, Current.CancellationToken);
            isReady = true;
        }, Current.CancellationToken);

        await Await()
            .Until(() => isReady);

        isReady.ShouldBeTrue();
    }

    [Fact]
    public async Task Until_ShouldSucceed()
    {
        var isReady = false;
        _ = Task.Run(async () =>
        {
            await Task.Delay(500, Current.CancellationToken);
            isReady = true;
        }, Current.CancellationToken);

        await Await()
            .UntilAsync(async () =>
            {
                await Task.Delay(50, Current.CancellationToken);
                return isReady;
            });

        isReady.ShouldBeTrue();
    }

    [Fact]
    public async Task Until_WhenConditionNotMet_ShouldThrow()
    {
        var timeoutException = await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(_200Milliseconds)
                .Until(() => false)
        );
        timeoutException.ShouldBeOfType<TimeoutException>();
    }

    [Fact]
    public async Task UntilAsync_WhenConditionNotMet_ShouldThrow()
    {
        await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(_200Milliseconds)
                .UntilAsync(async () =>
                {
                    await Task.Delay(10, Current.CancellationToken);
                    return false;
                })
        );
    }

    [Fact]
    public async Task UntilAsserted_ShouldSucceed()
    {
        await Await()
            .PollInterval(TimeSpan.FromMilliseconds(100))
            .UntilAsserted(() => true.ShouldBeTrue());
    }

    [Fact]
    public async Task UntilAssertedAsync_ShouldSucceed()
    {
        await Await()
            .PollInterval(TimeSpan.FromMilliseconds(100))
            .UntilAssertedAsync(async () =>
            {
                await Task.Delay(10, Current.CancellationToken);
                true.ShouldBeTrue();
            });
    }

    [Fact]
    public async Task UntilAssertedAsync_ShouldTimeOutAndThrow()
    {
        var timeoutException = await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(_200Milliseconds)
                .UntilAssertedAsync(async () =>
                {
                    await Task.Delay(50, Current.CancellationToken);
                    false.ShouldBeTrue();
                })
        );
        timeoutException.ShouldBeOfType<TimeoutException>();
    }

    [Fact]
    public async Task UntilAsserted_ShouldTimeOutAndThrow()
    {
        var timeoutException = await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(_200Milliseconds)
                .UntilAsserted(() => { false.ShouldBeTrue(); })
        );

        timeoutException.ShouldBeOfType<TimeoutException>();
    }

    [Fact]
    public async Task UntilAssertedAsync_WhenAssertionFail_ShouldThrow()
    {
        var attemptCount = 0;

        var timeoutException = await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(TimeSpan.FromMilliseconds(150))
                .PollInterval(TimeSpan.FromMilliseconds(50))
                .UntilAssertedAsync(async () =>
                {
                    attemptCount++;
                    await Task.Delay(50, Current.CancellationToken);
                    attemptCount.ShouldBe(3);
                }));

        timeoutException.ShouldBeOfType<TimeoutException>();
        timeoutException.InnerException.ShouldNotBeNull();
        timeoutException.InnerException.Message.Substring(0, "attemptCount".Length)
            .ShouldBe("attemptCount");
    }

    [Fact]
    public async Task UntilAsserted_WhenAssertionFail_ShouldThrow()
    {
        var attemptCount = 0;

        var timeoutException = await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(TimeSpan.FromMilliseconds(150))
                .PollInterval(TimeSpan.FromMilliseconds(50))
                .UntilAsserted(() =>
                {
                    attemptCount++;
                    attemptCount.ShouldBe(Int32.MaxValue);
                }));

        timeoutException.ShouldBeOfType<TimeoutException>();
        timeoutException.InnerException.ShouldNotBeNull();
        timeoutException.InnerException.Message.Substring(0, "attemptCount".Length)
            .ShouldBe("attemptCount");
    }

    [Fact]
    public async Task UntilAsserted_InitialFailThenSuccess_ShouldSucceed()
    {
        var attemptCount = 0;
        await Await(TimeSpan.FromSeconds(1))
            .PollInterval(TimeSpan.FromMilliseconds(100))
            .UntilAsserted(() =>
            {
                attemptCount++;
                if (attemptCount <= 2)
                {
                    throw new InvalidOperationException($"Assertion failed on attempt {attemptCount}");
                }

                "success".ShouldBe("success");
            });

        attemptCount.ShouldBe(3);
    }

    [Fact]
    public async Task UntilNotNull_ValueBecomesNotNull_ShouldSucceed()
    {
        string? changingValue = null;
        _ = Task.Run(async () =>
        {
            await Task.Delay(500, Current.CancellationToken);
            changingValue = "Success!";
        }, Current.CancellationToken);

        var result = await Await()
            .UntilNotNull(() => changingValue);

        result.ShouldBe("Success!");
    }

    [Fact]
    public async Task UntilNotNullAsync_ValueBecomesNotNull_ShouldSucceed()
    {
        string? changingValue = null;
        _ = Task.Run(async () =>
        {
            await Task.Delay(500, Current.CancellationToken);
            changingValue = "Success!";
        }, Current.CancellationToken);

        var result = await Await()
            .UntilNotNullAsync(async () => await Task.FromResult(changingValue));

        result.ShouldBe("Success!");
    }

    [Fact]
    public async Task UntilNotNull_ValueRemainsNull_ThrowsTimeoutException()
    {
        await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(_200Milliseconds)
                .UntilNotNull<string>(() => null)
        );
    }

    [Fact]
    public async Task UntilNotNullAsync_SyncValueRemainsNull_ThrowsTimeoutException()
    {
        await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(_200Milliseconds)
                .UntilNotNullAsync(() => Task.FromResult<string?>(null))
        );
    }

    [Fact]
    public async Task UntilAsync_WhenValueSatisfiesPredicate_ShouldReturnValue()
    {
        var counter = 0;
        _ = Task.Run(async () =>
        {
            await Task.Delay(500, Current.CancellationToken);
            counter = 10;
        }, Current.CancellationToken);

        var result = await Await()
            .UntilAsync(async () =>
            {
                await Task.Delay(50, Current.CancellationToken);
                return counter;
            }, count => count == 10);

        result.ShouldBe(10);
    }

    [Fact]
    public async Task UntilAsync_WhenValueDoesNotSatisfyPredicate_ShouldThrow()
    {
        var counter = 0;
        await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(_200Milliseconds)
                .UntilAsync(async () =>
                {
                    await Task.Delay(10, Current.CancellationToken);
                    return counter;
                }, count => count > 5)
        );
    }

    [Fact]
    public async Task Until_WhenValueSatisfiesPredicate_ShouldReturnValue()
    {
        var counter = 0;
        _ = Task.Run(async () =>
        {
            await Task.Delay(500, Current.CancellationToken);
            counter = 10;
        }, Current.CancellationToken);

        var result = await Await()
            .Until(() => counter, count => count == 10);

        result.ShouldBe(10);
    }

    [Fact]
    public async Task Until_WhenValueDoesNotSatisfyPredicate_ShouldThrow()
    {
        var counter = 0;
        await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(_200Milliseconds)
                .Until(() => counter, count => count > 5)
        );
    }

    [Fact]
    public async Task UntilEquals_WhenValueBecomesEqual_ShouldReturnValue()
    {
        var mutable = "Unexpected";

        _ = Task.Run(async () =>
        {
            await Task.Delay(500, Current.CancellationToken);
            mutable = "Expected Value";
        }, Current.CancellationToken);

        var result = await Await()
            .UntilEquals(() => mutable, "Expected Value");

        result.ShouldBe("Expected Value");
    }

    [Fact]
    public async Task UntilEquals_WhenValueNeverBecomesEqual_ShouldThrow()
    {
        var timeoutException = await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(_200Milliseconds)
                .UntilEquals(() => "a value", "Expected Value")
        );
        timeoutException.ShouldBeOfType<TimeoutException>();
    }
    
    [Fact]
    public async Task WithTimeoutMessage_ShouldThrowWithCustomMessage()
    {
        var exception = await Should.ThrowAsync<TimeoutException>(async () =>
            await Await(TimeSpan.FromMilliseconds(100))
                .WithTimeoutMessage("Custom timeout message")
                .Until(() => false)
        );

        exception.Message.ShouldBe("Custom timeout message");
    }

    [Fact]
    public async Task WithCancellation_WhenTokenIsCancelled_ShouldThrowTaskCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        await Should.ThrowAsync<TaskCanceledException>(async () =>
            await Await(TimeSpan.FromSeconds(1))
                .WithCancellation(cts.Token)
                .UntilAsync(async () =>
                {
                    await Task.Delay(100, Current.CancellationToken);
                    return false;
                })
        );
    }
}