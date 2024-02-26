using com.touir.expenses.Users.Infrastructure.Options;
using com.touir.expenses.Users.Models;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace com.touir.expenses.Users.Repositories
{
    public class AuthenticationRepository : IAuthenticationRepository
    {
        private readonly string _connectionString;
        public AuthenticationRepository(IOptions<PostgresOptions> postgresOptions) 
        {
            _connectionString = postgresOptions.Value.ConnectionString;
        }

        public async Task<Authentication?> GetAuthenticationByIdAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT ATH_UserId Id, ATH_HashPassword HashPassword, ATH_HashSalt HashSalt
                    FROM ""ATH_Authentications""
                    WHERE ATH_UserId = @Id";

                return await connection.QueryFirstOrDefaultAsync<Authentication>(sql, new { Id = id });
            }
        }
    }
}
