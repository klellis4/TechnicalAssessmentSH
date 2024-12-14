using Newtonsoft.Json.Linq;

namespace TechnicalAssessmentSH.Services;

public class MedicalEquipmentOrderService
{
    const string apiBaseUrl = "http://localhost:9876";

    public static async Task<JObject[]> FetchMedicalEquipmentOrders()
    {
        using HttpClient httpClient = new();
        var response = await httpClient.GetAsync(apiBaseUrl + "/orders");
        if (response.IsSuccessStatusCode)
        {
            var ordersData = await response.Content.ReadAsStringAsync();

            return JArray.Parse(ordersData).ToObject<JObject[]>();
        }
        else
        {
            Console.WriteLine("Failed to fetch orders from API.");
            return [];
        }
    }

    public static JObject ProcessOrder(JObject order)
    {
        var items = order["Items"].ToObject<JArray>();
        foreach (var item in items)
        {
            if (IsItemDelivered(item))
            {
                SendAlertMessage(item, order["OrderId"].ToString());
                IncrementDeliveryNotification(item);
            }
        }

        return order;
    }

    private static bool IsItemDelivered(JToken item)
    {
        return item["Status"].ToString().Equals("Delivered", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Delivery alert
    /// </summary>
    /// <param name="orderId">The order id for the alert</param>
    static void SendAlertMessage(JToken item, string orderId)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var alertData = new
            {
                Message = $"Alert for delivered item: Order {orderId}, Item: {item["Description"]}, " +
                            $"Delivery Notifications: {item["DeliveryNotification"]}"
            };
            var content = new StringContent(JObject.FromObject(alertData).ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = httpClient.PostAsync(apiBaseUrl + "/alerts", content).Result;

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Alert sent for delivered item: {item["Description"]}");
            }
            else
            {
                Console.WriteLine($"Failed to send alert for delivered item: {item["Description"]}");
            }
        }
    }

    static void IncrementDeliveryNotification(JToken item)
    {
        item["DeliveryNotification"] = item["DeliveryNotification"].Value<int>() + 1;
    }

    public static async Task SendAlertAndUpdateOrder(JObject order)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            var content = new StringContent(order.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(apiBaseUrl + "/update", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Updated order sent for processing: OrderId {order["OrderId"]}");
            }
            else
            {
                Console.WriteLine($"Failed to send updated order for processing: OrderId {order["OrderId"]}");
            }
        }
    }
}