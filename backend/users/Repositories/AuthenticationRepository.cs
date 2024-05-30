using com.touir.expenses.Users.Infrastructure.Options;
using com.touir.expenses.Users.Models;
using com.touir.expenses.Users.Repositories.Contracts;
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
                    SELECT ATH_UserId Id, ATH_HashPassword HashPassword, ATH_HashSalt HashSalt, ATH_IsTemporaryPassword IsTemporaryPassword
                    FROM ATH_Authentications
                    WHERE ATH_UserId = @Id";

                return await connection.QueryFirstOrDefaultAsync<Authentication>(sql, new { Id = id });
            }
        }

        public async Task<bool> CreateAuthenticationAsync(Authentication authentication, bool resetHash = false)
        {
            if (authentication == null || authentication.User == null) 
                return false;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                using(var transaction =  connection.BeginTransaction())
                {
                    try
                    {
                        string sql = @"
                            INSERT INTO ATH_AUTHENTICATIONS(ATH_UserId, ATH_HashPassword, ATH_HashSalt, ATH_IsTemporaryPassword)
                            VALUES (@UserId, @HashPassword, @HashSalt, @IsTemporaryPassword)
                            ";
                        await connection.ExecuteAsync(sql, new
                        {
                            UserId = authentication.User.Id,
                            authentication.HashPassword,
                            authentication.HashSalt,
                            authentication.IsTemporaryPassword
                        }, transaction);

                        if(resetHash)
                        {
                            sql = @"UPDATE USR_Users SET USR_EmailValidationHash = null WHERE USR_Id = @Id";
                            await connection.ExecuteAsync(sql, authentication.User, transaction);
                        }

                        transaction.Commit();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        // to implement : log errors

                        transaction.Rollback();

                        return false;
                    }
                    
                }
            }
        }

        public async Task<bool> UpdateAuthenticationAsync(Authentication authentication, bool resetHash = false)
        {
            if (authentication == null || authentication.User == null)
                return false;

            using (var connection = new NpgsqlConnection(_connectionString))
            {
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string sql = @"
                            UPDATE ATH_AUTHENTICATIONS
                            SET
                                ATH_HashPassword = @HashPassword, 
                                ATH_HashSalt = @HashSalt,
                                ATH_IsTemporaryPassword = @IsTemporaryPassword
                            WHERE ATH_UserId = @UserId
                            ";
                        await connection.ExecuteAsync(sql, new
                        {
                            UserId = authentication.User.Id,
                            authentication.HashPassword,
                            authentication.HashSalt,
                            authentication.IsTemporaryPassword
                        }, transaction);

                        if (resetHash)
                        {
                            sql = @"UPDATE USR_Users SET USR_EmailValidationHash = null WHERE USR_Id = @Id";
                            await connection.ExecuteAsync(sql, authentication.User, transaction);
                        }

                        transaction.Commit();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        // to implement : log errors

                        transaction.Rollback();

                        return false;
                    }

                }
            }
        }
    }
}
