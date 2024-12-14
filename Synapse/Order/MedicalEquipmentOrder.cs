namespace TechnicalAssessmentSH.Order;

public class MedicalEquipmentOrder()
{
    public string OrderId = "";
    public List<Item>? Items;

    public class Item()
    {
        public string Status = "";
        public int DeliveryNotification = 0;
    }

}