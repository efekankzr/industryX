namespace IndustryX.WebUI.Models
{
    public class AlertMessage
    {
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string AlertType { get; set; } = "info"; // success, danger, warning, info
    }
}
