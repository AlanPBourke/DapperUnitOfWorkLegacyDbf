using Dapper;
using DapperUnitOfWorkLegacyDbf.Entities;
using System.Data;

namespace DapperUnitOfWorkLegacyDbf.Repositories
{
    public class CustomerTransactionRepository
    {
        private readonly IDbTransaction transaction;

        public CustomerTransactionRepository(IDbTransaction t)
        {
            transaction = t;
        }

        private IDbConnection _connection { get => transaction.Connection!; }

        public CustomerTransaction? GetById(int id)
        {
            var cmd = @"select tx_cust, tx_type, tx_ref, tx_value, id from customertransactions where id=?";

            try
            {
                return _connection.QuerySingle<CustomerTransaction>(cmd, param: new { i = id }, transaction);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<CustomerTransaction> GetForCustomer(string customerCode)
        {
            var cmd = @"select tx_cust, tx_type, tx_ref, tx_value, id from customertransactions where tx_cust=?";
            return _connection.Query<CustomerTransaction>(cmd, param: new { c = customerCode }, transaction).ToList();
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
                value = customerTransaction.Value,
            },
            transaction);

            cmd = @"update Customers where cu_code = ? set cu_balance=cu_balance + ?";
            _connection.ExecuteScalar(cmd, param: new
            {
                acc = customerTransaction.CustomerCode,
                value = customerTransaction.Value,
            },
            transaction);
        }
    }
}
