// See https://aka.ms/new-console-template for more information
using DapperUnitOfWorkLegacyDbf.Dapper;
using DapperUnitOfWorkLegacyDbf.Entities;
using System.Reflection;

namespace SimpleExample;

class SimpleExample
{
    static int Main(string[] args)
    {
        string? assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyLocation is null)
        {
            Console.WriteLine("Couldn't resolve the database location.");
            return -1;
        }

        var dbcFileName = Path.Combine(assemblyLocation, @"database\sample.dbc");
        var connString = $"Provider=vfpoledb;Data Source='{dbcFileName}';Collating Sequence=machine;Mode=Share Deny None;";

        var Dapper = new DapperUnitOfWork(connString);

        Dapper.CustomerRepository.Add(new Customer
        {
            Code = "CUST001",
            Name = "Bourke International",
            Address1 = "1 The Larches",
            Address2 = "Larchton",
            Postcode = "S65 2JP"
        });

        Dapper.Commit();

        Dapper.CustomerTransactionRepository.Add(new CustomerTransaction
        {
            CustomerCode = "CUST001",
            Reference = "Invoice 1",
            Type = "I",
            Value = (float)129.99
        });

        Dapper.CustomerTransactionRepository.Add(new CustomerTransaction
        {
            CustomerCode = "CUST001",
            Reference = "Invoice 2",
            Type = "I",
            Value = (float)29.50
        });

        Dapper.CustomerTransactionRepository.Add(new CustomerTransaction
        {
            CustomerCode = "CUST001",
            Reference = "Credit Note 1",
            Type = "C",
            Value = (float)-12.99
        });

        Dapper.Commit();

        var customer = Dapper.CustomerRepository.GetByCode("CUST001");
        var transactions = Dapper.CustomerTransactionRepository.GetForCustomer("CUST001");

        Console.WriteLine($"{customer.Code}\n{customer.Name}\n{customer.Address1}\n{customer.Address2}\n");

        foreach (var t in transactions)
        {
            Console.WriteLine($"{t.Reference} {(t.Type == "I" ? "Invoice" : "Credit")} {t.Value}");
        }

        Console.WriteLine($"\nBalance: {customer.Balance}");


        return 0;
    }

}


