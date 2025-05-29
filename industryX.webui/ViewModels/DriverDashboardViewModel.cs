namespace IndustryX.WebUI.ViewModels
{
    public class DriverDashboardViewModel
    {
        public List<TransferViewModel> TransfersToPickup { get; set; } = new();
        public List<UserDeficitViewModel> Deficits { get; set; } = new();
    }
}
