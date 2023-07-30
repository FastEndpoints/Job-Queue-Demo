namespace JobQueueDemo;

sealed class SayGoodByeCommand : ICommand
{
    public int Id { get; set; }
    public string Message { get; set; } = default!;
}

sealed class SayGoodByeHandler : ICommandHandler<SayGoodByeCommand>
{
    private readonly ILogger<SayGoodByeHandler> logger;

    public SayGoodByeHandler(ILogger<SayGoodByeHandler> logger)
    {
        this.logger = logger;
    }

    public async Task ExecuteAsync(SayGoodByeCommand command, CancellationToken ct)
    {
        await Task.Delay(500, ct);

        logger.LogInformation("goodbye from id: {id} message: {msg}", command.Id, command.Message);
    }
}