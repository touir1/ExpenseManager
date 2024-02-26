using com.touir.expenses.Users.Infrastructure.Options;
using com.touir.expenses.Users.Models;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace com.touir.expenses.Users.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IOptions<PostgresOptions> postgresOptions)
        {
            _connectionString = postgresOptions.Value.ConnectionString;
        }
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT USR_Id Id, USR_FirstName FirstName, USR_LastName LastName, USR_Email Email,
                        USR_FamilyId FamilyId, USR_CreatedAt CreatedAt, USR_CreatedBy CreatedBy, 
                        USR_LastUpdatedAt LastUpdatedAt, USR_LastUpdatedBy LastUpdatedBy, USR_IsDisabled IsDisabled
                    FROM USR_Users
                    WHERE USR_Email = @Email";

                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
            }
        }
    }
}
