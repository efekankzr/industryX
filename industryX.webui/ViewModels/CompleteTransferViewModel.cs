using System.ComponentModel.DataAnnotations;

namespace IndustryX.WebUI.ViewModels
{
    public class CompleteTransferViewModel
    {
        [Required]
        public string Barcode { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Box count must be at least 1.")]
        public int ReceivedBoxCount { get; set; }

        // Readonly bilgiler
        public string ProductName { get; set; }
        public int TotalBoxCount { get; set; }
        public string SourceWarehouse { get; set; }
        public string DestinationWarehouse { get; set; }
    }

}
