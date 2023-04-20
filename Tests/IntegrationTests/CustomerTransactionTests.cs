using DapperUnitOfWorkLegacyDbf.Dapper;
using DapperUnitOfWorkLegacyDbf.Entities;
using Xunit.Abstractions;

namespace Tests;

public class CustomerTransactionTests : IDisposable
{
    private readonly ITestOutputHelper output;
    private string SampleDbcFilename { get; set; } = string.Empty;
    private string ConnString { get; set; } = string.Empty;
    private DapperUnitOfWork UnitOfWorkUnderTest { get; set; }

    public CustomerTransactionTests(ITestOutputHelper o)
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
    public void Transaction_GetById_IdExists_ShouldReturnOne()
    {
        var txn = UnitOfWorkUnderTest.CustomerTransactionRepository.GetById(8);
        Assert.True(txn is not null);
        Assert.Equal(8, txn.Id);
    }

    [Fact]
    public void Transaction_GetById_IdDoesNotExist_ShouldReturnNull()
    {
        var txn = UnitOfWorkUnderTest.CustomerTransactionRepository.GetById(-1);
        Assert.True(txn is null);
    }

    [Fact]
    public void Transaction_GetForCustomer_ShouldReturnThree()
    {
        var txns = UnitOfWorkUnderTest.CustomerTransactionRepository.GetForCustomer("ABC100");
        Assert.True(txns is not null);
        Assert.Equal(3, txns.Count());
    }

    [Fact]
    public void Transaction_GetForCustomer_CustomerDoesNotExist_ShouldReturnEmptyList()
    {
        var txns = UnitOfWorkUnderTest.CustomerTransactionRepository.GetForCustomer("XXX999");
        Assert.Empty(txns);
    }

    [Fact]
    public void Transaction_AddTwo_Commit()
    {
        var customer1 = UnitOfWorkUnderTest.CustomerTransactionRepository.GetForCustomer("ABC100");
        var customer2 = UnitOfWorkUnderTest.CustomerTransactionRepository.GetForCustomer("IKS220");
        var total1pre = customer1.Sum(c => c.Value);
        var total2pre = customer2.Sum(c => c.Value);
        var value1 = (float)498.99;
        var value2 = (float)1000.00;

        UnitOfWorkUnderTest.CustomerTransactionRepository.Add(new CustomerTransaction
        {
            CustomerCode = "ABC100",
            Reference = "ABC100 Invoice",
            Type = "I",
            Value = value1
        });

        UnitOfWorkUnderTest.CustomerTransactionRepository.Add(new CustomerTransaction
        {
            CustomerCode = "IKS220",
            Reference = "IKS220 Credit Note",
            Type = "C",
            Value = value2
        });

        UnitOfWorkUnderTest.Commit();
        customer1 = UnitOfWorkUnderTest.CustomerTransactionRepository.GetForCustomer("ABC100");
        customer2 = UnitOfWorkUnderTest.CustomerTransactionRepository.GetForCustomer("IKS220");
        var total1post = customer1.Sum(c => c.Value);
        var total2post = customer2.Sum(c => c.Value);
        Assert.Equal(7, customer1.Count() + customer2.Count());
        Assert.Equal(total1pre + value1 + total2pre + value2, total1post + total2post);
    }

    [Fact]
    public void Transaction_AddOne_Rollback()
    {
        UnitOfWorkUnderTest.CustomerTransactionRepository.Add(new CustomerTransaction
        {
            CustomerCode = "ABC100",
            Reference = "ABC100 Invoice",
            Type = "I",
            Value = (float)498.99
        });

        UnitOfWorkUnderTest.Rollback();
        var t1 = UnitOfWorkUnderTest.CustomerTransactionRepository.GetForCustomer("ABC100");
        Assert.Equal(3, t1.Count());
    }
}