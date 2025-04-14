# WaitForIt ⏳

[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Build](https://img.shields.io/github/actions/workflow/status/your-org/waitforit/build.yml?label=build)](https://github.com/your-org/waitforit/actions)
[![NuGet](https://img.shields.io/nuget/v/WaitForIt.svg)](https://www.nuget.org/packages/WaitForIt/)
[![Coverage](https://img.shields.io/codecov/c/github/your-org/waitforit)](https://codecov.io/gh/your-org/waitforit)

**WaitForIt** is a lightweight and fluent .NET library inspired by [Awaitility](https://github.com/awaitility/awaitility), designed to make time-bound asynchronous assertions easy and expressive in tests or other wait-retry scenarios.

---

## ✨ Features

- Fluent API for waiting until a condition is met
- Supports sync and async assertions
- Configurable timeout and polling intervals
- Helpful exception messages for debugging
- Optional logging of failures during polling

---

## 🚀 Usage: `UntilAsserted`

```csharp
[Fact]
public async Task UntilAssertedAsync_ShouldEventuallySucceed()
{
    int value = 0;

    // Simulate background change
    _ = Task.Run(async () =>
    {
        await Task.Delay(300);
        value = 42;
    });

    await WaitForIt
        .Await(TimeSpan.FromSeconds(1))
        .PollInterval(TimeSpan.FromMilliseconds(100))
        .UntilAssertedAsync(() =>
        {
            value.ShouldBe(42);
        });
}
