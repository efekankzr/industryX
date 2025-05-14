using IndustryX.Domain.Entities;

public class ProductTransferDeficit
{
    public int Id { get; set; }

    public int ProductTransferId { get; set; }
    public ProductTransfer ProductTransfer { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; }

    public int DeficitQuantity { get; set; }
}