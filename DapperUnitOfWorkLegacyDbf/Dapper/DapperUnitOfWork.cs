using Dapper;
using Dapper.FluentMap;
using DapperUnitOfWorkLegacyDbf.EntityMaps;
using DapperUnitOfWorkLegacyDbf.Repositories;
using System.Data;
using System.Data.OleDb;

namespace DapperUnitOfWorkLegacyDbf.Dapper;

public class DapperUnitOfWork : IDisposable
{
    private IDbConnection dbConnection;
    private IDbTransaction dbTransaction;

    private string ConnectionString { get; set; } = string.Empty;

    private CustomerTransactionRepository? customerTransactionRepository { get; set; }
    public CustomerTransactionRepository? CustomerTransactionRepository
    {
        get
        {
            return customerTransactionRepository ?? (customerTransactionRepository = new CustomerTransactionRepository(dbTransaction));
        }
    }

    private CustomerRepository? customerRepository { get; set; }
    public CustomerRepository? CustomerRepository
    {
        get
        {
            return customerRepository ?? (customerRepository = new CustomerRepository(dbTransaction));
        }
    }

    // https://ianrufus.com/blog/2017/04/c-unit-of-work-pattern-with-dapper/
    public DapperUnitOfWork(string connString)
    {
        ConnectionString = connString;

        if (FluentMapper.EntityMaps.Any(m => m.Key == typeof(Entities.Customer)) == false)
        {
            FluentMapper.Initialize(config =>
            {
                config.AddMap(new CustomerEntityMap());
                config.AddMap(new CustomerTransactionEntityMap());
            });
        }

        dbConnection = new OleDbConnection(ConnectionString);
        dbConnection.Open();
        var cmd = $"set null off{Environment.NewLine}set exclusive off{Environment.NewLine}set deleted on{Environment.NewLine}";
        dbConnection.Execute(cmd);
        dbTransaction = dbConnection.BeginTransaction();
    }

    public void Commit()
    {
        try
        {
            dbTransaction.Commit();
        }
        catch
        {
            dbTransaction.Rollback();
            throw;
        }
        finally
        {

            dbTransaction.Dispose();
            dbTransaction = dbConnection.BeginTransaction();
            ResetRepositories();
        }
    }

    public void Rollback()
    {
        if (dbTransaction is not null)
        {
            dbTransaction.Rollback();
            dbTransaction.Dispose();
            dbTransaction = dbConnection.BeginTransaction();
            ResetRepositories();
        }
    }

    public void Dispose()
    {

        if (dbTransaction is not null)
        {
            dbTransaction.Dispose();
        }

        if (dbConnection is not null)
        {
            dbConnection.Dispose();
        }
    }

    private void ResetRepositories()
    {
        customerRepository = null;
        customerTransactionRepository = null;
    }
}
