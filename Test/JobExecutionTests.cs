using JobQueueDemo;

namespace Test;

public class JobExecutionTests(TestFixture fixture) : IClassFixture<TestFixture>
{
    public readonly HttpClient Client = fixture.Client;

    [Fact]
    public async Task Fake_Command_Handler_Executes_Instead_Of_Real_Handler()
    {
        for (var i = 1; i <= 10; i++)
        {
            var rsp = await Client.GetAsync($"goodbye/{i}");
            Assert.Equal(HttpStatusCode.OK, rsp.StatusCode);

            Assert.True(await TestCommandHandler.IsReceived(i, "bye!"));
        }
    }

    [Fact]
    public async Task Fake_Storage_Provider_Receives_Jobs()
    {
        var (rsp, _) = await Client.GETAsync<SayHelloEndpoint, EmptyResponse>();
        Assert.Equal(HttpStatusCode.OK, rsp.StatusCode);

        var ids = await TestJobStorageProvider.GetCommandIDsFor<SayHelloCommand>();
        Assert.Equal(9, ids.Count());

        var expectedIds = Enumerable.Range(1, 9);
        Assert.False(expectedIds.Except(ids).Any());
    }
}