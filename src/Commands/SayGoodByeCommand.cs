namespace JobQueueDemo;

sealed class SayGoodByeCommand : ICommand
{
    public int Id { get; init; }
    public string Message { get; init; } = default!;
}

sealed class SayGoodByeHandler(ILogger<SayGoodByeHandler> logger) : ICommandHandler<SayGoodByeCommand>
{
    public async Task ExecuteAsync(SayGoodByeCommand command, CancellationToken ct)
    {
        await Task.Delay(500, ct);

        logger.LogInformation("goodbye from id: {id} message: {msg}", command.Id, command.Message);
    }
}