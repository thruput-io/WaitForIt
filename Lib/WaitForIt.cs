namespace Lib;

public static class WaitForIt
{
    public static WaitForItExpression Await()
    {
        return Await(Defaults.Timeout);
    }

    public static WaitForItExpression Await(TimeSpan timeout)
    {
        return new WaitForItExpression().AtMost(timeout);
    }
}