namespace TechnicalAssessmentSH.Models;

public class MedicalEquipmentOrder()
{
    public string OrderId { get; set; } = "";
    public string OrderFirstName { get; set; } = "";
    public string OrderLastName { get; set; } = "";
    public double Total { get; set; } = 0.00;
    public List<Item>? Items { get; set; }

    public class Item()
    {
        public string Description { get; set; } = "";
        public string Status { get; set; } = "";
        public int DeliveryNotification { get; set; } = 0;
        public double Price { get; set; }
    }
}