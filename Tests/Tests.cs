using DapperUnitOfWorkLegacyDbf.Dapper;
using DapperUnitOfWorkLegacyDbf.Entities;
using Xunit.Abstractions;
using Xunit.Priority;

namespace Tests;

public class IntegrationTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private string SampleDbcFilename { get; set; } = string.Empty;
    private string ConnString { get; set; } = string.Empty;

    public IntegrationTests(ITestOutputHelper o)
    {
        SampleDbcFilename = TestHelpers.GetTemporaryDbcFilename();
        ConnString = $"Provider=vfpoledb;Data Source={SampleDbcFilename};Collating Sequence=machine;Mode=Share Deny None;";
        this.output = o;
    }

    [Fact, Priority(1)]
    public void CustomerCountShouldReturnTwo()
    {
        TestHelpers.CreateAndSeedSampleDatabase(SampleDbcFilename);
        ConnString = $"Provider=vfpoledb;Data Source={SampleDbcFilename};Collating Sequence=machine;Mode=Share Deny None;";
        DapperUnitOfWork u = new(ConnString);
        var custs = u.CustomerRepository.GetAll();
        Assert.Equal(2, custs.Count());
    }

    [Fact, Priority(2)]
    public void AddCustomer()
    {

        TestHelpers.CreateAndSeedSampleDatabase(SampleDbcFilename);
        DapperUnitOfWork u = new(ConnString);
        u.CustomerRepository.Add(new Customer
        {
            Code = "XXX999",
            Address1 = "address 1",
            Address2 = "address 2",
            Postcode = "postcode"
        });
        u.Commit();
        //u = new(ConnString);
        var custs = u.CustomerRepository.GetAll();
        TestHelpers.DeleteTemporaryData(Path.GetDirectoryName(SampleDbcFilename));
        Assert.Equal(3, custs.Count());
    }

    [Fact, Priority(98)]
    public void Delete_Rollback_Customer_CustomerCountShouldReturnTwo()
    {
        DapperUnitOfWork u = new(ConnString);
        u.CustomerRepository.Delete("IKS220");
        u.Rollback();
        u = new(ConnString);
        var custs = u.CustomerRepository.GetAll();
        Assert.Equal(2, custs.Count());
    }

    [Fact, Priority(99)]
    public void Delete_Commit_Customer_CustomerCountShouldReturnOne()
    {
        DapperUnitOfWork u = new(ConnString);
        u.CustomerRepository.Delete("IKS220");
        u.Commit();
        var custs = u.CustomerRepository.GetAll();
        Assert.Single<Customer>(custs);
    }

    public void Dispose()
    {
    }
}