namespace JobQueueDemo;

sealed class SayHelloCommand : ICommand
{
    public int Id { get; init; }
    public string Message { get; init; } = default!;
}

sealed class SayHelloHandler(ILogger<SayHelloHandler> logger) : ICommandHandler<SayHelloCommand>
{
    public async Task ExecuteAsync(SayHelloCommand command, CancellationToken ct)
    {
        if (command.Id == 10) // the 10th hello will time out because max execution time is 1 second
            await Task.Delay(1100, ct);
        else
            await Task.Delay(500, ct);

        logger.LogInformation("hello from id: {id} message: {msg}", command.Id, command.Message);
    }
}