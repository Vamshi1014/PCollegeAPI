using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
    public class DataMapping : DataRepositoryBase, IDataMapping
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        public DataMapping(IConfiguration config, ILogger<DataMapping> logger) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }

        //creating company and form controls mapping 

        public async Task<string> CompanyQuestionMapping(int companyId, int formDataId)
        {
            string respose = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    connection.Open();

                    // Create command and specify stored procedure name
                    SqlCommand command = new SqlCommand("InsertCompanyFormQuestions", connection);
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@CompanyId", companyId);
                    command.Parameters.AddWithValue("@FormDataId", formDataId);

                    // Execute the stored procedure
                    await command.ExecuteNonQueryAsync();
                    respose = "Data inserted successfully.";
                }
                }
                catch (Exception ex)
                {
                throw;
                }
            return respose;
            
        
    }
    }
}
