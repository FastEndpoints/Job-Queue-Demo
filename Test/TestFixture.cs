using FastEndpoints;
using JobQueueDemo;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Test;

public class TestFixture : IDisposable
{
    public HttpClient Client { get; set; }

    readonly WebApplicationFactory<Program> _factory = new();

    public TestFixture()
    {
        Client = _factory.WithWebHostBuilder(
            c =>
            {
                c.ConfigureTestServices(
                    s =>
                    {
                        s.AddJobQueues<JobRecord, TestJobStorageProvider>();                   //register fake storage provider
                        s.RegisterTestCommandHandler<SayGoodByeCommand, TestCommandHandler>(); //register fake handler
                    });
            }).CreateClient();
    }

#region disposable

    bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Client.Dispose();
                _factory.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

#endregion
}