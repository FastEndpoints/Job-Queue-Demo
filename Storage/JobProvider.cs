﻿using Order = MongoDB.Entities.Order;

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

    public async Task<IEnumerable<JobRecord>> GetNextBatchAsync(PendingJobSearchParams<JobRecord> p)
    {
        return await db
            .Find<JobRecord>()
            .Match(p.Match)
            .Sort(r => r.ID, Order.Ascending)
            .Limit(p.Limit)
            .ExecuteAsync(p.CancellationToken);
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
        logger.LogInformation("Rescheduling failed job to be retried after 60 seconds...");

        return db
            .Update<JobRecord>()
            .MatchID(job.ID)
            .Modify(r => r.ExecuteAfter, DateTime.UtcNow.AddMinutes(1))
            .ExecuteAsync(ct);
    }

    public Task PurgeStaleJobsAsync(StaleJobSearchParams<JobRecord> p)
    {
        return db.DeleteAsync(p.Match, p.CancellationToken);
    }
}
