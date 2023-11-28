var bld = WebApplication.CreateBuilder();
bld.Services
   .AddSingleton(new DbContext("JobStoreDatabase", "localhost"))
   .AddAuthorization()
   .AddFastEndpoints()
   .AddJobQueues<JobRecord, JobProvider>();

var app = bld.Build();
app.UseAuthorization()
   .UseFastEndpoints()
   .UseJobQueues(
       o =>
       {
           o.MaxConcurrency = 4;
           o.ExecutionTimeLimit = TimeSpan.FromSeconds(1);
           o.LimitsFor<SayGoodByeCommand>(
               maxConcurrency: 1,
               timeLimit: TimeSpan.FromSeconds(5));
       });
app.Run();

[HttpGet("hello"), AllowAnonymous]
sealed class SayHelloEndpoint : EndpointWithoutRequest
{
    public override async Task HandleAsync(CancellationToken c)
    {
        for (var i = 1; i <= 10; i++)
        {
            await new SayHelloCommand
            {
                Id = i,
                Message = "hello executed!"
            }.QueueJobAsync(ct: c);
        }
        await SendAsync("all jobs queued!", cancellation: c);
    }
}

[HttpGet("goodbye/{id}"), AllowAnonymous]
sealed class GoodByeEndpoint : EndpointWithoutRequest
{
    public override async Task HandleAsync(CancellationToken c)
    {
        await new SayGoodByeCommand
        {
            Id = Route<int>("id"),
            Message = "bye!"
        }.QueueJobAsync(ct: c);

        await SendOkAsync(c);
    }
}

public partial class Program { }