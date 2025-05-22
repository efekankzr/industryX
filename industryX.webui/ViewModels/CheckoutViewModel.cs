using IndustryX.Domain.Entities;

namespace IndustryX.WebUI.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; }

        public decimal Total { get; set; }

        public List<UserAddress>? SavedAddresses { get; set; }

        public int? SelectedShippingAddressId { get; set; }
        public int? SelectedBillingAddressId { get; set; }

        // Custom address entry
        public AddressInputModel? CustomShippingAddress { get; set; }
        public AddressInputModel? CustomBillingAddress { get; set; }

        public bool UseCustomShippingAddress => SelectedShippingAddressId == -1;
        public bool UseCustomBillingAddress => SelectedBillingAddressId == -1;
    }

}
