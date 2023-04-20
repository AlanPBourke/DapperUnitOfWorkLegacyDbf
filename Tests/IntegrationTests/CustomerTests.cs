using DapperUnitOfWorkLegacyDbf.Dapper;
using DapperUnitOfWorkLegacyDbf.Entities;
using Xunit.Abstractions;

namespace Tests;

public class CustomerTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private string SampleDbcFilename { get; set; } = string.Empty;
    private string ConnString { get; set; } = string.Empty;
    private DapperUnitOfWork UnitOfWorkUnderTest { get; set; }

    public CustomerTests(ITestOutputHelper o)
    {
        this.output = o;
        SampleDbcFilename = TestHelpers.GetTemporaryDbcFilename();
        ConnString = $"Provider=vfpoledb;Data Source={SampleDbcFilename};Collating Sequence=machine;Mode=Share Deny None;";
        TestHelpers.CreateAndSeedSampleDatabase(SampleDbcFilename);
        UnitOfWorkUnderTest = new(ConnString);
    }

    public void Dispose()
    {
        UnitOfWorkUnderTest.Dispose();
        TestHelpers.DeleteTemporaryData(Path.GetDirectoryName(SampleDbcFilename));
    }

    [Fact]
    public void Customer_CountShouldReturnTwo()
    {
        var custs = UnitOfWorkUnderTest.CustomerRepository.GetAll();
        Assert.Equal(2, custs.Count());
    }

    [Fact]
    public void Customer_GetByCode_CodeExists()
    {
        var cust = UnitOfWorkUnderTest.CustomerRepository.GetByCode("IKS220");
        Assert.True(cust is not null);
        Assert.Equal("IKS220", cust.Code.TrimEnd());
    }

    [Fact]
    public void Customer_GetByCode_CodeDoesNotExist_ShouldBeNull()
    {
        var cust = UnitOfWorkUnderTest.CustomerRepository.GetByCode("XXX999");
        Assert.True(cust is null);
    }

    [Fact]
    public void Customer_AddTwo_Commit()
    {
        UnitOfWorkUnderTest.CustomerRepository.Add(new Customer
        {
            Code = "XXX999",
            Address1 = "999 address 1",
            Address2 = "999 address 2",
            Postcode = "999 postcode"
        });

        UnitOfWorkUnderTest.CustomerRepository.Add(new Customer
        {
            Code = "XXX998",
            Address1 = "998 address 1",
            Address2 = "998 address 2",
            Postcode = "998 postcode"
        });

        UnitOfWorkUnderTest.Commit();
        var custs = UnitOfWorkUnderTest.CustomerRepository.GetAll();
        Assert.Equal(4, custs.Count());
    }

    [Fact]
    public void Customer_AddTwo_Rollback()
    {
        UnitOfWorkUnderTest.CustomerRepository.Add(new Customer
        {
            Code = "XXX999",
            Address1 = "999 address 1",
            Address2 = "999 address 2",
            Postcode = "999 postcode"
        });

        UnitOfWorkUnderTest.CustomerRepository.Add(new Customer
        {
            Code = "XXX998",
            Address1 = "998 address 1",
            Address2 = "998 address 2",
            Postcode = "998 postcode"
        });

        UnitOfWorkUnderTest.Rollback();
        var custs = UnitOfWorkUnderTest.CustomerRepository.GetAll();
        Assert.Equal(2, custs.Count());
    }

    [Fact]
    public void Customer_Delete_Rollback_CountShouldReturnTwo()
    {
        UnitOfWorkUnderTest.CustomerRepository.Delete("IKS220");
        UnitOfWorkUnderTest.Rollback();
        var custs = UnitOfWorkUnderTest.CustomerRepository.GetAll();
        Assert.Equal(2, custs.Count());
    }

    [Fact]
    public void Customer_Delete_Commit_CountShouldReturnOne()
    {
        UnitOfWorkUnderTest.CustomerRepository.Delete("IKS220");
        UnitOfWorkUnderTest.Commit();
        var custs = UnitOfWorkUnderTest.CustomerRepository.GetAll();
        Assert.Single(custs);
    }


}