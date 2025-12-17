using System.Data.SqlClient;

namespace TruequeTextil.Shared.Infrastructure;

/// <summary>
/// Provides database connection factory for ADO.NET access.
/// All repositories should use this to obtain connections.
/// </summary>
public class DatabaseConfig
{
    private readonly string _connectionString;

    public DatabaseConfig(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
    }

    /// <summary>
    /// Creates a new SqlConnection. Caller is responsible for disposing.
    /// </summary>
    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    /// <summary>
    /// Gets the connection string for direct use when needed.
    /// </summary>
    public string ConnectionString => _connectionString;
}
