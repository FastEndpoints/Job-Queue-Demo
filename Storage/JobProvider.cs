using System.Linq.Expressions;
using Order = MongoDB.Entities.Order;

namespace JobQueueDemo;

sealed class JobProvider : IJobStorageProvider<JobRecord>
{
    private readonly DbContext db;
    private readonly ILogger<JobProvider> logger;

    public JobProvider(DbContext db, ILogger<JobProvider> logger)
    {
        this.db = db;
        this.logger = logger;
    }

    public Task StoreJobAsync(JobRecord job, CancellationToken ct)
    {
        return db.SaveAsync(job, ct);
    }

    public async Task<IEnumerable<JobRecord>> GetNextBatchAsync(Expression<Func<JobRecord, bool>> match, int batchSize, CancellationToken ct)
    {
        return await db
            .Find<JobRecord>()
            .Match(match)
            .Sort(r => r.ID, Order.Ascending)
            .Limit(batchSize)
            .ExecuteAsync(ct);
    }

    public Task MarkJobAsCompleteAsync(JobRecord job, CancellationToken ct)
    {
        return db
            .Update<JobRecord>()
            .MatchID(job.ID)
            .Modify(r => r.IsComplete, true)
            .ExecuteAsync(ct);
    }

    public Task OnHandlerExecutionFailureAsync(JobRecord job, Exception exception, CancellationToken ct)
    {
        logger.LogCritical("job-id: {id} command type: {tCommand} ex: {msg}", job.ID, job.Command.GetType().FullName, exception.Message);
        return db
            .Update<JobRecord>()
            .MatchID(job.ID)
            .Modify(r => r.ExecuteAfter, DateTime.UtcNow.AddHours(1))
            .ExecuteAsync(ct);

        // alternatively, you can update the job.ExpireOn property as well to do infinite retries.
    }

    public Task PurgeStaleJobsAsync(Expression<Func<JobRecord, bool>> match, CancellationToken ct)
    {
        return db.DeleteAsync(match, ct);
    }
}
