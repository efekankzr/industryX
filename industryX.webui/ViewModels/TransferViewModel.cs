namespace IndustryX.WebUI.ViewModels
{
    public class TransferViewModel
    {
        public int Id { get; set; }
        public string TransferBarcode { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string SourceWarehouse { get; set; }
        public string DestinationWarehouse { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
    }
}
