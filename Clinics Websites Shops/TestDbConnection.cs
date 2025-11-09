using Clinics_Websites_Shops.Services;

// Test the EnvironmentService connection string generation
var envService = new EnvironmentService();

Console.WriteLine("=== Database Configuration Test ===");
Console.WriteLine($"DB_CONNECTION: {envService.GetValue("DB_CONNECTION")}");
Console.WriteLine($"DB_HOST: {envService.GetValue("DB_HOST")}");
Console.WriteLine($"DB_PORT: {envService.GetValue("DB_PORT")}");
Console.WriteLine($"DB_DATABASE: {envService.GetValue("DB_DATABASE")}");
Console.WriteLine($"DB_USERNAME: {envService.GetValue("DB_USERNAME")}");
Console.WriteLine($"Password set: {!string.IsNullOrEmpty(envService.GetValue("DB_PASSWORD"))}");

Console.WriteLine("\n=== Generated Connection Strings ===");
try 
{
    var connectionString = envService.GetConnectionString();
    Console.WriteLine($"Main DB: {connectionString}");
    
    var masterConnectionString = envService.GetMasterConnectionString();
    Console.WriteLine($"Master DB: {masterConnectionString}");
    
    var dbProvider = envService.GetDatabaseProvider();
    Console.WriteLine($"Database Provider: {dbProvider}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();