using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class PermissionRepository : DataRepositoryBase, IPermissionRepository
    {

        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        public PermissionRepository(IConfiguration config, ILogger<PermissionRepository> logger) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }



        public async Task<bool> UserHasPermissionAsync(int userId, string methodName)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                using (SqlCommand command = new SqlCommand("CheckUserMethodPermission", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@MethodName", methodName);

                    SqlParameter hasPermissionParam = new SqlParameter("@HasPermission", SqlDbType.Bit)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(hasPermissionParam);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    bool hasPermission = (bool)hasPermissionParam.Value;
                    return hasPermission;
                }
            }
        }


    }
}
