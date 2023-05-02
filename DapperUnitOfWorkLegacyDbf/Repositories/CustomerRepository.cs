using Dapper;
using DapperUnitOfWorkLegacyDbf.Entities;
using System.Data;

namespace DapperUnitOfWorkLegacyDbf.Repositories;

public class CustomerRepository
{
    private readonly IDbTransaction databaseTransaction;

    public CustomerRepository(IDbTransaction t)
    {
        databaseTransaction = t;
    }

    private IDbConnection _connection { get => databaseTransaction.Connection!; }

    public Customer GetByCode(string code)
    {
        var cmd = @"select cu_code, cu_name, cu_addr1, cu_addr2, cu_postcode, cu_balance ";
        cmd += "from Customers where cu_code = ?";
        return _connection.QueryFirstOrDefault<Customer>(cmd, param: new { c = code }, databaseTransaction);
    }

    public List<Customer> GetAll()
    {
        var cmd = @"select cu_code, cu_name, cu_addr1, cu_addr2, cu_postcode, cu_balance ";
        cmd += "from Customers ";
        return _connection.Query<Customer>(cmd, transaction: databaseTransaction).ToList();
    }

    public void Update(Customer customer)
    {
        var cmd = @"update Customers set cu_name=?, cu_addr1=?, cu_addr2=?, cu_postcode=? where cu_code=?";
        _connection.ExecuteScalar(cmd, param: new
        {
            n = customer.Name,
            add1 = customer.Address1,
            add2 = customer.Address2,
            pc = customer.Postcode,
            acc = customer.Code,
        },
        databaseTransaction);
    }

    public void Add(Customer customer)
    {
        var cmd = @"insert into Customers (cu_code, cu_name, cu_addr1, cu_addr2, cu_postcode) ";
        cmd += "values (?, ?, ?, ?, ?)";
        _connection.ExecuteScalar(cmd, param: new
        {
            acc = customer.Code,
            n = customer.Name,
            add1 = customer.Address1,
            add2 = customer.Address2,
            pc = customer.Postcode,
        },
        databaseTransaction);
    }

    public bool Delete(string customerCode)
    {
        var cmd = @"select count(id) where tx_cust=? from CustomerTransactions";
        var transactionCount = _connection.ExecuteScalar<int>(cmd, param: new { acc = customerCode }, transaction: databaseTransaction);
        if (transactionCount > 0)
        {
            return false;
        }

        cmd = @"delete from Customers where cu_code=?";
        _connection.Execute(cmd, transaction: databaseTransaction, param: new { c = customerCode });
        return true;
    }
}
