using System.Globalization;
using IndustryX.Domain.Entities;
using IndustryX.Services.Interfaces;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Extensions.Options;

namespace IndustryX.Services.Abstract
{
    public class IyzicoService : IIyzicoService
    {
        private readonly Iyzipay.Options _options;

        public IyzicoService(IOptions<IyzicoOptions> iyzicoOptions)
        {
            _options = new Iyzipay.Options
            {
                ApiKey = iyzicoOptions.Value.ApiKey,
                SecretKey = iyzicoOptions.Value.SecretKey,
                BaseUrl = iyzicoOptions.Value.BaseUrl
            };
        }

        public async Task<string> CreatePaymentRequestAsync(Order order)
        {
            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.EN.ToString(),
                ConversationId = order.Id.ToString(),
                Price = FormatPrice(order.TotalPrice),
                PaidPrice = FormatPrice(order.TotalPrice),
                Currency = Currency.TRY.ToString(),
                BasketId = order.Id.ToString(),
                CallbackUrl = $"https://localhost:7287/payment/callback?orderId={order.Id}",

                Buyer = new Buyer
                {
                    Id = order.UserId,
                    Name = order.BillingAddress.FirstName,
                    Surname = order.BillingAddress.LastName,
                    Email = order.User?.Email ?? "customer@example.com",
                    IdentityNumber = "11111111111", // Dummy TCKN
                    RegistrationAddress = order.BillingAddress.FullAddress,
                    Ip = "127.0.0.1", // Gerçek projede kullanıcı IP'si alınabilir
                    Country = order.BillingAddress.Country,
                    City = order.BillingAddress.City
                },

                ShippingAddress = new Address
                {
                    ContactName = order.ShippingAddress.FullName,
                    City = order.ShippingAddress.City,
                    Country = order.ShippingAddress.Country,
                    Description = order.ShippingAddress.FullAddress
                },

                BillingAddress = new Address
                {
                    ContactName = order.BillingAddress.FullName,
                    City = order.BillingAddress.City,
                    Country = order.BillingAddress.Country,
                    Description = order.BillingAddress.FullAddress
                },

                BasketItems = order.OrderItems.Select(item => new BasketItem
                {
                    Id = item.SalesProductId.ToString(),
                    Name = item.SalesProduct?.Name ?? "Product",
                    Category1 = "General",
                    ItemType = BasketItemType.PHYSICAL.ToString(),
                    Price = FormatPrice(item.TotalPrice)
                }).ToList()
            };

            var checkoutForm = await Task.Run(() => CheckoutFormInitialize.Create(request, _options));

            if (checkoutForm == null || checkoutForm.Status != "success" || string.IsNullOrWhiteSpace(checkoutForm.CheckoutFormContent))
            {
                var error = checkoutForm?.ErrorMessage ?? "Unknown error";
                return $"<p style='color:red'>Payment creation failed: {error}</p>";
            }

            return checkoutForm.CheckoutFormContent!;
        }

        private string FormatPrice(decimal price)
        {
            return price.ToString("F2", CultureInfo.InvariantCulture);
        }
    }

    public class IyzicoOptions
    {
        public string ApiKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string BaseUrl { get; set; } = null!;
    }
}
