# DapperUnitOfWorkLegacyDbf

This application demonstrates how to perform atomic create, update and delete operations on typical Customer and Customer Transaction tables in a one-to-many relationship, using the Dapper micro-ORM and FluentMap and a Repository\Unit Of Work pattern. The database is comprised of DBF files and is accessed using the Visual FoxPro OleDb driver. There are no rules, triggers or similar implemented at database level.

By way of usage examples some integration tests using XUnit are provided in the 'Tests' project, and there is also a simple console application in the 'SimpleExample' project.

***Important!***
This application requires the [Microsoft Visual FoxPro 9.0 OleDb Provider](https://github.com/VFPX/VFP9SP2Hotfix3/blob/master/OLEDB_Release_Notes.md). This is a 32-bit only provider, there is no 64-bit version. As a result this application must be compiled for x86 only.

## Dapper And FluentMap

### Dapper
[Dapper](https://github.com/DapperLib/Dapper) is a popular micro-ORM \ object mapper that extends IDbConnection with convenience methods that return database results mapped to entity types.

Here's a DBF-format database table:

```
   Field   Field Name                  Type                                  Width
       1   CU_CODE                     Character                                10
       2   CU_NAME                     Character                                50
       3   CU_ADDR1                    Character                                50
       4   CU_ADDR2                    Character                                50
       5   CU_POSTCODE                 Character                                10
       6   CU_BALANCE                  Numeric                                  12
```

and here's the C# entity representing it.

```csharp
public class Customer
{
    [Key]
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address1 { get; set; } = string.Empty;

    public string? Address2 { get; set; } = string.Empty;

    public string? Postcode { get; set; }

    /// <summary>
    /// Gets or sets the customer balance. Not writable. It can only
    /// be updated by inserting, deleting or updating a
    /// transaction or transactions for this customer.
    /// </summary>
    [Write(false)]
    public float Balance { get; set; }

    public override string ToString()
    {
        return $"{Code} {Name}";
    }
}
```

Dapper then provides the ability to do things like:

```csharp
    public Customer GetByCode(string code)
    {
        var cmd = @"select cu_code, cu_name, cu_addr1, cu_addr2, cu_postcode, cu_balance ";
        cmd += "from Customers where cu_code = ?";
        return _connection.QueryFirstOrDefault<Customer>(cmd, param: new { c = code }, transaction);
    }
```

Note the way that query parameters are implemented - OleDB does not support named parameters, only positional parameters. So where multiple parameters are used the order is vital:

```csharp
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
        transaction);
    }
```    

### FluentMap
[FluentMap](https://github.com/henkmollema/Dapper-FluentMap) is a Dapper extension allowing the mapping between C# entity properties and the associated database table fields to be explicitly declared.

```csharp
public class CustomerEntityMap : EntityMap<Customer>
{
    public CustomerEntityMap()
    {
        Map(c => c.Code).ToColumn("cu_code", caseSensitive: false);
        Map(c => c.Name).ToColumn("cu_name", caseSensitive: false);
        Map(c => c.Address1).ToColumn("cu_addr1", caseSensitive: false);
        Map(c => c.Address2).ToColumn("cu_addr2", caseSensitive: false);
        Map(c => c.Postcode).ToColumn("cu_postcode", caseSensitive: false);
        Map(c => c.Balance).ToColumn("cu_balance", caseSensitive: false);
    }
}
```
## The Repository And Unit Of Work Implementations

The Unit Of Work pattern allows database create, update and delete operations to be performed or rolled back as a single transaction, enabling database 'atomicity' where all updates occur, or none occur.

A repository pattern isolates database operations from the user interface and allows database operations to be performed by adding, updating or deleting items from a collection of objects.

### Repositories
There are two repository classes in the application, ```CustomerRepositoty``` and ```CustomerTransactionRepository```. Each is passed a parameter of type **IDbConnection** through the constructor. The database connection to use is then retrieved from that parameter:

```csharp
private IDbConnection _connection { get => databaseTransaction.Connection!; }
```

Note the null-forgiving '!' operator. This dependency injection of course makes the class database provider independent. The class then contains various methods for the database CRUD operations, such as the following method that will return a List of Customer objects:

```csharp
 public List<Customer> GetAll()
 {
     var cmd = @"select cu_code, cu_name, cu_addr1, cu_addr2, cu_postcode, cu_balance ";
     cmd += "from Customers ";
     return _connection.Query<Customer>(cmd, transaction: transaction).ToList();
 }
```    

### Unit Of Work

In this application the unit of work is represented by the ```DapperUnitOfWork``` class. This is a class implementing **IDisposable**. It has instances of both types of repository. The constructor takes the connection string as a parameter and configures the FluentMap mapping if it has not been already. It then opens an OleDb connection and starts a new transaction.

```csharp
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
```

The repository properties on the class get the database transaction injected in their getter, the code below will either return an existing repository or create a new one as required:

```csharp
 public CustomerRepository? CustomerRepository
 {
     get
     {
         return customerRepository ??= new CustomerRepository(dbTransaction);
     }
 }
```

The ```Commit()``` method on the class attempts to commit the current transaction. Any exception will cause a rollback, and the exception will be thrown up. There is also a ```Rollback()``` method that can be used to explicitly roll the transaction back. In all eventualities the current transaction will be disposed and a new one created, and the repository members reset.

```csharp
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
```    

Because both the ```Customer``` and ```CustomerTransaction``` repository objects are using the same transaction, the commit or rollback are atomic and represent one unit of work.

Both ```Commit()``` and ```Rollback()``` methods will explicitly call the ```Dispose()``` methof the class. This method takes care of disposing the current transaction and connection, and resetting the repository members.

```csharp
 public void Dispose()
 {
     dbTransaction?.Dispose();
     dbConnection?.Dispose();
     GC.SuppressFinalize(this);
 }
 ```
 
 ***Important***
Always disposing the transaction and connection when finished are extremely important in a file-based database such as DBF. Any file handles left open on the disk file can cause issues for other applications./

This is a simple example - the unit of work class here is kind of a 'god object' since it always has to contain an instance of each type of repository class. So it is a candidate for further abstraction.


