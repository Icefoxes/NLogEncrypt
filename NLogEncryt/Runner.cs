using Microsoft.Extensions.Logging;

public class Runner
{
    private readonly ILogger<Runner> _logger;

    public Runner(ILogger<Runner> logger)
    {
        _logger = logger;
    }

    public void DoAction(string name)
    {
        _logger.LogDebug(20, "Doing hard work! {Action}", name);
    }
}