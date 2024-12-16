using Microsoft.Extensions.Logging;
using TechnicalAssessmentSH.HttpServer;
using TechnicalAssessmentSH.Services;

namespace TechnicalAssessmentSH
{
    class Program(MedicalEquipmentOrderService medicalEquipmentOrderService
        , ILogger<MedicalEquipmentOrderService> logger)
    {
        public readonly MedicalEquipmentOrderService medicalEquipmentOrderService = medicalEquipmentOrderService;
        public readonly ILogger<MedicalEquipmentOrderService> logger = logger;

        public static async Task<int> Main(string[] args)
        {

            var mockHttpServer = new MockHttpServer();

            /// In a real life production scenario, I would configure this logger to log to 
            /// something like a database, azure app insights, etc. But due to time constraints, I'm
            /// just logging to the console
            using var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder
                .SetMinimumLevel(LogLevel.Trace)
                .AddConsole());
            ILogger<MedicalEquipmentOrderService> logger = loggerFactory.CreateLogger<MedicalEquipmentOrderService>();
            MedicalEquipmentOrderService medicalEquipmentOrderService = new(logger, new HttpClient());

            logger.LogInformation("Start of App");

            await medicalEquipmentOrderService.HandleOrderProcessing();
            mockHttpServer.StopServer();
            return 0;
        }
    }
}