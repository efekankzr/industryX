namespace IndustryX.WebUI.ViewModels.ApiViewModels
{
    public class VehicleLocationUpdateDto
    {
        public string DeviceId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Status { get; set; }
    }
}
