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
    public class UniversityRepository: DataRepositoryBase, IUniversityRepository
    {
        protected readonly IConfiguration _config;
        protected readonly ILogger _logger;
        DataTables objDataTables = new DataTables();
        public UniversityRepository(ILogger<UniversityRepository> logger, IConfiguration config) : base(logger, config)
        {
            _config = config;
            _logger = logger;
        }
        public async Task<List<UniversityMaster>> GetUniversities(string? universityName)
        {
            List<UniversityMaster> universityMaster = new List<UniversityMaster>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    SqlCommand command = new SqlCommand("GetTopUniversities", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Univ_Name", universityName);
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            UniversityMaster universitymaster = new UniversityMaster
                            {
                                UniversityId = Convert.ToInt32(reader["Univ_ID"]),
                                Univ_Name = reader["Univ_Name"] != DBNull.Value ? reader["Univ_Name"].ToString() : null,

                            };

                            universityMaster.Add(universitymaster);
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

            return universityMaster;
        }

        public async Task<List<EntryRequirement>> GetEntryRequirementsByCountryAndUniversityAsync(int universityCountry, int universityID, int academicCategoryId)
        {
            var entryRequirements = new List<EntryRequirement>();
            try
            {

                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (var command = new SqlCommand("GetEntryRequirementsByCountryAndUniversity", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UniversityCountry", universityCountry);
                        command.Parameters.AddWithValue("@UniversityID", universityID);
                        command.Parameters.AddWithValue("@AcademicCategoryId", academicCategoryId);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var entryRequirement = new EntryRequirement
                                {
                                    EntryRequirementID = reader.GetInt32(reader.GetOrdinal("EntryRequirementID")),
                                    UniversityID = reader.GetInt32(reader.GetOrdinal("UniversityID")),
                                    AcademicEntryRequirements = reader.GetString(reader.GetOrdinal("AcademicEntryRequirements")),
                                    MathematicsEntryRequirements = reader.GetString(reader.GetOrdinal("MathematicsEntryRequirements")),
                                    EnglishLanguageRequirements = reader.GetString(reader.GetOrdinal("EnglishLanguageRequirements")),
                                    EnglishLanguageWaiver = reader.GetString(reader.GetOrdinal("EnglishLanguageWaiver")),
                                    // Assuming CountryID and AcademicCategoryId are used to fetch or construct Country and AcademicCategory objects
                                    EntryCountry = new Country
                                    {
                                        CountryID = reader.GetInt32(reader.GetOrdinal("entryCountryId")),
                                        CountryName = reader.GetString(reader.GetOrdinal("entryCountryName"))
                                    },
                                    AcademicCategory = new AcademicCategory
                                    {
                                        AcademicCategoryID = reader.GetInt32(reader.GetOrdinal("AcademicCategoryId"))
                                    },
                                    UniversityCountry = new Country
                                    {
                                        CountryID = reader.GetInt32(reader.GetOrdinal("UniversityCountry"))
                                    }
                                };

                                entryRequirements.Add(entryRequirement);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return entryRequirements;
        }

        public async Task<EntryRequirement> UpsertEntryRequirementAsync(EntryRequirement entryRequirement,int? userId =0)
        {
            EntryRequirement updatedEntryRequirement = null;
            string message = "Upsert failed";

            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                using (SqlCommand command = new SqlCommand("UpsertEntryRequirement", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@EntryRequirementID", entryRequirement.EntryRequirementID);
                    command.Parameters.AddWithValue("@UniversityID", entryRequirement.UniversityID);
                    command.Parameters.AddWithValue("@CreatedBy", userId);
                    command.Parameters.AddWithValue("@CountryID", entryRequirement.EntryCountry != null ? (object)entryRequirement.EntryCountry.CountryID : DBNull.Value);
                    command.Parameters.AddWithValue("@AcademicCategoryId", entryRequirement.AcademicCategory != null ? (object)entryRequirement.AcademicCategory.AcademicCategoryID : DBNull.Value);
                    command.Parameters.AddWithValue("@AcademicEntryRequirements", entryRequirement.AcademicEntryRequirements ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@MathematicsEntryRequirements", entryRequirement.MathematicsEntryRequirements ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EnglishLanguageRequirements", entryRequirement.EnglishLanguageRequirements ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EnglishLanguageWaiver", entryRequirement.EnglishLanguageWaiver ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UniversityCountry", entryRequirement.UniversityCountry != null ? (object)entryRequirement.UniversityCountry.CountryID : DBNull.Value);

                    SqlParameter outputID = new SqlParameter("@NewEntryRequirementID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputID);

                    SqlParameter outputMessage = new SqlParameter("@Message", SqlDbType.NVarChar, 4000)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputMessage);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Handle the output parameter values
                        int newEntryRequirementID = outputID.Value != DBNull.Value ? Convert.ToInt32(outputID.Value) : 0;
                        message = outputMessage.Value != DBNull.Value ? outputMessage.Value.ToString() : "Upsert failed";

                        // Update EntryRequirement object with the new ID
                        updatedEntryRequirement = new EntryRequirement
                        {
                            EntryRequirementID = newEntryRequirementID,
                            UniversityID = entryRequirement.UniversityID,
                            EntryCountry = entryRequirement.EntryCountry,
                            AcademicCategory = entryRequirement.AcademicCategory,
                            AcademicEntryRequirements = entryRequirement.AcademicEntryRequirements,
                            MathematicsEntryRequirements = entryRequirement.MathematicsEntryRequirements,
                            EnglishLanguageRequirements = entryRequirement.EnglishLanguageRequirements,
                            EnglishLanguageWaiver = entryRequirement.EnglishLanguageWaiver,
                            UniversityCountry = entryRequirement.UniversityCountry
                        };

                        if (message != "Upserted successfully")
                        {
                            throw new Exception(message);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exception and set error message
                        updatedEntryRequirement = null;
                        message = "An error occurred: " + ex.Message;
                    }
                }
            }

            return updatedEntryRequirement;
        }

        public async Task<List<CountrySpecificUniversity>> GetCountrySpecificUniversitiesAsync(string? searchUniversityName)
        {
            List<CountrySpecificUniversity> universities = new List<CountrySpecificUniversity>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand("GetCountrySpecificUniversities", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (string.IsNullOrEmpty(searchUniversityName))
                        {
                            command.Parameters.Add(new SqlParameter("@UniversityName", DBNull.Value));
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@UniversityName", searchUniversityName));
                        }

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                CountrySpecificUniversity university = new CountrySpecificUniversity
                                {
                                    CountrySpecificUniversityId = reader.GetInt32(reader.GetOrdinal("CountrySpecificUniversityID")),
                                    CountrySpecificUniversityName = reader.IsDBNull(reader.GetOrdinal("CountrySpecificUniversityName")) ? null : reader.GetString(reader.GetOrdinal("CountrySpecificUniversityName")),
                                    Is_Active = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                    Country = new Country
                                    {
                                        CountryID = reader.GetInt32(reader.GetOrdinal("Country_ID"))
                                    }
                                };

                                universities.Add(university);
                            }
                        }
                    }
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving country-specific universities.");
                throw;
            }

            return universities;
        }
        public async Task<List<HigherSecondaryBoard>> GetHigherSecondaryBoardsAsync(string? searchHSCName)
        {
            List<HigherSecondaryBoard> boards = new List<HigherSecondaryBoard>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    await connection.OpenAsync();
                    using (SqlCommand command = new SqlCommand("GetHigherSecondaryBoards", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (string.IsNullOrEmpty(searchHSCName))
                        {
                            command.Parameters.Add(new SqlParameter("@HSCName", DBNull.Value));
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@HSCName", searchHSCName));
                        }

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                HigherSecondaryBoard board = new HigherSecondaryBoard
                                {
                                    HSCID = reader.GetInt32(reader.GetOrdinal("HSCID")),
                                    HSCName = reader.IsDBNull(reader.GetOrdinal("HSCName")) ? null : reader.GetString(reader.GetOrdinal("HSCName")),
                                    Is_Active = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                    Country = new Country
                                    {
                                        CountryID = reader.GetInt32(reader.GetOrdinal("Country_ID"))
                                    }
                                };

                                boards.Add(board);
                            }
                        }
                    }
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving higher secondary boards.");
                throw;
            }

            return boards;
        }

        public async Task<List<UniversityEntryRequirements>> UpsertUniversityEntryRequirementsAsync(List<UniversityEntryRequirements> listUniversityEntryRequirementsData)
        {
            List<UniversityEntryRequirements> updatedListUniversityEntryRequirementsData = new List<UniversityEntryRequirements>();

            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                using (SqlCommand command = new SqlCommand("UpsertUniversityEntryRequirements", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Create and populate table-valued parameter
                    DataTable universityEntryRequirementsDataTable = objDataTables.UniversityEntryRequirementsDataTable();
                    foreach (var universityEntryRequirement in listUniversityEntryRequirementsData)
                    {
                        universityEntryRequirementsDataTable.Rows.Add(
                            universityEntryRequirement.UniversityEntryRequirementsId,
                            universityEntryRequirement.Typeid,
                            universityEntryRequirement.Universityid,
                            universityEntryRequirement.EntryCountryid,
                            universityEntryRequirement.UniversityCountryid,
                            universityEntryRequirement.HSCId,
                            universityEntryRequirement.Percentage,
                            universityEntryRequirement.MOIId,
                            universityEntryRequirement.Education_Gap,
                            universityEntryRequirement.Created_By,
                            universityEntryRequirement.Created_On,
                            universityEntryRequirement.Updated_By,
                            universityEntryRequirement.Updated_On,
                            universityEntryRequirement.IsActive
                        );
                    }

                    SqlParameter parameter = command.Parameters.AddWithValue("@UniversityEntries", universityEntryRequirementsDataTable);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.UniversityEntryRequirementsType"; // Ensure this matches your table type name

                    SqlParameter successMessageParam = new SqlParameter("@SuccessMessage", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(successMessageParam);

                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                UniversityEntryRequirements updatedUniversityEntryRequirement = new UniversityEntryRequirements
                                {
                                    UniversityEntryRequirementsId = reader.GetInt32(0),                                   
                                    Typeid =  reader.GetInt32(1) ,
                                    Universityid = reader.GetInt32(2),
                                    EntryCountryid = reader.GetInt32(3),
                                    UniversityCountryid =reader.GetInt32(4),
                                    HSCId = reader.GetInt32(5),
                                    Percentage = reader.GetDecimal(6),
                                    MOIId = reader.GetInt32(7),
                                    Education_Gap = reader.GetInt32(8),
                                    Created_By = reader.GetInt32(9),
                                    Created_On = reader.GetDateTime(10),
                                    Updated_By = reader.GetInt32(11),
                                    Updated_On = reader.GetDateTime(12),
                                    IsActive = reader.GetBoolean(13)
                                };

                                updatedListUniversityEntryRequirementsData.Add(updatedUniversityEntryRequirement);
                            }
                        }
                    }

                    // Retrieve the success message from the output parameter
                    string successMessage = (string)successMessageParam.Value;

                    // Assign the success message to each item in the list
                    foreach (var universityEntryRequirement in updatedListUniversityEntryRequirementsData)
                    {
                        universityEntryRequirement.Response = successMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception as needed, perhaps logging it or rethrowing
                throw new ApplicationException("An error occurred while upserting university entry requirements.", ex);
            }

            return updatedListUniversityEntryRequirementsData;
        }

        public async Task<List<UniversityEntryRequirements>> GetUniversityEntryRequirementsAsync(int universityId, int entryCountryId, int universityCountryId, int? typeId)
        {
            var results = new List<UniversityEntryRequirements>();

            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                using (var command = new SqlCommand("GetUniversityEntryRequirements", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@Universityid", universityId);
                    command.Parameters.AddWithValue("@EntryCountryid", entryCountryId);
                    command.Parameters.AddWithValue("@UniversityCountryid", universityCountryId);
                    command.Parameters.AddWithValue("@Typeid", (object)typeId ?? DBNull.Value);

                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var entry = new UniversityEntryRequirements
                            {
                                UniversityEntryRequirementsId = reader.GetInt32(0),
                                Typeid = reader.GetInt32(1),
                                Universityid = reader.GetInt32(2),
                                EntryCountryid = reader.GetInt32(3),
                                UniversityCountryid = reader.GetInt32(4),
                                HSCId = reader.GetInt32(5),
                                Percentage = reader.GetDecimal(6),
                                MOIId = reader.GetInt32(7),
                                Education_Gap = reader.GetInt32(8),
                                Created_By = reader.GetInt32(9),
                                Created_On = reader.GetDateTime(10),
                                Updated_By = reader.GetInt32(11),
                                Updated_On = reader.GetDateTime(12),
                                IsActive = reader.GetBoolean(13)
                            };

                            results.Add(entry);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., logging)
                throw new ApplicationException("An error occurred while retrieving university entry requirements.", ex);
            }

            return results;
        }

        public async Task<List<UniversityExamRequirement>> InsertEnglishExamRequirementsAsync(List<UniversityExamRequirement> universityEnglishExamRequirement, SqlTransaction transaction)
        {
            List<UniversityExamRequirement> insertedRequirements = new List<UniversityExamRequirement>();
            
            try
            {
                using (var command = new SqlCommand("InsertEnglishExamRequirements", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Create and populate DataTable
                    DataTable examRequirementsDataTable = objDataTables.examRequirementsTable(); // Ensure this method creates the DataTable schema correctly

                    foreach (var requirement in universityEnglishExamRequirement.Where(r => r.EnglishExamRequirement != null).ToList())
                    {
                        examRequirementsDataTable.Rows.Add(
                            requirement.EnglishExamRequirement.ExamTypeId,
                            requirement.EnglishExamRequirement.ListeningScore,
                            requirement.EnglishExamRequirement.SpeakingScore,
                            requirement.EnglishExamRequirement.ReadingScore,
                            requirement.EnglishExamRequirement.WritingScore,
                            requirement.EnglishExamRequirement.VerbalReasoningScore,
                            requirement.EnglishExamRequirement.QuantitativeReasoningScore,
                            requirement.EnglishExamRequirement.AnalyticalWritingScore,
                            requirement.EnglishExamRequirement.MinimumScore,
                            requirement.EnglishExamRequirement.OverallScore,
                            requirement.EnglishExamRequirement.LiteracyScore,
                            requirement.EnglishExamRequirement.ConversationScore,
                            requirement.EnglishExamRequirement.ComprehensionScore,
                            requirement.EnglishExamRequirement.ProductionScore,
                            requirement.EnglishExamRequirement.Createdon,
                            requirement.EnglishExamRequirement.CreatedBy,
                            requirement.EnglishExamRequirement.UpdatedBy,
                            requirement.EnglishExamRequirement.UpdatedOn
                        );
                    }

                    SqlParameter tvpParam = command.Parameters.AddWithValue("@ExamRequirements", examRequirementsDataTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.EnglishExamRequirementsType"; // Ensure this matches your table type name

                  

                    // Execute the stored procedure and read results
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        int index = 0;

                        while (await reader.ReadAsync() && index < universityEnglishExamRequirement.Count)
                        {
                            // Assuming the stored procedure returns the inserted ExamRequirementId
                            int insertedExamRequirementId = reader.GetInt32(0); // The identity column value returned from the database

                            // Update the ExamRequirementId in the corresponding EntryRequirementID object
                            universityEnglishExamRequirement[index].EnglishExamRequirement.ExamRequirementId = insertedExamRequirementId;

                            // Add the updated requirement to the list
                            insertedRequirements.Add(universityEnglishExamRequirement[index]);

                            index++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (logging, rethrowing, etc.)
                throw new ApplicationException("An error occurred while inserting English exam requirements.", ex);
            }

            return insertedRequirements;
        }

        public async Task<List<UniversityExamRequirement>> InsertUniversityExamRequirementsAsync(List<UniversityExamRequirement> universityEnglishExamRequirements, SqlTransaction transaction)
        {
            List<UniversityExamRequirement> insertedData = new List<UniversityExamRequirement>();

            try
            {
                using (var command = new SqlCommand("InsertOrUpdateUniversityEnglishExamRequirements", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Create and populate DataTable
                    DataTable universityExamRequirementsDataTable = objDataTables.UniversityExamRequirementsTable();
                    foreach (var requirement in universityEnglishExamRequirements)
                    {
                        universityExamRequirementsDataTable.Rows.Add(
                            requirement.UniversityExamRequirementId,
                            requirement.UniversityId,
                            requirement.AcademicCategoryId,
                            requirement.EnglishExamRequirement?.ExamRequirementId, // Safely access ExamRequirementId
                            requirement.IsActive, requirement.UniversityCountryId
                        );
                    }

                    SqlParameter tvpParam = command.Parameters.AddWithValue("@UniversityExamRequirements", universityExamRequirementsDataTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.UniversityEnglishExamRequirementsType"; // Ensure this matches your table type name

                  
                    // Execute the stored procedure and read results
                    // Execute the stored procedure and read results
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        int index = 0;

                        while (await reader.ReadAsync() && index < universityEnglishExamRequirements.Count)
                        {
                            // Assuming the stored procedure returns the inserted ExamRequirementId
                            int insertedExamRequirementId = reader.GetInt32(reader.GetOrdinal("UniversityExamRequirementId")); // Adjust to the correct column

                            // Update the ExamRequirementId in the corresponding EntryRequirementID object
                            universityEnglishExamRequirements[index].UniversityExamRequirementId = insertedExamRequirementId;

                            // Add the updated requirement to the list
                            insertedData.Add(universityEnglishExamRequirements[index]);

                            index++;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // Handle exception (logging, rethrowing, etc.)
                throw new ApplicationException("An error occurred while inserting university exam requirements.", ex);
            }

            return insertedData;
        }

        public async Task<List<UniversityExamRequirement>> InsertUniversityExamRequirements(List<UniversityExamRequirement> universityEnglishExamRequirement)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // Upsert company details
                    universityEnglishExamRequirement = await InsertEnglishExamRequirementsAsync(universityEnglishExamRequirement, transaction);
                    universityEnglishExamRequirement = await InsertUniversityExamRequirementsAsync(universityEnglishExamRequirement, transaction);

                    // Commit transaction
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    transaction.Rollback();
                    throw;
                }
                finally
                {
                    // Close connection
                    _ = connection.CloseAsync();
                }
            }

            return universityEnglishExamRequirement;
        }

        public async Task<List<UniversityExamRequirement>> GetUniversityEnglishExamRequirementsAsync(
    int universityCountryId = 0,
    int academicCategoryId = 0,
    int universityId = 0)
        {
            var universityExamRequirements = new List<UniversityExamRequirement>();

            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (var command = new SqlCommand("GetUniversityEnglishExamRequirementbasedonUniversity", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UniversityCountryId", universityCountryId);
                        command.Parameters.AddWithValue("@AcademicCategoryId", academicCategoryId);
                        command.Parameters.AddWithValue("@UniversityId", universityId);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var universityExamRequirement = new UniversityExamRequirement
                                {
                                    UniversityExamRequirementId = reader.GetInt32(reader.GetOrdinal("UniversityExamRequirementId")),
                                    UniversityId = reader.GetInt32(reader.GetOrdinal("UniversityId")),
                                    AcademicCategoryId = reader.GetInt32(reader.GetOrdinal("AcademicCategoryId")),
                                    IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")),
                                    UniversityCountryId = reader.GetInt32(reader.GetOrdinal("UniversityCountryId")),
                                    EnglishExamRequirement = new EnglishExam
                                    {
                                        ExamRequirementId = reader.GetInt32(reader.GetOrdinal("ExamRequirementId")),
                                        ExamTypeId = reader.GetInt32(reader.GetOrdinal("ExamTypeId")),
                                        ListeningScore = reader.IsDBNull(reader.GetOrdinal("ListeningScore")) ? null : reader.GetString(reader.GetOrdinal("ListeningScore")),
                                        ReadingScore = reader.IsDBNull(reader.GetOrdinal("ReadingScore")) ? null : reader.GetString(reader.GetOrdinal("ReadingScore")),
                                        SpeakingScore = reader.IsDBNull(reader.GetOrdinal("SpeakingScore")) ? null : reader.GetString(reader.GetOrdinal("SpeakingScore")),
                                        WritingScore = reader.IsDBNull(reader.GetOrdinal("WritingScore")) ? null : reader.GetString(reader.GetOrdinal("WritingScore")),
                                        OverallScore = reader.IsDBNull(reader.GetOrdinal("OverallScore")) ? null : reader.GetString(reader.GetOrdinal("OverallScore")),
                                        MinimumScore = reader.IsDBNull(reader.GetOrdinal("MinimumScore")) ? null : reader.GetString(reader.GetOrdinal("MinimumScore")),
                                        AnalyticalWritingScore = reader.IsDBNull(reader.GetOrdinal("AnalyticalWritingScore")) ? null : reader.GetString(reader.GetOrdinal("AnalyticalWritingScore")),
                                        ComprehensionScore = reader.IsDBNull(reader.GetOrdinal("ComprehensionScore")) ? null : reader.GetString(reader.GetOrdinal("ComprehensionScore")),
                                        ConversationScore = reader.IsDBNull(reader.GetOrdinal("ConversationScore")) ? null : reader.GetString(reader.GetOrdinal("ConversationScore")),
                                        ProductionScore = reader.IsDBNull(reader.GetOrdinal("ProductionScore")) ? null : reader.GetString(reader.GetOrdinal("ProductionScore")),
                                        QuantitativeReasoningScore = reader.IsDBNull(reader.GetOrdinal("QuantitativeReasoningScore")) ? null : reader.GetString(reader.GetOrdinal("QuantitativeReasoningScore")),
                                        LiteracyScore = reader.IsDBNull(reader.GetOrdinal("LiteracyScore")) ? null : reader.GetString(reader.GetOrdinal("LiteracyScore")),
                                        VerbalReasoningScore = reader.IsDBNull(reader.GetOrdinal("VerbalReasoningScore")) ? null : reader.GetString(reader.GetOrdinal("VerbalReasoningScore"))
                                    }
                                };

                                universityExamRequirements.Add(universityExamRequirement);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Consider logging the exception before rethrowing
                throw new ApplicationException("An error occurred while retrieving university exam requirements.", ex);
            }

            return universityExamRequirements;
        }

    }
}
