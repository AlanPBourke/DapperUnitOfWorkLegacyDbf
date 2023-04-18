using Dapper.FluentMap.Mapping;
using DapperUnitOfWorkLegacyDbf.Entities;

namespace DapperUnitOfWorkLegacyDbf.EntityMaps;

public class CustomerEntityMap : EntityMap<Customer>
{
    public CustomerEntityMap()
    {
        Map(c => c.Code).ToColumn("cu_code", caseSensitive: false);
        Map(c => c.Name).ToColumn("cu_name", caseSensitive: false);
        Map(c => c.Address1).ToColumn("cu_addr1", caseSensitive: false);
        Map(c => c.Address2).ToColumn("cu_addr2", caseSensitive: false);
        Map(c => c.Postcode).ToColumn("cu_postcode", caseSensitive: false);
        Map(c => c.Balance).ToColumn("cu_balance", caseSensitive: false);
    }
}
