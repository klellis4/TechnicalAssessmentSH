using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using TechnicalAssessmentSH.Models;
using TechnicalAssessmentSH.Services;
using static TechnicalAssessmentSH.Models.MedicalEquipmentOrder;

namespace TechnicalAssessmentSHTest.Services;

[TestFixture]
public sealed class MedicalEquipmentOrderServiceTests
{
    private MedicalEquipmentOrderService medicalEquipmentOrderService;
    private Mock<ILogger<MedicalEquipmentOrderService>> loggerMock;
    private MedicalEquipmentOrder medicalEquipmentOrder;

    [SetUp]
    public void SetUp()
    {
        loggerMock = new Mock<ILogger<MedicalEquipmentOrderService>>();
        medicalEquipmentOrder = new()
        {
            OrderId = "1234",
            OrderFirstName = "Test",
            OrderLastName = "User",
            Total = 39.99,
            Items = []
        };
    }

    [Test]
    public async Task HandleOrderProcessing_Should_LogErrorIfOrdersIsNull()
    {
        List<MedicalEquipmentOrder> orders = [medicalEquipmentOrder];
        var serializedOrder = JsonConvert.SerializeObject(orders);
        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent(serializedOrder)
        };

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(handlerMock.Object);

        medicalEquipmentOrderService = new MedicalEquipmentOrderService(loggerMock.Object, httpClient);

        await medicalEquipmentOrderService.HandleOrderProcessing();

        loggerMock.Verify(logger => logger.Log(
            It.Is<LogLevel>(loglevel => loglevel == LogLevel.Information),
            It.Is<EventId>(eventId => eventId.Id == 0),
            It.Is<It.IsAnyType>((@object, @type) => @object.ToString() == "Results sent to relevant APIs." && @type.Name == "FormattedLogValues"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void ProcessOrder_Should_IncrementItemsDeliverNotification_If_IsDelivered()
    {
        Item item = new()
        {
            Description = "Test item",
            DeliveryNotification = 0,
            Price = 39.99,
            Status = "Delivered"
        };

        medicalEquipmentOrder.Items.Add(item);

        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = System.Net.HttpStatusCode.OK
        };

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(handlerMock.Object);

        medicalEquipmentOrderService = new MedicalEquipmentOrderService(loggerMock.Object, httpClient);

        medicalEquipmentOrderService.ProcessOrder(medicalEquipmentOrder);

        NUnit.Framework.Assert.That(medicalEquipmentOrder.Items[0].DeliveryNotification, Is.EqualTo(1));
    }

    public void ProcessOrder_Should_NotIncrementItemsDeliverNotification_If_IsNotDelivered()
    {
        Item item = new()
        {
            Description = "Test item",
            DeliveryNotification = 0,
            Price = 39.99,
            Status = "Ordered"
        };

        medicalEquipmentOrder.Items.Add(item);

        HttpResponseMessage httpResponseMessage = new()
        {
            StatusCode = System.Net.HttpStatusCode.OK
        };

        var handlerMock = new Mock<HttpMessageHandler>();

        handlerMock
        .Protected()
        .Setup<Task<HttpResponseMessage>>(
          "SendAsync",
          ItExpr.IsAny<HttpRequestMessage>(),
          ItExpr.IsAny<CancellationToken>())
        .ReturnsAsync(httpResponseMessage);

        var httpClient = new HttpClient(handlerMock.Object);

        medicalEquipmentOrderService = new MedicalEquipmentOrderService(loggerMock.Object, httpClient);

        medicalEquipmentOrderService.ProcessOrder(medicalEquipmentOrder);

        NUnit.Framework.Assert.That(medicalEquipmentOrder.Items[0].DeliveryNotification, Is.EqualTo(0));
    }
}
