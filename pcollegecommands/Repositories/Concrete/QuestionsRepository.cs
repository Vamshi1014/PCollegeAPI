using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Models.Enum;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class QuestionsRepository : DataRepositoryBase, IQuestionsRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        public QuestionsRepository(IConfiguration config, ILogger<QuestionsRepository> logger) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }
        public async Task<List<Questions>> GetQuestionsFromDatabase(int companyId, int formId)
        {
            List<Questions> questions = new List<Questions>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    SqlCommand command = new SqlCommand("GetQuestionsBasedonCompany", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@CompanyId", companyId));
                    command.Parameters.Add(new SqlParameter("@FormQuestionId", formId));

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Questions question = new Questions
                            {
                                QuestionId = Convert.ToInt32(reader["QuestionId"]),
                                QuestionText = reader["Question"] != DBNull.Value ? reader["Question"].ToString() : null,
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                IsMandatory = Convert.ToBoolean(reader["IsMandatory"]),
                                ValidationMessage = reader["ValidationMessage"] != DBNull.Value ? reader["ValidationMessage"].ToString() : null,
                                WarningMessage = reader["WarningMessage"] != DBNull.Value ? reader["WarningMessage"].ToString() : null,
                                CharLength = reader["CharLength"] != DBNull.Value ? Convert.ToInt32(reader["CharLength"]) : (int?)null,
                                InputType = (InputType)Convert.ToInt32(reader["InputType"]),
                                DocumentType = new DocumentType
                                {
                                    Id = (DocumentTypeId)Convert.ToInt32(reader["DocumentType"])
                                },
                                ClassId = reader["ClassId"] != DBNull.Value ? reader["ClassId"].ToString() : null,
                                ClassName = reader["ClassName"] != DBNull.Value ? reader["ClassName"].ToString() : null,
                                FieldEnable = reader["FieldEnable"] != DBNull.Value ? Convert.ToBoolean(reader["FieldEnable"]) : (bool?)null,
                                SortOrder = Convert.ToInt32(reader["SortOrder"]),
                                Tab = reader["tabId"] != DBNull.Value ? Convert.ToInt32(reader["tabId"]) : (int?)null
                            };

                            questions.Add(question);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here as needed
                throw;
            }

            return questions;
        }


    }
}
