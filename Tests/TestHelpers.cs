﻿using System.Data.OleDb;
using System.Reflection;

namespace Tests;

public static class TestHelpers
{
    private static List<string> DataBaseFileNames { get; set; } = new List<string>();

    static TestHelpers()
    {
        DataBaseFileNames.AddRange(new string[]
        {
            "sample.dcx",
            "sample.dct",
            "sample.dbc",
            "customers.dbf",
            "customers.cdx",
            "customertransactions.dbf",
            "customertransactions.cdx"
        });
    }

    public static string GetTemporaryDbcFilename()
    {
        string copyDirectory = Path.GetTempPath();
        return Path.Combine(copyDirectory, "sample.dbc");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Windows only.")]
    public static void CreateAndSeedSampleDatabase(string tempDbcFilename)
    {
        string assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        string sourceDirectory = Path.Combine(assemblyLocation, @"database");

        CopyFileList(sourceDirectory, Path.GetDirectoryName(tempDbcFilename));

        var connString = $"Provider=vfpoledb;Data Source='{tempDbcFilename}';Collating Sequence=machine;Mode=Share Deny None;";
        using OleDbConnection conn = new(connString);
        using var command = conn.CreateCommand();
        command.CommandText = $"set null off{Environment.NewLine}set exclusive off{Environment.NewLine}set deleted on{Environment.NewLine}";
        conn.Open();
        command.ExecuteNonQuery();
        command.CommandText = @"insert into customers (cu_code, cu_name, cu_addr1, cu_addr2, cu_postcode, cu_balance ) values (?, ?, ?, ?, ?, ?)";
        var pCu_code = command.Parameters.Add("@pCu_code", OleDbType.VarChar, 10);
        var pCu_name = command.Parameters.Add("@pCu_name", OleDbType.VarChar, 50);
        var pCu_addr1 = command.Parameters.Add("@pCu_addr1", OleDbType.VarChar, 50);
        var pCu_addr2 = command.Parameters.Add("@pCu_addr2", OleDbType.VarChar, 50);
        var pCu_postcode = command.Parameters.Add("@pCu_postcode", OleDbType.VarChar, 50);
        var pCu_balance = command.Parameters.Add("@pCu_balance", OleDbType.Decimal, 12);

        pCu_code.Value = "ABC100";
        pCu_name.Value = "ABC Limited";
        pCu_addr1.Value = "1 The Larches";
        pCu_addr2.Value = "Larchton";
        pCu_postcode.Value = "S23 2JY";
        pCu_balance.Value = Convert.ToDecimal(124.99 + 12.50 - 12.50);
        command.ExecuteNonQuery();

        pCu_code.Value = "IKS220";
        pCu_name.Value = "International Karate Supplies";
        pCu_addr1.Value = "Chop House";
        pCu_addr2.Value = "Hiyerley";
        pCu_postcode.Value = "A12 3KO";
        pCu_balance.Value = Convert.ToDecimal(78.99 - 100.00);
        command.ExecuteNonQuery();

        command.Parameters.Clear();
        command.CommandText = @"insert into CustomerTransactions (tx_cust, tx_ref, tx_type, tx_value ) values (?, ?, ?, ?)";
        var pTx_cust = command.Parameters.Add("@pTx_cust", OleDbType.VarChar, 10);
        var pTx_ref = command.Parameters.Add("@pTx_ref", OleDbType.VarChar, 20);
        var pTx_type = command.Parameters.Add("@pTx_type", OleDbType.VarChar, 1);
        var pTx_value = command.Parameters.Add("@pTx_value", OleDbType.Decimal, 12);

        pTx_cust.Value = "ABC100";
        pTx_ref.Value = "Invoice 1298439";
        pTx_type.Value = "I";
        pTx_value.Value = Convert.ToDecimal(124.99);
        command.ExecuteNonQuery();
        pTx_ref.Value = "Invoice 2223422";
        pTx_type.Value = "I";
        pTx_value.Value = Convert.ToDecimal(12.50);
        command.ExecuteNonQuery();
        pTx_ref.Value = "Credit 2223422";
        pTx_type.Value = "C";
        pTx_value.Value = Convert.ToDecimal(-12.50);
        command.ExecuteNonQuery();

        pTx_cust.Value = "IKS220";
        pTx_ref.Value = "Invoice 122333";
        pTx_type.Value = "I";
        pTx_value.Value = Convert.ToDecimal(78.99);
        command.ExecuteNonQuery();
        pTx_ref.Value = "Credit 929839";
        pTx_type.Value = "C";
        pTx_value.Value = Convert.ToDecimal(-100);
        command.ExecuteNonQuery();

        command.CommandText = @"use in select('customers')";
        command.ExecuteNonQuery();

        conn.Close();
    }

    private static void CopyFileList(string source, string destination)
    {
        foreach (string filename in DataBaseFileNames)
        {
            var destFile = Path.Combine(destination, filename);
            File.Copy(Path.Combine(source, filename), destFile, true);
        }
    }

    public static void DeleteTemporaryData(string? location)
    {
        if (location is null)
        {
            return;
        }

        foreach (string filename in DataBaseFileNames)
        {
            var destFile = Path.Combine(location, filename);
            if (File.Exists(destFile))
            {
                File.Delete(destFile);
            }
        }
    }
}
