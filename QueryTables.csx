#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 8.0.0"

using Npgsql;

var connString = "Server=127.0.0.1;Port=13121;User Id=postgres;Password=(eFEW-ZX+bhaUagmD2qTgt;Database=amis102";

try
{
    using var conn = new NpgsqlConnection(connString);
    conn.Open();

    using var cmd = new NpgsqlCommand(
        @"SELECT table_schema, table_name 
          FROM information_schema.tables 
          WHERE table_schema NOT IN ('pg_catalog', 'information_schema')
          ORDER BY table_schema, table_name",
        conn);

    using var reader = cmd.ExecuteReader();
    Console.WriteLine("=== PostgreSQL Tables in [amis102] ===\n");

    while (reader.Read())
    {
        var schema = reader.GetString(0);
        var table = reader.GetString(1);
        Console.WriteLine($"{schema}.{table}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
