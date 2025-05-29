namespace IndustryX.WebUI.ViewModels
{
    public class UserDeficitViewModel
    {
        public string TransferBarcode { get; set; }
        public string ProductName { get; set; }
        public int DeficitQuantity { get; set; }
        public string SourceWarehouse { get; set; }
        public string DestinationWarehouse { get; set; }
        public DateTime DeliveredAt { get; set; }
    }
}
