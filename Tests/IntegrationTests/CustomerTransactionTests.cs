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
        UnitOfWorkUnderTest.CustomerTransactionRepository.Add(new CustomerTransaction
        {
            CustomerCode = "ABC100",
            Reference = "ABC100 Invoice",
            Type = "I",
            Value = (float)498.99
        });

        UnitOfWorkUnderTest.CustomerTransactionRepository.Add(new CustomerTransaction
        {
            CustomerCode = "IKS220",
            Reference = "IKS220 Credit Note",
            Type = "C",
            Value = (float)-29.50
        });

        UnitOfWorkUnderTest.Commit();
        var t1 = UnitOfWorkUnderTest.CustomerTransactionRepository.GetForCustomer("ABC100");
        var t2 = UnitOfWorkUnderTest.CustomerTransactionRepository.GetForCustomer("IKS220");
        Assert.Equal(7, t1.Count() + t2.Count());
    }
}