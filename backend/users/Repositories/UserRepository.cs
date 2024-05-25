using com.touir.expenses.Users.Infrastructure.Options;
using com.touir.expenses.Users.Models;
using com.touir.expenses.Users.Repositories.Contracts;
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
                        USR_LastUpdatedAt LastUpdatedAt, USR_LastUpdatedBy LastUpdatedBy, USR_IsDisabled IsDisabled,
                        USR_IsEmailValidated IsEmailValidated, USR_EmailValidationHash EmailValidationHash
                    FROM USR_Users
                    WHERE USR_Email = @Email";

                return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
            }
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    INSERT INTO USR_Users(USR_FirstName, USR_LastName, USR_Email, USR_CreatedAt, USR_CreatedBy, USR_LastUpdatedAt,
                        USR_LastUpdatedBy, USR_IsDisabled, USR_IsEmailValidated, USR_EmailValidationHash)
                    VALUES(@FirstName, @LastName, @Email, @CreatedAt, @CreatedById, @LastUpdatedAt, @LastUpdatedById, @IsDisabled,
                        @IsEmailValidated, @EmailValidationHash) RETURNING USR_Id";

                user.Id = await connection.ExecuteScalarAsync<int>(sql, new {
                    user.FirstName,
                    user.LastName,
                    user.Email,
                    user.CreatedAt,
                    CreatedById = user.CreatedBy?.Id,
                    user.LastUpdatedAt,
                    LastUpdatedById = user.LastUpdatedBy?.Id,
                    user.IsDisabled,
                    user.IsEmailValidated,
                    user.EmailValidationHash,
                });

                return user;
            }
        }

        public async Task<bool?> DeleteUserAsync(User user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    WITH deleted AS (DELETE FROM USR_Users WHERE USR_Id = @Id RETURNING *) SELECT count(*) FROM deleted";

                return await connection.ExecuteScalarAsync<int>(sql, user) > 0;
            }
        }

        public async Task<IList<string>> GetUsedEmailValidationHashesAsync()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<string>("SELECT USR_EmailValidationHash FROM USR_Users WHERE USR_EmailValidationHash IS NOT NULL AND COALESCE(USR_IsEmailValidated,0) = 0");
                return result.ToList();
            }
        }

        public async Task<bool> VerifyEmail(string emailValidationHash, string email)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    WITH updated as (UPDATE USR_Users SET USR_IsEmailValidated = TRUE WHERE USR_EmailValidationHash = @EmailValidationHash AND USR_Email = @Email RETURNING *)
                    SELECT count(*) FROM updated";

                int count = await connection.ExecuteScalarAsync<int>(sql, new
                {
                    EmailValidationHash = emailValidationHash,
                    Email = email
                });

                return count > 0;
            }
        }
    }
}
