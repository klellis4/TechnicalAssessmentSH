using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using TechnicalAssessmentSH.Models;

namespace TechnicalAssessmentSH.Services;

public class MedicalEquipmentOrderService(ILogger<MedicalEquipmentOrderService> logger)
{
    // In a real life production scenario, this url would be configurable for
    // different environments in something like helm
    const string apiBaseUrl = "http://localhost:9876";
    public readonly ILogger logger = logger;

    /// <summary>
    /// Gets a list of orders from the API, processes the orders, and then updates them
    /// </summary>
    public async Task HandleOrderProcessing()
    {
        var medicalEquipmentOrders = await FetchMedicalEquipmentOrders();

        if (medicalEquipmentOrders == null)
        {
            this.logger.LogError("No orders data was found.");
        }
        else
        {
            foreach (var order in medicalEquipmentOrders)
            {
                var updatedOrder = ProcessOrder(order);
                await UpdateOrder(updatedOrder);
            }
        }

        this.logger.LogInformation("Results sent to relevant APIs.");
    }

    /// <summary>
    /// Fetches the list of medical equipment orders from the api (json file)
    /// and parses them into a list of MedicalEquipmentOrder objects
    /// </summary>
    public async Task<List<MedicalEquipmentOrder>?> FetchMedicalEquipmentOrders()
    {
        using HttpClient httpClient = new();

        var response = await httpClient.GetAsync(apiBaseUrl + "/orders");

        if (response.IsSuccessStatusCode)
        {
            var ordersData = await response.Content.ReadAsStringAsync();

            if (ordersData == null)
            {
                this.logger.LogError("No orders data was found.");
                return null;
            }

            return JsonSerializer.Deserialize<List<MedicalEquipmentOrder>>(ordersData);
        }
        else
        {
            this.logger.LogError("Failed to fetch orders from API.");
            return [];
        }
    }

    /// <summary>
    /// Checks if the order is in a delivered state,
    /// If yes then send a delivery alert and increment deliveryNotification
    /// </summary>
    /// <param name="order">The item in the order that was delivered</param>

    public MedicalEquipmentOrder ProcessOrder(MedicalEquipmentOrder order)
    {
        if (order.Items == null)
        {
            this.logger.LogError("Order {OrderId} contains no items.", order.OrderId);
        }
        else
        {
            foreach (var item in order.Items)
            {
                if (IsItemDelivered(item))
                {
                    SendAlertMessage(item, order.OrderId);
                    item.DeliveryNotification++;
                }
            }
        }

        return order;
    }

    private static bool IsItemDelivered(MedicalEquipmentOrder.Item item)
    {
        return item.Status.ToString().Equals("Delivered", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Sends a delivery alert to the /alerts endpoint of the API
    /// with the order's information
    /// </summary>
    /// <param name="item">The item in the order that was delivered</param>
    /// <param name="orderId">The order id for the alert</param>
    private void SendAlertMessage(MedicalEquipmentOrder.Item item, string orderId)
    {
        using HttpClient httpClient = new();
        var alertData = new
        {
            Message = $"Alert for delivered item: Order {orderId}, Item: {item.Description}, " +
                        $"Delivery Notifications: {item.DeliveryNotification}"
        };
        var content = new StringContent(JObject.FromObject(alertData).ToString(), System.Text.Encoding.UTF8, "application/json");
        var response = httpClient.PostAsync(apiBaseUrl + "/alerts", content).Result;

        if (response.IsSuccessStatusCode)
        {
            this.logger.LogInformation("Alert sent for delivered item: {description}", item.Description);
        }
        else
        {
            this.logger.LogError("Failed to send alert for delivered item: {description}", item.Description);
        }
    }

    /// <summary>
    /// Sends the updated order to the /update endpoint of the API
    /// </summary>
    /// <param name="order">The updated order object</param>
    public async Task UpdateOrder(MedicalEquipmentOrder order)
    {
        using HttpClient httpClient = new();
        var content = new StringContent(order.ToString(), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync(apiBaseUrl + "/update", content);

        if (response.IsSuccessStatusCode)
        {
            this.logger.LogInformation("Updated order sent for processing: OrderId {OrderId}", order.OrderId);
        }
        else
        {
            this.logger.LogError("Failed to send updated order for processing: OrderId {OrderId}", order.OrderId);
        }
    }
}