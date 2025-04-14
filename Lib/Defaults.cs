namespace Lib;

static class Defaults
{ 
    public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);
    public const string TimeoutMessage = "Condition was not met within timeout.";
}