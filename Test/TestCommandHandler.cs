using FastEndpoints;
using JobQueueDemo;
using System.Collections.Concurrent;

namespace Test;

[DontRegister] //it is essential to mark test command handlers with this
sealed class TestCommandHandler : ICommandHandler<SayGoodByeCommand>
{
    public static ConcurrentBag<string> Received { get; } = new();

    public Task ExecuteAsync(SayGoodByeCommand command, CancellationToken ct)
    {
        Received.Add($"{command.Id} - {command.Message}");
        return Task.CompletedTask;
    }

    public static async Task<bool> IsReceived(int id, string msg)
    {
        while (!Received.Contains($"{id} - {msg}"))
        {
            await Task.Delay(100);
        }
        return true;
    }
}