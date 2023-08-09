global using FastEndpoints;
using JobQueueDemo;
using Microsoft.AspNetCore.Authorization;

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
       o.ExecutionTimeLimit = TimeSpan.FromSeconds(1);
       o.LimitsFor<SayGoodByeCommand>(
           maxConcurrency: 1,
           timeLimit: TimeSpan.FromSeconds(5));
   });
app.Run();

[HttpGet("test"), AllowAnonymous]
sealed class SayHelloEndpoint : EndpointWithoutRequest
{
    public override async Task HandleAsync(CancellationToken c)
    {
        for (int i = 1; i <= 10; i++)
        {
            await new SayHelloCommand
            {
                Id = i,
                Message = "hello executed!"

            }.QueueJobAsync();
        }

        //await Parallel.ForEachAsync(Enumerable.Range(1, 10), async (i, ct) =>
        //{
        //    await new SayGoodByeCommand
        //    {
        //        Id = i,
        //        Message = "    >>>>>>>>>>>>>>>>>> goodbye executed!"

        //    }.QueueJobAsync();
        //});

        await SendAsync("all jobs queued!");
    }
}

public partial class Program { }