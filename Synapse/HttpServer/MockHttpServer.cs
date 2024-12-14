using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace TechnicalAssessmentSH.HttpServer;

public class MockHttpServer
{
    private WireMockServer server;

    public MockHttpServer()
    {
        this.StartServer();
        this.CreateGetOrdersStub();
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
        // a Content-Type header with value `text/plain` and a response body
        // containing the text `Hello, world!`
        server.Given(
            Request.Create().WithPath("/orders").UsingGet()
        )
        .RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(
                    "[" +
                        "{" +
                            "\"OrderId\" : \"1\"," +
                            "\"OrderFirstName\" : \"Jack\"," +
                            "\"OrderLastName\" : \"Shephard\"," +
                            "\"Items\": " +
                            "[" +
                                "{" +
                                    "\"Status\" : \"Sent\"," +
                                    "\"DeliveryNotification\" : \"0\"" +
                                "}" +
                            "]" +
                        "}" +
                    "]"
                )
        );
    }

    public void StopServer()
    {
        // This stops the mock server to clean up after ourselves
        server.Stop();
    }
}
