global using FastEndpoints;
using JobQueueDemo;

var bld = WebApplication.CreateBuilder();
bld.Services
   .AddSingleton(new DbContext("JobStoreDatabase", "localhost"))
   .AddFastEndpoints()
   .AddJobQueues<JobRecord, JobProvider>();

var app = bld.Build();
app.UseAuthorization()
   .UseFastEndpoints()
   .UseJobQueues(o =>
   {
       o.MaxConcurrency = 4;
       o.ExecutionTimeLimit = TimeSpan.FromSeconds(2);
       o.LimitsFor<SayGoodByeCommand>(
           maxConcurrency: 1,
           timeLimit: TimeSpan.FromSeconds(5));
   });
app.Run();

sealed class TestJobQueueEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("test");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        await Parallel.ForEachAsync(Enumerable.Range(1, 10), async (i, ct) =>
        {
            await new SayHelloCommand
            {
                Id = i,
                Message = "hello executed!"

            }.QueueJobAsync(ct);
        });

        await Parallel.ForEachAsync(Enumerable.Range(1, 10), async (i, ct) =>
        {
            await new SayGoodByeCommand
            {
                Id = i,
                Message = "    >>>>>>>>>>>>>>>>>> goodbye executed!"

            }.QueueJobAsync(ct);
        });

        await SendAsync("all jobs queued!");
    }
}