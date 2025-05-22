using IndustryX.Domain.Entities;

namespace IndustryX.Services.Interfaces
{
    public interface IIyzicoService
    {
        Task<string> CreatePaymentRequestAsync(Order order);
    }
}
