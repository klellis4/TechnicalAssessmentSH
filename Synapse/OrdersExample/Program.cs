using TechnicalAssessmentSH.HttpServer;
using TechnicalAssessmentSH.Services;

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

            HandleOrderProcessing();

            mockHttpServer.StopServer();
            return 0;
        }

        private static void HandleOrderProcessing()
        {
            var medicalEquipmentOrders = MedicalEquipmentOrderService.FetchMedicalEquipmentOrders().GetAwaiter().GetResult();

            foreach (var order in medicalEquipmentOrders)
            {
                var updatedOrder = MedicalEquipmentOrderService.ProcessOrder(order);
                MedicalEquipmentOrderService.SendAlertAndUpdateOrder(updatedOrder).GetAwaiter().GetResult();
            }

            Console.WriteLine("Results sent to relevant APIs.");
        }
    }
}