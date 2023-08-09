using FastEndpoints;
using JobQueueDemo;
using System.Collections.Concurrent;

namespace Test;

public sealed class TestJobStorageProvider : IJobStorageProvider<JobRecord>
{
    private static readonly ConcurrentDictionary<string, List<JobRecord>> _queues = new();

    public Task StoreJobAsync(JobRecord r, CancellationToken ct)
    {
        var q = _queues.GetOrAdd(r.QueueID, new List<JobRecord>());
        q.Add(r);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<JobRecord>> GetNextBatchAsync(PendingJobSearchParams<JobRecord> p)
    {
        var q = _queues.GetOrAdd(p.QueueID, new List<JobRecord>());
        var match = p.Match.Compile();
        return Task.FromResult(q.Where(match).Take(p.Limit));
    }

    public Task MarkJobAsCompleteAsync(JobRecord r, CancellationToken ct)
    {
        r.IsComplete = true;
        return Task.CompletedTask;
    }

    public Task OnHandlerExecutionFailureAsync(JobRecord r, Exception x, CancellationToken ct) => Task.CompletedTask;

    public Task PurgeStaleJobsAsync(StaleJobSearchParams<JobRecord> p) => Task.CompletedTask;

    public static async Task<IEnumerable<int>> GetCommandIDsFor<TCommand>() where TCommand : ICommand
    {
        foreach (var q in _queues)
        {
            if (q.Value.FirstOrDefault()?.Command.GetType() == typeof(TCommand))
            {
                while (q.Value.Count < 10)
                {
                    await Task.Delay(100);
                }
                var expected = q.Value.Select(r => new { r.IsComplete, ((SayHelloCommand)r.Command).Id }).Where(x => x.Id != 10);
                while (!expected.All(x => x.IsComplete))
                {
                    await Task.Delay(100);
                }
                return expected.Select(x => x.Id);
            }
        }
        return Enumerable.Empty<int>();
    }
}