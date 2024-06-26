﻿using MongoDB.Entities;

namespace JobQueueDemo;

public sealed class JobRecord : Entity, IJobStorageRecord
{
    public string QueueID { get; set; } = default!;
    public Guid TrackingID { get; set; } = default!;
    public object Command { get; set; } = default!;
    public DateTime ExecuteAfter { get; set; }
    public DateTime ExpireOn { get; set; }
    public bool IsComplete { get; set; }
    public bool IsCancelled { get; set; } //not enforced by the interface
}