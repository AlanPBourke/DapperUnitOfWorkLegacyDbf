//https://ianrufus.com/blog/2017/04/c-unit-of-work-pattern-with-dapper/
using Dapper;
using Dapper.FluentMap;
using DapperUnitOfWorkLegacyDbf.EntityMaps;
using DapperUnitOfWorkLegacyDbf.Repositories;
using System.Data;
using System.Data.OleDb;

namespace DapperUnitOfWorkLegacyDbf.Dapper;

public class DapperUnitOfWork : IDisposable
{
    private IDbConnection _connection;
    private IDbTransaction _transaction;
    private string _connectionString { get; } = string.Empty;

    private CustomerRepository? _customerRepository;
    public CustomerRepository CustomerRepository
    {
        get
        {
            return _customerRepository ?? (_customerRepository = new CustomerRepository(_transaction));
        }
    }

    private CustomerTransactionRepository? _customerTransactionRepository;
    public CustomerTransactionRepository CustomerTransactionRepository
    {
        get
        {
            return _customerTransactionRepository ?? (_customerTransactionRepository = new CustomerTransactionRepository(_transaction));
        }
    }

    public DapperUnitOfWork(string connString)
    {
        _connectionString = connString;

        if (FluentMapper.EntityMaps.Any(m => m.Key == typeof(Entities.Customer)) == false)
        {
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new CustomerEntityMap());
                config.AddMap(new CustomerTransactionEntityMap());
            });
        }

        _connection = new OleDbConnection(_connectionString);
        _connection.Open();
        var cmd = $"set null off{Environment.NewLine}set exclusive off{Environment.NewLine}set deleted on{Environment.NewLine}";
        //cmd += $"end transaction";
        _connection.Execute(cmd);
        _transaction = _connection.BeginTransaction();
    }

    public void Commit()
    {
        try
        {
            _transaction.Commit();
        }
        catch
        {
            _transaction.Rollback();
            throw;
        }
        finally
        {

            _transaction.Dispose();
            _transaction = _connection.BeginTransaction();
            ResetRepositories();
        }
    }

    public void Rollback()
    {
        if (_transaction is not null)
        {
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = _connection.BeginTransaction();
            ResetRepositories();
        }
    }

    public void Dispose()
    {

        if (_transaction is not null)
        {
            _transaction.Dispose();
        }

        if (_connection is not null)
        {
            _connection.Dispose();
        }
    }

    private void ResetRepositories()
    {
        _customerRepository = null;
    }
}
