using System.ComponentModel.DataAnnotations;

namespace IndustryX.WebUI.ViewModels
{
    public class AcceptTransferViewModel
    {
        [Required]
        [Display(Name = "Transfer Barcode")]
        public string TransferBarcode { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid box count.")]
        [Display(Name = "Delivered Box Count")]
        public int DeliveredBoxCount { get; set; }
    }
}
