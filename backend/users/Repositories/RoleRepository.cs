using com.touir.expenses.Users.Infrastructure.Options;
using com.touir.expenses.Users.Models;
using com.touir.expenses.Users.Repositories.Contracts;
using Dapper;
using Microsoft.Extensions.Options;
using Npgsql;

namespace com.touir.expenses.Users.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly string _connectionString;
        public RoleRepository(IOptions<PostgresOptions> postgresOptions)
        {
            _connectionString = postgresOptions.Value.ConnectionString;
        }

        public async Task<IEnumerable<Role>> GetUserRolesByApplicationCodeAsync(string applicationCode, int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"
                    SELECT RLE_Id Id, RLE_Code Code, RLE_Name Name, RLE_Description Description
                    FROM RLE_Roles
                    INNER JOIN APP_Applications
                        ON APP_Id = RLE_ApplicationId
                    INNER JOIN URR_UserRoles
                        ON URR_RoleId = RLE_Id
                    WHERE URR_UserId = @UserId AND APP_Code = @Code";

                return await connection.QueryAsync<Role>(sql, new { Code = applicationCode, UserId = userId });
            }
        }
    }
}
