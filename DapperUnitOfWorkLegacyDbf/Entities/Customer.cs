using Dapper.Contrib.Extensions;

namespace DapperUnitOfWorkLegacyDbf.Entities;

public class Customer
{
    [Key]
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address1 { get; set; } = string.Empty;

    public string? Address2 { get; set; } = string.Empty;

    public string? Postcode { get; set; }

    /// <summary>
    /// Gets or sets the customer balance. Not writable. It can only
    /// be updated by inserting, deleting or updating a
    /// transaction or transactions for this customer.
    /// </summary>
    [Write(false)]
    public float Balance { get; set; }

    public override string ToString()
    {
        return $"{Code} {Name}";
    }
}