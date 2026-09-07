using Oracle.ManagedDataAccess.Client;

var cmd = new OracleCommand();
cmd.BindByName = true;
Console.WriteLine("[1/2] BindByName = true  OK");
cmd.InitialLONGFetchSize = -1;
Console.WriteLine("[2/2] InitialLONGFetchSize = -1  OK");
Console.WriteLine("ALL_PASSED");
