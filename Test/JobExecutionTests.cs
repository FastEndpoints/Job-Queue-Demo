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
    public async Task JobExecutesSuccessfully()
    {
        var (rsp, _) = await _client.GETAsync<SayHelloEndpoint, EmptyResponse>();
        Assert.Equal(HttpStatusCode.OK, rsp.StatusCode);

        var ids = await TestJobStorageProvider.GetCommandIDsFor<SayHelloCommand>();
        Assert.True(ids.Count() == 9);

        var expectedIds = Enumerable.Range(1, 9);
        Assert.True(!expectedIds.Except(ids).Any());
    }
}