using Dapper;
using DapperUnitOfWorkLegacyDbf.Entities;
using System.Data;

namespace DapperUnitOfWorkLegacyDbf.Repositories
{
    public class CustomerTransactionRepository
    {
        private IDbConnection _connection { get => _transaction.Connection; }
        private IDbTransaction _transaction;

        public CustomerTransactionRepository(IDbTransaction t)
        {
            _transaction = t;
        }

        public CustomerTransaction GetById(int id)
        {
            var cmd = @"select * from customertransactions where id=?";
            // EntityMap takes care of field names to entity properties.
            return _connection.QueryFirstOrDefault<CustomerTransaction>(cmd, param: new { i = id }, _transaction);
        }

        public List<CustomerTransaction> GetAllForCustomer(string customerCode)
        {
            var cmd = @"select * from customertransactions where customercode=?";
            // EntityMap takes care of field names to entity properties.
            return _connection.Query<CustomerTransaction>(cmd, param: new { c = customerCode }, _transaction).ToList();
        }
    }
}
