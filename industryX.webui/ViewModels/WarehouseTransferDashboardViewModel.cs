namespace IndustryX.WebUI.ViewModels
{
    public class WarehouseTransferDashboardViewModel
    {
        public List<TransferViewModel> IncomingTransfers { get; set; } = new();
        public List<TransferViewModel> OutgoingTransfers { get; set; } = new();
        public List<UserDeficitViewModel> Deficits { get; set; } = new(); // 💡 yeni eklendi
    }
}
