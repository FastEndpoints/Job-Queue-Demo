namespace JobQueueDemo;

public sealed class LongRunningCommand : ICommand;

sealed class LongRunningCommandHandler(ILogger<LongRunningCommandHandler> logger) : ICommandHandler<LongRunningCommand>
{
    public async Task ExecuteAsync(LongRunningCommand cmd, CancellationToken ct)
    {
        var startTime = DateTime.Now;

        while (!ct.IsCancellationRequested) //periodically check if this job should be terminated
        {
            logger.LogInformation("It's been {elapsed:0} seconds since the long running job was started...", DateTime.Now.Subtract(startTime).TotalSeconds);

            await Task.Delay(1000);
        }

        logger.LogWarning("the long running task was cancelled after {elapsed:0} seconds!", DateTime.Now.Subtract(startTime).TotalSeconds);
    }
}