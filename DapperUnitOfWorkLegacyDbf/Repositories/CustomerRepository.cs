using Dapper;
using DapperUnitOfWorkLegacyDbf.Entities;
using System.Data;

namespace DapperUnitOfWorkLegacyDbf.Repositories;

public class CustomerRepository
{
    private IDbConnection _connection { get => _transaction.Connection; }
    private IDbTransaction _transaction;

    public CustomerRepository(IDbTransaction t)
    {
        _transaction = t;
    }

    public Customer GetByCode(string code)
    {
        var cmd = @"select cu_code, cu_name, cu_addr1, cu_addr2, cu_postcode, cu_balance ";
        cmd += "from Customers where cu_code = ?";
        // EntityMap takes care of field names to entity properties.
        return _connection.QueryFirstOrDefault<Customer>(cmd, param: new { c = code }, _transaction);
    }

    public List<Customer> GetAll()
    {
        var cmd = @"select cu_code, cu_name, cu_addr1, cu_addr2, cu_postcode, cu_balance ";
        cmd += "from Customers ";
        return _connection.Query<Customer>(cmd, transaction: _transaction).ToList();
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
            acc = customer.Code
        },
        _transaction);
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
            pc = customer.Postcode

        },
        _transaction);
    }

    public void Delete(string code)
    {
        var cmd = @"delete from Customers where cu_code=?";
        _connection.Execute(cmd, transaction: _transaction, param: new { c = code });
    }

}
