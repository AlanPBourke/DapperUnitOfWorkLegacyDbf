//https://ianrufus.com/blog/2017/04/c-unit-of-work-pattern-with-dapper/
using Dapper;
using DapperUnitOfWorkLegacyDbf.Repositories;
using System.Data;
using System.Data.OleDb;

namespace DapperUnitOfWorkLegacyDbf.Dapper;

public class DapperUnitOfWork : IDisposable
{
    private IDbConnection _connection;
    private IDbTransaction _transaction;

    private CustomerRepository? _customerRepository;
    public CustomerRepository CustomerRepository
    {
        get
        {
            return _customerRepository ?? (_customerRepository = new CustomerRepository(_transaction));
        }
    }

    public DapperUnitOfWork(string connString)
    {
        _connection = new OleDbConnection(connString);
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
        if (_transaction != null)
        {
            _transaction.Rollback();
        }
    }

    public void Dispose()
    {
        if (_transaction != null)
        {
            _transaction.Dispose();
            //_transaction = null;
        }

        if (_connection != null)
        {
            _connection.Dispose();
            //_connection = null;
        }
    }

    private void ResetRepositories()
    {
        _customerRepository = null;
    }
}
