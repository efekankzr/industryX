namespace IndustryX.Domain.ValueObjects
{
    public class OrderAddress
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public string District { get; set; } = null!;
        public string FullAddress { get; set; } = null!;

        // Kullanıcı adı ve soyadı birleştirilmiş hali
        public string FullName => $"{FirstName} {LastName}";
    }
}
