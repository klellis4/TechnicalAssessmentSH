using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace TechnicalAssessmentSH.HttpServer;

/// <summary>
/// This class acts as a local HTTP server to mock API calls/returns
/// </summary>
public class MockHttpServer
{
    private WireMockServer server;

    public MockHttpServer()
    {
        this.StartServer();
        this.CreateGetOrdersStub();
        this.CreatePostUpdateStub();
        this.CreatePostAlertsStub();
    }

    public void StartServer()
    {
        // This starts a new mock server instance listening at port 9876
        this.server = WireMockServer.Start(9876);
    }

    private void CreateGetOrdersStub()
    {
        // This defines a mock API response that responds to an incoming HTTP GET
        // to the `/orders` endpoint with a response with HTTP status code 200,
        // a Content-Type header with value `application/json` and a response body
        // containing the json array of orders read from orders.json
        string json = "";
        using (StreamReader r = new("Orders.json"))
        {
            json = r.ReadToEnd();
        }
        server.Given(
            Request.Create().WithPath("/orders").UsingGet()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(json)
        );
    }

    private void CreatePostUpdateStub()
    {
        // This defines a mock API response that responds to an incoming HTTP POST
        // to the `/update` endpoint with a response with HTTP status code 200
        // and a Content-Type header with value `text/plain`
        server.Given(
            Request.Create().WithPath("/update").UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/plain")
        );
    }

    private void CreatePostAlertsStub()
    {
        // This defines a mock API response that responds to an incoming HTTP POST
        // to the `/alerts` endpoint with a response with HTTP status code 200
        // and a Content-Type header with value `text/plain`
        server.Given(
            Request.Create().WithPath("/alerts").UsingPost()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "text/plain")
        );
    }

    public void StopServer()
    {
        // This stops the mock server to clean up after ourselves
        server.Stop();
    }
}
