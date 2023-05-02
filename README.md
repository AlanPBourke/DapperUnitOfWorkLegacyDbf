# DapperUnitOfWorkLegacyDbf

A simple illustration of how the Dapper micro-ORM and FluentMap can be used with a Repository and Unit Of Work pattern to perform CRUD operations on a DBF database via OLEDB.

As examples some integration tests using XUnit are provided in the 'Tests' project, and there is also a simple console application in the 'SimpleExample' project.

## The Unit Of Work And Repository Patterns

The Unit Of Work pattern allows database create, update and delete operations to be performed or rolled back as a single transaction, enabling database 'atomicity' where all updates occur, or none occur.

A repository pattern isolates database operations from the user interface and allows database operations to be performed by adding, updating or deleting items from a collection of objects.

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


