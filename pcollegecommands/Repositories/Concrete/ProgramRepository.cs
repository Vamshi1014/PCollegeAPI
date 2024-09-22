using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
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
    public class ProgramRepository : DataRepositoryBase, IProgramRepository
    {
        protected readonly IConfiguration _config;
        protected readonly ILogger _logger;

        public ProgramRepository(ILogger<MenuRepository> logger, IConfiguration config) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }
        public async Task<(int TotalCount, List<UniversityProgram> Programs)> GetUniversityProgramsAsync(
    string? universityName = null,
    string? programName = null,
    string? academicLevel = null,
    int countryId = 0,
    int pageNumber = 1,
    int pageSize = 1000)
        {
            int totalCount = 0;
            var universityPrograms = new List<UniversityProgram>();
            var englishExamScores = new List<UniversityExamRequirement>();
            try
            {

                // Setup the database connection
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    await connection.OpenAsync();

                    // Setup the command to call the stored procedure
                    using (var command = new SqlCommand("GetUniversitiesPrograms", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters to the command
                        command.Parameters.AddWithValue("@UnivName", (object)universityName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ProgramName", (object)programName ?? DBNull.Value);
                        command.Parameters.AddWithValue("@AcademicLevels", (object)academicLevel ?? DBNull.Value);
                        command.Parameters.AddWithValue("@CountryId", countryId);
                        command.Parameters.AddWithValue("@PageNumber", pageNumber);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        // Execute the command and process the results
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Read the total count
                            if (await reader.ReadAsync())
                            {
                                totalCount = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                            }

                            // Read the university programs
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var universityProgram = new UniversityProgram
                                    {

                                        UniversityProgramId = reader.GetInt32(reader.GetOrdinal("UniversityCountryId")),
                                        UniversityMaster = new UniversityMaster
                                        {
                                            UniversityId = reader.GetInt32(reader.GetOrdinal("Univ_ID")),
                                            Univ_Name = reader.GetString(reader.GetOrdinal("Univ_Name")),
                                            Univ_Website = reader.GetString(reader.GetOrdinal("Univ_Website"))
                                        },
                                        ProgramMaster = new ProgramMaster
                                        {
                                            ProgramName = reader.GetString(reader.GetOrdinal("ProgramName")),
                                            Duration = reader.GetString(reader.GetOrdinal("Duration")),
                                            Intake1 = reader.GetString(reader.GetOrdinal("I1")),
                                            Intake2 = reader.GetString(reader.GetOrdinal("I2")),
                                            Intake3 = reader.GetString(reader.GetOrdinal("I3")),
                                            Intake4 = reader.GetString(reader.GetOrdinal("I4")),
                                            Intake5 = reader.GetString(reader.GetOrdinal("I5")),
                                            CostOfLiving = Convert.ToDecimal(reader.GetString(reader.GetOrdinal("CostOfLiving"))),
                                            ApplicationFee = Convert.ToDecimal(reader.GetString(reader.GetOrdinal("ApplicationFee"))),
                                            TutionFee = Convert.ToDecimal(reader.GetString(reader.GetOrdinal("TutionFee"))),
                                            MinimumDeposit = Convert.ToDecimal(reader.GetString(reader.GetOrdinal("MinimumDeposit"))),
                                            ProcessingTime = reader.GetString(reader.GetOrdinal("ProcessingTime")),
                                            CreatedOn = reader.GetDateTime(reader.GetOrdinal("CreatedOn")),
                                            Currency = reader.IsDBNull(reader.GetOrdinal("Currency")) ? null : reader.GetString(reader.GetOrdinal("Currency"))
                                },
                                        AcademicLevel = new AcademicLevel
                                        {
                                            LevelName = reader.GetString(reader.GetOrdinal("LevelName")),
                                            AcademicCategory = new AcademicCategory { AcademicCategoryID = reader.GetInt32(reader.GetOrdinal("AcademicCategoryID")) }
                                            ,
                                        },
                                        Country = new Country
                                        {
                                            CountryName = reader.GetString(reader.GetOrdinal("Country_Name"))
                                        }
                                    };

                                    universityPrograms.Add(universityProgram);
                                }
                            }

                            // Read the English exam scores
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                  var englishExamScore = new UniversityExamRequirement
{
    UniversityId = reader.GetInt32(reader.GetOrdinal("Univ_ID")),
    AcademicCategoryId = reader.GetInt32(reader.GetOrdinal("AcademicCategoryID")),
    EnglishExamRequirement = new EnglishExam
    {
        ExamTypeId = reader.IsDBNull(reader.GetOrdinal("ExamTypeId")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ExamTypeId")),
        ListeningScore = reader.IsDBNull(reader.GetOrdinal("ListeningScore")) ? null : reader.GetString(reader.GetOrdinal("ListeningScore")),
        ReadingScore = reader.IsDBNull(reader.GetOrdinal("ReadingScore")) ? null : reader.GetString(reader.GetOrdinal("ReadingScore")),
        SpeakingScore = reader.IsDBNull(reader.GetOrdinal("SpeakingScore")) ? null : reader.GetString(reader.GetOrdinal("SpeakingScore")),
        WritingScore = reader.IsDBNull(reader.GetOrdinal("WritingScore")) ? null : reader.GetString(reader.GetOrdinal("WritingScore")),
        OverallScore = reader.IsDBNull(reader.GetOrdinal("OverallScore")) ? null : reader.GetString(reader.GetOrdinal("OverallScore")),
        LiteracyScore = reader.IsDBNull(reader.GetOrdinal("LiteracyScore")) ? null : reader.GetString(reader.GetOrdinal("LiteracyScore")),
        ProductionScore = reader.IsDBNull(reader.GetOrdinal("ProductionScore")) ? null : reader.GetString(reader.GetOrdinal("ProductionScore")),
        AnalyticalWritingScore = reader.IsDBNull(reader.GetOrdinal("AnalyticalWritingScore")) ? null : reader.GetString(reader.GetOrdinal("AnalyticalWritingScore")),
        ComprehensionScore = reader.IsDBNull(reader.GetOrdinal("ComprehensionScore")) ? null : reader.GetString(reader.GetOrdinal("ComprehensionScore")),
        ConversationScore = reader.IsDBNull(reader.GetOrdinal("ConversationScore")) ? null : reader.GetString(reader.GetOrdinal("ConversationScore")),
        QuantitativeReasoningScore = reader.IsDBNull(reader.GetOrdinal("QuantitativeReasoningScore")) ? null : reader.GetString(reader.GetOrdinal("QuantitativeReasoningScore")),
        VerbalReasoningScore = reader.IsDBNull(reader.GetOrdinal("VerbalReasoningScore")) ? null : reader.GetString(reader.GetOrdinal("VerbalReasoningScore"))
    },
    UniversityCountryId = reader.GetInt32(reader.GetOrdinal("UniversityCountryId"))
};

                                    englishExamScores.Add(englishExamScore);
                                }
                                if (englishExamScores.Count > 0)
                                {
                                    foreach (var program in universityPrograms)
                                    {
                                        program.ListUniversityExamRequirement = englishExamScores
                                            .Where(score => score.UniversityId == program.UniversityMaster.UniversityId && score.AcademicCategoryId == program.AcademicLevel.AcademicCategory.AcademicCategoryID).ToList();
                                    }
                                }
                            }
                        }
                    }
                }

              
            }
            catch (Exception ex)
            { throw ex; }

            return (totalCount, universityPrograms);
        }
        public async Task<List<ProgramMaster>> GetPrograms(string? programName = null)
        {
            List<ProgramMaster> programMaster = new List<ProgramMaster>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    SqlCommand command = new SqlCommand("GetTopPrograms", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProgramName", (object)programName ?? DBNull.Value);
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ProgramMaster programmaster = new ProgramMaster
                            {
                                ProgramId = Convert.ToInt32(reader["ProgramID"]),
                                ProgramName = reader["ProgramName"] != DBNull.Value ? reader["ProgramName"].ToString() : null,
                              
                            };

                            programMaster.Add(programmaster);
                        }
                        await connection.CloseAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) here as needed
                throw;
            }

            return programMaster;
        }

        public async Task<List<AcademicLevel>>GetAcademicLevels(string? levelName = null)
        {
            List<AcademicLevel> academicLevels = new List<AcademicLevel>();

            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                using (SqlCommand command = new SqlCommand("GetTopAcademicLevels", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@LevelName", (object?)levelName ?? DBNull.Value);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AcademicLevel level = new AcademicLevel
                            {
                                AcademicLevelId = reader.GetInt32(reader.GetOrdinal("LevelID")),
                                LevelName = reader.IsDBNull(reader.GetOrdinal("LevelName")) ? null : reader.GetString(reader.GetOrdinal("LevelName")),
                                AcademicCategory = new AcademicCategory
                                {
                                    AcademicCategoryID = reader.IsDBNull(reader.GetOrdinal("AcademicCategoryID")) ? 0 : reader.GetInt32(reader.GetOrdinal("AcademicCategoryID")),
                                    AcademicCategoryName = reader.IsDBNull(reader.GetOrdinal("AcademicCategoryName")) ? null : reader.GetString(reader.GetOrdinal("AcademicCategoryName"))
                                }
                            };

                            academicLevels.Add(level);
                        }
                    }
                }
            }

            return academicLevels;
        }
      

    }


}
