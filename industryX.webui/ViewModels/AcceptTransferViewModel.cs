using System.ComponentModel.DataAnnotations;

namespace IndustryX.WebUI.ViewModels
{
    public class AcceptTransferViewModel
    {
        [Required]
        public string TransferBarcode { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Box count must be at least 1")]
        public int DeliveredBoxCount { get; set; }

        // Read-only display properties
        public string ProductName { get; set; }
        public int QuantityBox { get; set; }
        public string SourceWarehouse { get; set; }
        public string DestinationWarehouse { get; set; }
    }

}
