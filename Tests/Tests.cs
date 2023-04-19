using DapperUnitOfWorkLegacyDbf.Dapper;
using DapperUnitOfWorkLegacyDbf.Entities;
using Xunit.Priority;

namespace Tests;

public class IntegrationTests : IDisposable
{
    private string SampleDbcPath { get; set; } = string.Empty;
    private string ConnString { get; set; } = string.Empty;

    public IntegrationTests()
    {
        SampleDbcPath = TestHelpers.CreateAndSeedSampleDatabase();
        ConnString = $"Provider=vfpoledb;Data Source={SampleDbcPath};Collating Sequence=machine;Mode=Share Deny None;";
    }

    [Fact, Priority(1)]
    public void CustomerCountShouldReturnTwo()
    {
        TestHelpers.CreateAndSeedSampleDatabase();
        DapperUnitOfWork u = new(ConnString);
        var custs = u.CustomerRepository.GetAll();
        Assert.Equal(2, custs.Count());
    }

    [Fact, Priority(1)]
    public void AddCustomer()
    {
        TestHelpers.CreateAndSeedSampleDatabase();
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