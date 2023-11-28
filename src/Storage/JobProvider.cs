using Order = MongoDB.Entities.Order;

namespace JobQueueDemo;

sealed class JobProvider(DbContext db, ILogger<JobProvider> logger) : IJobStorageProvider<JobRecord>
{
    public Task StoreJobAsync(JobRecord job, CancellationToken ct)
        => db.SaveAsync(job, ct);

    public async Task<IEnumerable<JobRecord>> GetNextBatchAsync(PendingJobSearchParams<JobRecord> p)
        => await db.Find<JobRecord>()
                   .Match(p.Match)
                   .Sort(r => r.ID, Order.Ascending)
                   .Limit(p.Limit)
                   .ExecuteAsync(p.CancellationToken);

    public Task MarkJobAsCompleteAsync(JobRecord job, CancellationToken ct)
        => db.Update<JobRecord>()
             .MatchID(job.ID)
             .Modify(r => r.IsComplete, true)
             .ExecuteAsync(ct);

    public Task OnHandlerExecutionFailureAsync(JobRecord job, Exception exception, CancellationToken ct)
    {
        logger.LogInformation("Rescheduling failed job to be retried after 60 seconds...");

        return db.Update<JobRecord>()
                 .MatchID(job.ID)
                 .Modify(r => r.ExecuteAfter, DateTime.UtcNow.AddMinutes(1))
                 .ExecuteAsync(ct);
    }

    public Task PurgeStaleJobsAsync(StaleJobSearchParams<JobRecord> p)
        => db.DeleteAsync(p.Match, p.CancellationToken);
}