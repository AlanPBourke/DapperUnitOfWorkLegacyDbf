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

        public CustomerTransaction? GetById(int id)
        {
            var cmd = @"select tx_cust, tx_type, tx_ref, tx_value, id from customertransactions where id=?";

            try
            {
                return _connection.QuerySingle<CustomerTransaction>(cmd, param: new { i = id }, _transaction);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<CustomerTransaction> GetForCustomer(string customerCode)
        {
            var cmd = @"select tx_cust, tx_type, tx_ref, tx_value, id from customertransactions where tx_cust=?";
            return _connection.Query<CustomerTransaction>(cmd, param: new { c = customerCode }, _transaction).ToList();
        }

        public void Add(CustomerTransaction customerTransaction)
        {
            var cmd = @"insert into CustomerTransactions (tx_cust, tx_type, tx_ref, tx_value) ";
            cmd += "values (?, ?, ?, ?)";
            _connection.ExecuteScalar(cmd, param: new
            {
                acc = customerTransaction.CustomerCode,
                type = customerTransaction.Type,
                reference = customerTransaction.Reference,
                value = customerTransaction.Value
            },
            _transaction);

            cmd = @"update Customers where cu_code = ? set cu_balance=cu_balance + ?";
            _connection.ExecuteScalar(cmd, param: new
            {
                acc = customerTransaction.CustomerCode,
                value = customerTransaction.Value
            },
            _transaction);
        }
    }
}
