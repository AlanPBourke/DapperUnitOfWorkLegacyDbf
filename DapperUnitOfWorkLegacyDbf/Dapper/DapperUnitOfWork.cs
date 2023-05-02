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
    private readonly IDbConnection databaseConnection;
    private IDbTransaction databaseTransaction;

    /// <summary>
    /// Initializes a new instance of the <see cref="DapperUnitOfWork"/> class.
    /// Sets up the unit of work and configures the FluentMap mappings. 
    /// Opens the OleDb connection.
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

        databaseConnection = new OleDbConnection(ConnectionString);
        databaseConnection.Open();

        // Some default setup items for the connection.
        // 'Set null off' - any inserts will insert the relevant empty value for the database field type instead of a null
        // where a value is not supplied.
        // 'set exclusive off' - tables will be opened in shared mode.
        // 'set deleted on' - unintuitively this means that table rows marked as deleted will be ignored in SELECTs.
        var cmd = $"set null off{Environment.NewLine}set exclusive off{Environment.NewLine}set deleted on{Environment.NewLine}";
        databaseConnection.Execute(cmd);

        databaseTransaction = databaseConnection.BeginTransaction();
    }

    /// <summary>
    /// Gets the customer transaction repository.
    /// </summary>
    public CustomerTransactionRepository? CustomerTransactionRepository
    {
        get
        {
            return _customerTransactionRepository ??= new CustomerTransactionRepository(databaseTransaction);
        }
    }

    /// <summary>
    /// Gets the customer repository.
    /// </summary>
    public CustomerRepository? CustomerRepository
    {
        get
        {
            return _customerRepository ??= new CustomerRepository(databaseTransaction);
        }
    }

    private CustomerTransactionRepository? _customerTransactionRepository { get; set; }

    private CustomerRepository? _customerRepository { get; set; }

    private string ConnectionString { get; set; }

    /// <summary>
    /// Will attempt a commit of the current transaction.
    /// </summary>
    public void Commit()
    {
        try
        {
            databaseTransaction.Commit();
        }
        catch
        {
            databaseTransaction.Rollback();
            throw;
        }
        finally
        {
            databaseTransaction.Dispose();
            databaseTransaction = databaseConnection.BeginTransaction();
            ResetRepositories();
        }
    }

    public void Rollback()
    {
        if (databaseTransaction is not null)
        {
            databaseTransaction.Rollback();
            databaseTransaction.Dispose();
            databaseTransaction = databaseConnection.BeginTransaction();
            ResetRepositories();
        }
    }

    public void Dispose()
    {
        databaseTransaction?.Dispose();
        databaseConnection?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ResetRepositories()
    {
        _customerRepository = null;
        _customerTransactionRepository = null;
    }
}
