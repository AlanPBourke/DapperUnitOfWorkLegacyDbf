# DapperUnitOfWorkLegacyDbf

This application demonstrates how to perform atomic create, update and delete operations on typical Customer and Customer Transaction tables in a one-to-many relationship, using the Dapper micro-ORM and FluentMap and a Repository\Unit Of Work pattern. The database is comprised of DBF files and is accessed using the Visual FoxPro OleDb driver. There are no rules, triggers or similar implemented at database level.

As examples some integration tests using XUnit are provided in the 'Tests' project, and there is also a simple console application in the 'SimpleExample' project.

## The Unit Of Work And Repository Patterns

The Unit Of Work pattern allows database create, update and delete operations to be performed or rolled back as a single transaction, enabling database 'atomicity' where all updates occur, or none occur.

A repository pattern isolates database operations from the user interface and allows database operations to be performed by adding, updating or deleting items from a collection of objects.

There are two repository classes in the application, ```CustomerRepositoty``` and ```CustomerTransactionRepository```. . Each is passed a parameter of type **IDbConnection** through the constructor. The database connection to use is then retrieved from that parameter:

```
private IDbConnection _connection { get => transaction.Connection!; }
```

Note the null-forgiving '!' operator. This dependency injection of course makes the class database provider independent. The class then contains various methods for the database CRUD operations

In this application the unit of work is represented by the ```DapperUnitOfWork``` class. This is a class implementing **IDisposable**. It has instances of ea

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

```
public class Customer
{
    [Key]
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Address1 { get; set; } = string.Empty;

    public string? Address2 { get; set; } = string.Empty;

    public string? Postcode { get; set; }

    [Write(false)]
    public float Balance { get; set; }

    public override string ToString()
    {
        return $"{Code} {Name}";
    }
}
```

Dapper then provides the ability to do things like:

```
    public Customer GetByCode(string code)
    {
        var cmd = @"select cu_code, cu_name, cu_addr1, cu_addr2, cu_postcode, cu_balance ";
        cmd += "from Customers where cu_code = ?";
        return _connection.QueryFirstOrDefault<Customer>(cmd, param: new { c = code }, transaction);
    }
```

Note the way that query parameters are implemented - OleDB does not support named parameters, only positional parameters. So where multiple parameters are used the order is vital:

```
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

```
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


