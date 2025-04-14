using Shouldly;
using WaitForIt;
using static Xunit.TestContext;

namespace UnitTests;

public class FluentWaitTests
{
    readonly TimeSpan _200Milliseconds = TimeSpan.FromMilliseconds(200);

    [Fact]
    public async Task UntilAsserted_ShouldSucceed()
    {
        await FluentWait.Await()
            .PollInterval(TimeSpan.FromMilliseconds(100))
            .UntilAsserted(() => true.ShouldBeTrue());
    }
    
    [Fact]
    public async Task UntilAsserted_WithReturn_ShouldSucceed()
    {
        var result = await FluentWait.Await()
            .PollInterval(TimeSpan.FromMilliseconds(100))
            .UntilAsserted(() =>
            {
                "value".ShouldBe("value");
                return "value";
            });
        result.ShouldBe("value");
    }

    [Fact]
    public async Task UntilAssertedAsync_ShouldSucceed()
    {
        await FluentWait.Await()
            .PollInterval(TimeSpan.FromMilliseconds(100))
            .UntilAssertedAsync(async () =>
            {
                await Task.Delay(10, Current.CancellationToken);
                true.ShouldBeTrue();
            });
    }
    [Fact]
    public async Task UntilAssertedAsync_WithReturnValue_ShouldSucceed()
    {
        var result = await FluentWait.Await()
            .PollInterval(TimeSpan.FromMilliseconds(100))
            .UntilAssertedAsync(async () =>
            {
                await Task.Delay(10, Current.CancellationToken);
                true.ShouldBeTrue();
                return "value";
            });
        result.ShouldBe("value");
    }
    
    [Fact]
    public async Task UntilAssertedAsync_WithReturn_ShouldSucceed()
    {
        await FluentWait.Await()
            .PollInterval(TimeSpan.FromMilliseconds(100))
            .UntilAssertedAsync(async () =>
            {
                await Task.Delay(10, Current.CancellationToken);
                true.ShouldBeTrue();
            });
        
    }

    [Fact]
    public async Task UntilAssertedAsync_WhenOperationTimesOut_ShouldThrow()
    {
        var timeoutException = await Should.ThrowAsync<TimeoutException>(async () =>
            await FluentWait.Await(_200Milliseconds)
                .UntilAssertedAsync(async () =>
                {
                    await Task.Delay(50, Current.CancellationToken);
                    false.ShouldBeTrue();
                })
        );
        timeoutException.ShouldBeOfType<TimeoutException>();
    }

    [Fact]
    public async Task UntilAsserted_WhenOperationTimesOut_ShouldThrow()
    {
        var timeoutException = await Should.ThrowAsync<TimeoutException>(async () =>
            await FluentWait.Await(_200Milliseconds)
                .UntilAsserted(() => { false.ShouldBeTrue(); })
        );

        timeoutException.ShouldBeOfType<TimeoutException>();
    }

    [Fact]
    public async Task UntilAssertedAsync_WhenAssertionFail_ShouldThrow()
    {
        var attemptCount = 0;

        var timeoutException = await Should.ThrowAsync<TimeoutException>(async () =>
            await FluentWait.Await(TimeSpan.FromMilliseconds(150))
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
            await FluentWait.Await(TimeSpan.FromMilliseconds(150))
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
    public async Task UntilAsserted_WhenSuccessWithinTimeout_ShouldSucceed()
    {
        var attemptCount = 0;
        await FluentWait.Await(TimeSpan.FromSeconds(1))
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
}