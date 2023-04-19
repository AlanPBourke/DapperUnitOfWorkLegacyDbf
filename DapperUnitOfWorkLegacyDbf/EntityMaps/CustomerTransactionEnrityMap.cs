using Dapper.FluentMap.Mapping;
using DapperUnitOfWorkLegacyDbf.Entities;

namespace DapperUnitOfWorkLegacyDbf.EntityMaps;

public class CustomerTransactionEntityMap : EntityMap<CustomerTransaction>
{
    public CustomerTransactionEntityMap()
    {
        Map(t => t.CustomerCode).ToColumn("tx_cust", caseSensitive: false);
        Map(t => t.Reference).ToColumn("tx_ref", caseSensitive: false);
        Map(t => t.Type).ToColumn("tx_type", caseSensitive: false);
        Map(t => t.Value).ToColumn("tx_value", caseSensitive: false);
        Map(t => t.Id).ToColumn("id", caseSensitive: false);
    }
}
