using Dapper.Contrib.Extensions;

namespace DapperUnitOfWorkLegacyDbf.Entities;

public class CustomerTransaction
{
    [Key]
    [Write(false)]
    public int Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public float? Value { get; set; }

}
