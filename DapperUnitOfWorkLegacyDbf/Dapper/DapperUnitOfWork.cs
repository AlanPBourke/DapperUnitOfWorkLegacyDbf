using Dapper;
using Dapper.FluentMap;
using DapperUnitOfWorkLegacyDbf.EntityMaps;
using DapperUnitOfWorkLegacyDbf.Repositories;
using System.Data;
using System.Data.OleDb;

namespace DapperUnitOfWorkLegacyDbf.Dapper;

/// <summary>
/// A simple implementation of a unit of work pattern.
/// <see cref="https://ianrufus.com/blog/2017/04/c-unit-of-work-pattern-with-dapper/">Refer to Ian Rufus' blog.</see>.
/// </summary>
public class DapperUnitOfWork : IDisposable
{
    private readonly IDbConnection dbConnection;
    private IDbTransaction dbTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperUnitOfWork"/> class.
    /// Sets up the FluentMapper mappings and opens the OleDb connection.
    /// </summary>
    /// <param name="connString">The OleDb connection string.</param>
    public DapperUnitOfWork(string connString)
    {
        ConnectionString = connString;

        if (!FluentMapper.EntityMaps.Any(m => m.Key == typeof(Entities.Customer)))
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

    /// <summary>
    /// Gets the customer transaction repository.
    /// </summary>
    public CustomerTransactionRepository? CustomerTransactionRepository
    {
        get
        {
            return _customerTransactionRepository ??= new CustomerTransactionRepository(dbTransaction);
        }
    }

    /// <summary>
    /// Gets the customer repository.
    /// </summary>
    public CustomerRepository? CustomerRepository
    {
        get
        {
            return customerRepository ??= new CustomerRepository(dbTransaction);
        }
    }

    private CustomerTransactionRepository? _customerTransactionRepository { get; set; }

    private CustomerRepository? customerRepository { get; set; }

    private string ConnectionString { get; set; }

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
        dbTransaction?.Dispose();
        dbConnection?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ResetRepositories()
    {
        customerRepository = null;
        _customerTransactionRepository = null;
    }
}
