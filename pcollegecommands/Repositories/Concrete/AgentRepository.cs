using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class AgentRepository :  DataRepositoryBase, IAgentRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        DataTables objDataTables = new DataTables();
        public AgentRepository(IConfiguration config, ILogger<AgentRepository> logger) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }
        public List<Responses> BulkUpsertResponses(List<Responses> responses)
        {
            List<Responses> insertedResponses = new List<Responses>();

            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                SqlCommand command = new SqlCommand("BulkUpsertResponses", connection);
                command.CommandType = CommandType.StoredProcedure;

                // Create table-valued parameter
                DataTable responsesTable = objDataTables.ResponsesDataTable();
                foreach (Responses response in responses)
                {
                    responsesTable.Rows.Add(response.ResponsesId, response.QuestionId, response.CompanyId, response.ResponseText);
                }

                SqlParameter parameter = command.Parameters.AddWithValue("@ResponsesData", responsesTable);
                parameter.SqlDbType = SqlDbType.Structured;
                parameter.TypeName = "dbo.ResponsesTableType"; // Change to match your table type name
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Responses insertedResponse = new Responses
                    {
                        ResponsesId = !reader.IsDBNull(0) ? reader.GetInt32(0) : default(int),
                        QuestionId = !reader.IsDBNull(1) ? reader.GetInt32(1) : default(int),
                        CompanyId = !reader.IsDBNull(2) ? reader.GetInt32(2) : default(int),
                        ResponseText = !reader.IsDBNull(3) ? reader.GetString(3) : null
                    };

                    insertedResponses.Add(insertedResponse);
                }
            }

            return insertedResponses;
        }

    }
}
