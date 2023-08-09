namespace JobQueueDemo;

sealed class SayHelloCommand : ICommand
{
    public int Id { get; set; }
    public string Message { get; set; } = default!;
}

sealed class SayHelloHandler : ICommandHandler<SayHelloCommand>
{
    private readonly ILogger<SayHelloHandler> logger;

    public SayHelloHandler(ILogger<SayHelloHandler> logger)
    {
        this.logger = logger;
    }

    public async Task ExecuteAsync(SayHelloCommand command, CancellationToken ct)
    {
        if (command.Id == 10) // the 10th hello will timeout because max execution time is 1 second
            await Task.Delay(1100, ct);
        else
            await Task.Delay(500, ct);

        logger.LogInformation("hello from id: {id} message: {msg}", command.Id, command.Message);
    }
}