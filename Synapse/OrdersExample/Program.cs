using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TechnicalAssessmentSH.HttpServer;

namespace TechnicalAssessmentSH.OrdersExample
{
    /// <summary>
    /// I Get a list of orders from the API
    /// I check if the order is in a delivered state, If yes then send a delivery alert and add one to deliveryNotification
    /// I then update the order.   
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Start of App");

            var mockHttpServer = new MockHttpServer();

            var medicalEquipmentOrders = FetchMedicalEquipmentOrders().GetAwaiter().GetResult();
            mockHttpServer.StopServer();

            foreach (var order in medicalEquipmentOrders)
            {
                var updatedOrder = ProcessOrder(order);
                SendAlertAndUpdateOrder(updatedOrder).GetAwaiter().GetResult();
            }

            Console.WriteLine("Results sent to relevant APIs.");
            return 0;
        }

        static async Task<JObject[]> FetchMedicalEquipmentOrders()
        {
            using HttpClient httpClient = new();
            string ordersApiUrl = "http://localhost:9876/orders";
            var response = await httpClient.GetAsync(ordersApiUrl);
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

        static JObject ProcessOrder(JObject order)
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

        static bool IsItemDelivered(JToken item)
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
                string alertApiUrl = "https://alert-api.com/alerts";
                var alertData = new
                {
                    Message = $"Alert for delivered item: Order {orderId}, Item: {item["Description"]}, " +
                              $"Delivery Notifications: {item["deliveryNotification"]}"
                };
                var content = new StringContent(JObject.FromObject(alertData).ToString(), System.Text.Encoding.UTF8, "application/json");
                var response = httpClient.PostAsync(alertApiUrl, content).Result;

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

        static async Task SendAlertAndUpdateOrder(JObject order)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                string updateApiUrl = "https://update-api.com/update";
                var content = new StringContent(order.ToString(), System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(updateApiUrl, content);

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
}