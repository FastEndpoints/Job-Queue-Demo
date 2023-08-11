using FastEndpoints;
using JobQueueDemo;
using System.Net;

namespace Test;

public class JobExecutionTests : IClassFixture<TestFixture>
{
    public readonly HttpClient _client;

    public JobExecutionTests(TestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Fake_Command_Handler_Executes_Instead_Of_Real_Handler()
    {
        for (int i = 1; i <= 10; i++)
        {
            var rsp = await _client.GetAsync($"goodbye/{i}");
            Assert.Equal(HttpStatusCode.OK, rsp.StatusCode);

            Assert.True(await TestCommandHandler.IsReceived(i, "bye!"));
        }
    }

    [Fact]
    public async Task Fake_Storage_Provider_Receives_Jobs()
    {
        var (rsp, _) = await _client.GETAsync<SayHelloEndpoint, EmptyResponse>();
        Assert.Equal(HttpStatusCode.OK, rsp.StatusCode);

        var ids = await TestJobStorageProvider.GetCommandIDsFor<SayHelloCommand>();
        Assert.True(ids.Count() == 9);

        var expectedIds = Enumerable.Range(1, 9);
        Assert.True(!expectedIds.Except(ids).Any());
    }
}