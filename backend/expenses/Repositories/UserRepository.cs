using Dapper;
using com.touir.expenses.Expenses.Models;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Data;

namespace com.touir.expenses.Expenses.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IOptions<PostgresOptions> postgresOptions)
        {
            _connectionString = postgresOptions.Value.ConnectionString;
        }
        public async Task DeleteUserAsync(User user)
        {
            using(var connection = new NpgsqlConnection(_connectionString)) 
            {
                string sql = "UPDATE MQ_USR_Users SET USR_IsDeleted = true WHERE USR_Id = @Id";

                await connection.ExecuteAsync(sql, user);
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT USR_Id Id, USR_FirstName FirstName, USR_LastName LastName, USR_Email Email, USR_IsDeleted IsDeleted
                    FROM MQ_USR_Users
                    WHERE USR_Id = @Id";

                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
            }
        }

        public async Task<IEnumerable<User>> GetUsersByFamilyIdAsync(int familyId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT USR_Id Id, USR_FirstName FirstName, USR_LastName LastName, USR_Email Email, USR_IsDeleted IsDeleted
                    FROM MQ_USR_Users
                    WHERE USR_FamilyId = @FamilyId";

                return await connection.QueryAsync<User>(sql, new { FamilyId = familyId });
            }
        }

        public async Task SaveOrUpdateUserAsync(User user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                // Check if the user exists
                bool userExists = await connection.ExecuteScalarAsync<bool>(
                    "SELECT COUNT(1) FROM MQ_USR_Users WHERE USR_Id = @Id", new { user.Id });

                if (!userExists)
                {
                    // User does not exist, perform an insert
                    string insertSql = @"
                        INSERT INTO MQ_USR_Users (USR_Id, USR_FirstName, USR_LastName, USR_Email, USR_FamilyId, USR_IsDeleted)
                        VALUES (@Id, @FirstName, @LastName, @Email, @FamilyId, false)";

                    await connection.ExecuteAsync(insertSql, user);
                }
                else
                {
                    // User exists, perform an update
                    string updateSql = @"
                        UPDATE MQ_USR_Users 
                        SET USR_FirstName = @FirstName, USR_LastName = @LastName, USR_Email = @Email, USR_FamilyId = @FamilyId 
                        WHERE USR_Id = @Id";

                    await connection.ExecuteAsync(updateSql, user);
                }
            }
        }
    }
}
