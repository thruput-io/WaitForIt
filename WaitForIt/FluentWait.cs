namespace WaitForIt;

public static class FluentWait
{
    public static FluentWaitBuilder Await()
    {
        return Await(Defaults.Timeout);
    }

    public static FluentWaitBuilder Await(TimeSpan timeout)
    {
        return new FluentWaitBuilder().AtMost(timeout);
    }
}