using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Helpers;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Flyurdreamcommands.Service.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class InsertMasterData : DataRepositoryBase, IInsertMasterData
    {
        protected readonly IConfiguration _config;
        protected readonly ILogger _logger;
        protected readonly IBlobHandler _blobHandler;
        public InsertMasterData(IConfiguration config, ILogger<InsertMasterData> logger, IBlobHandler blobHandler) : base(logger, config)
        {
            _config = config;
            _logger = logger;
            _blobHandler = blobHandler;
        }
        public async Task<int> GetCountryId(string countryName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    SqlCommand command = new SqlCommand("SELECT Country_ID FROM country_Master WHERE Country_Name = @CountryName", connection);
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@CountryName", countryName);

                    if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                    {
                        connection.Open();
                    }
                    var result = command.ExecuteScalar();

                    if (result != null)
                    {
                        return (int)result;
                    }
                    else
                    {
                        throw new Exception("Country not found");
                    }
                }
            } catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> GetOrInsertUniversityId(UniversityMaster objUniversityMaster)
        {
            try
            {

                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (var command = new SqlCommand("GetorInsertUniversityMaster", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Univ_Name", objUniversityMaster.Univ_Name);
                        command.Parameters.AddWithValue("@Univ_Description", objUniversityMaster.Univ_Description);
                        command.Parameters.AddWithValue("@Univ_Logo", objUniversityMaster.Univ_Logo);
                        command.Parameters.AddWithValue("@Univ_Phone", objUniversityMaster.Univ_Phone);
                        command.Parameters.AddWithValue("@Univ_Email", objUniversityMaster.Univ_Email);
                        command.Parameters.AddWithValue("@Univ_Website", objUniversityMaster.Univ_Website);
                        command.Parameters.AddWithValue("@Assigned_Users", objUniversityMaster.Assigned_Users);
                        command.Parameters.AddWithValue("@Is_Active", objUniversityMaster.Is_Active);

                        SqlParameter outputIdParam = new SqlParameter("@Univ_ID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputIdParam);

                        if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                        {
                            connection.Open();
                        }
                        await command.ExecuteNonQueryAsync();
                        return (int)outputIdParam.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<int> GetOrInsertAcademicLevelId(AcademicLevel objAcademicLevel)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (var command = new SqlCommand("GetorInsertAcademicLevel", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@LevelName", objAcademicLevel.LevelName);
                        command.Parameters.AddWithValue("@Description", (object)objAcademicLevel.Description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@IsActive", objAcademicLevel.IsActive);

                        SqlParameter outputIdParam = new SqlParameter("@LevelID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputIdParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        if (outputIdParam.Value != DBNull.Value)
                        {
                            return (int)outputIdParam.Value;
                        }
                        else
                        {
                            throw new Exception("Failed to retrieve LevelID.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                throw;
            }
        }

        public async Task<int> GetOrInsertProgram(ProgramMaster objProgramMaster)
        {
            try
            { 
            using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                using (var command = new SqlCommand("GetorInsertProgramMaster", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProgramName", objProgramMaster.ProgramName);
                    //command.Parameters.AddWithValue("@Duration", objProgramMaster.Duration);
                    //command.Parameters.AddWithValue("@Intake", objProgramMaster.Intake);
                    //command.Parameters.AddWithValue("@CostOfLiving", objProgramMaster.CostOfLiving);
                    //command.Parameters.AddWithValue("@ApplicationFee", objProgramMaster.ApplicationFee);
                    //command.Parameters.AddWithValue("@TuitionFee", objProgramMaster.TuitionFee);
                    //command.Parameters.AddWithValue("@MinimumDeposit", objProgramMaster.MinimumDeposit);
                    //command.Parameters.AddWithValue("@ProcessingTime", objProgramMaster.ProcessingTime);
                        //command.Parameters.AddWithValue("@CreatedOn", createdOn);
                        if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                        {
                            connection.Open();
                        }

                        SqlParameter outputIdParam = new SqlParameter("@ProgramID", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputIdParam);
                        if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                        {
                            connection.Open();
                        }

                        await command.ExecuteNonQueryAsync();
                        return (int)outputIdParam.Value;
                }
            }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<int> GetOrInsertProgramDetails(ProgramMaster objProgramMaster)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (var command = new SqlCommand("GetorInsertProgramDetails", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Duration", objProgramMaster.Duration);
                        command.Parameters.AddWithValue("@Intake1", Convert.ToInt32(objProgramMaster.Intake1));
                        command.Parameters.AddWithValue("@Intake2", Convert.ToInt32(objProgramMaster.Intake2));
                        command.Parameters.AddWithValue("@Intake3", Convert.ToInt32(objProgramMaster.Intake3));
                        command.Parameters.AddWithValue("@Intake4", Convert.ToInt32(objProgramMaster.Intake4));
                        command.Parameters.AddWithValue("@Intake5", Convert.ToInt32(objProgramMaster.Intake5));
                        command.Parameters.AddWithValue("@CostOfLiving", objProgramMaster.CostOfLiving);
                        command.Parameters.AddWithValue("@ApplicationFee", objProgramMaster.ApplicationFee);
                        command.Parameters.AddWithValue("@TutionFee", objProgramMaster.TutionFee);
                        command.Parameters.AddWithValue("@Currency", objProgramMaster.Currency);
                        command.Parameters.AddWithValue("@MinimumDeposit", objProgramMaster.MinimumDeposit);
                        command.Parameters.AddWithValue("@ProcessingTime", objProgramMaster.ProcessingTime);
                        //command.Parameters.AddWithValue("@CreatedOn", createdOn);
                        if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                        {
                            connection.Open();
                        }

                        SqlParameter outputIdParam = new SqlParameter("@ProgramDetailsID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputIdParam);
                        if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                        {
                            connection.Open();
                        }

                        await command.ExecuteNonQueryAsync();
                        return (int)outputIdParam.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
        public async Task<int> GetInsertUniversityMapping(int countryId, int universityId, int academicLevelId, int programId, int programDetailsId)
        {
            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (var command = new SqlCommand("GetorInsertUniversityMapping", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CountryId", countryId);
                        command.Parameters.AddWithValue("@UniversityId", universityId);
                        command.Parameters.AddWithValue("@AcademicLevelId", academicLevelId);
                        command.Parameters.AddWithValue("@ProgramId", programId);
                        command.Parameters.AddWithValue("@ProgramDetailsId", programDetailsId);

                        SqlParameter outputIdParam = new SqlParameter("@UniversityCountryId", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(outputIdParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        if (outputIdParam.Value != DBNull.Value)
                        {
                            return (int)outputIdParam.Value;
                        }
                        else
                        {
                            throw new Exception("Failed to retrieve MappingId.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (logging, rethrowing, etc.)
                throw;
            }
        }

        public async Task<string> BulkInsert(DataTable dataTable, int batchSize)
        {
            try
            {
                string result = string.Empty;
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken)
                    {
                        connection.Open();
                    }
                    int totalRows = dataTable.Rows.Count;
                    int totalBatches = (int)Math.Ceiling((double)totalRows / batchSize);
                    var intakes = new List<List<int>>();
                    for (int i = 0; i < totalBatches; i++)
                    {
                        // Get a subset of the DataTable for the current batch
                        DataTable batchTable = dataTable.Clone();
                        for (int j = 0; j < batchSize && (i * batchSize) + j < totalRows; j++)
                        {
                            batchTable.ImportRow(dataTable.Rows[(i * batchSize) + j]);
                        }
                        foreach (DataRow row in batchTable.Rows)
                        {
                           
                        ProgramMaster objProgramMaster = new ProgramMaster();
                        objProgramMaster.ProgramName = row["Program Name"].ToString().Trim();
                        objProgramMaster.Duration = row["Duration"].ToString().Trim();
                        string Intake = row["Intake"].ToString().Trim();
                            string CostOfLiving = row["Cost of Living"].ToString().Trim();
                            string ApplicationFee = row["Application Fee"].ToString().Trim();
                            string TuitionFee = row["Tuition Fee"].ToString().Trim();
                            string MinimumDeposit = row["Minimum Deposit"].ToString().Trim();
                            var outputCostOfLiving = CurrencyHelper.ProcessCurrencyValue(CostOfLiving);
                            objProgramMaster.CostOfLiving = outputCostOfLiving.amount;
                            objProgramMaster.Currency = outputCostOfLiving.currency;
                            outputCostOfLiving = CurrencyHelper.ProcessCurrencyValue(ApplicationFee);
                            objProgramMaster.ApplicationFee = outputCostOfLiving.amount;
                            outputCostOfLiving = CurrencyHelper.ProcessCurrencyValue(TuitionFee);
                            objProgramMaster.TutionFee = outputCostOfLiving.amount;
                            outputCostOfLiving = CurrencyHelper.ProcessCurrencyValue(MinimumDeposit);
                            objProgramMaster.MinimumDeposit = outputCostOfLiving.amount;
                            objProgramMaster.ProcessingTime = row["Processing Time"].ToString().Trim();
                            var intakeList = IntakeHelper.SplitIntakes(Intake);

                            // Initialize all intakes to "0"
                            objProgramMaster.Intake1 = objProgramMaster.Intake2 = objProgramMaster.Intake3 = objProgramMaster.Intake4 = "0";

                            if (intakeList.Count > 0)
                            {
                                // Assign values based on the count of intakeList
                                if (intakeList.Count >= 1)
                                {
                                    objProgramMaster.Intake1 = intakeList[0].Count > 0 ? intakeList[0][0].ToString() : "0";
                                }
                                if (intakeList.Count >= 2)
                                {
                                    objProgramMaster.Intake2 = intakeList[1].Count > 0 ? intakeList[1][0].ToString() : "0";
                                }
                                if (intakeList.Count >= 3)
                                {
                                    objProgramMaster.Intake3 = intakeList[2].Count > 0 ? intakeList[2][0].ToString() : "0";
                                }
                                if (intakeList.Count >= 4)
                                {
                                    objProgramMaster.Intake4 = intakeList[3].Count > 0 ? intakeList[3][0].ToString() : "0";
                                }
                            }
                            //    var intakeList = IntakeHelper.SplitIntakes(Intake);
                            //    // Initialize all intakes to "0"
                            //    objProgramMaster.Intake1 = objProgramMaster.Intake2 = objProgramMaster.Intake3 = objProgramMaster.Intake4 = "0";

                            //if (intakeList.Count > 0)
                            //{

                            //    // Define a list of properties
                            //    var properties = new List<Action<List<int>>>
                            //        {
                            //            intake => objProgramMaster.Intake1 = intake.Count > 0 ? string.Join(",", intake) : string.Empty,
                            //            intake => objProgramMaster.Intake2 = intake.Count > 1 ? string.Join(",", intake[1]) : string.Empty,
                            //            intake => objProgramMaster.Intake3 = intake.Count > 2 ? string.Join(",", intake[2]) : string.Empty,
                            //            intake => objProgramMaster.Intake4 = intake.Count > 3 ? string.Join(",", intake[3]) : string.Empty
                            //        };

                            //    for (int z = 0; z < intakeList.Count; z++)
                            //    {
                            //        if (z < properties.Count)
                            //        {
                            //            properties[z](intakeList[z]);
                            //        }
                            //    }
                            //}

                            AcademicLevel objAcademicLevel = new AcademicLevel();
                        objAcademicLevel.LevelName = row["Level of Education"].ToString().Trim();
                        objAcademicLevel.Description = null;// row["Description"].ToString().Trim();
                        objAcademicLevel.IsActive = true;// Convert.ToBoolean(row["IsActive"]);
                        UniversityMaster objUniversityMaster = new UniversityMaster();
                        objUniversityMaster.Univ_Name = row["Institution Name"].ToString().Trim();
                        objUniversityMaster.Univ_Description = null;// row["Univ_Description"].ToString().Trim();
                        objUniversityMaster.Univ_Logo = null;// row["Univ_Logo"].ToString().Trim();
                        objUniversityMaster.Univ_Phone = null;// row["Univ_Phone"].ToString().Trim();
                        objUniversityMaster.Univ_Email = null;// row["Univ_Email"].ToString().Trim();
                        objUniversityMaster.Univ_Website = null;// row["Univ_Website"].ToString().Trim();
                        objUniversityMaster.Assigned_Users = null;//  row["Assigned_Users"].ToString().Trim();
                        objUniversityMaster.Is_Active = true;// Convert.ToBoolean(row["IsActive"]);
                        string countryName = row["Destination Country"].ToString().Trim();
                        int countryId = await GetCountryId(countryName);
                        int universityId = await GetOrInsertUniversityId(objUniversityMaster);
                        int academicLevelId = await GetOrInsertAcademicLevelId(objAcademicLevel);
                        int programId = await GetOrInsertProgram(objProgramMaster);
                        int programDetailsId = await GetOrInsertProgramDetails(objProgramMaster);
                            await GetInsertUniversityMapping(countryId, universityId, academicLevelId, programId, programDetailsId);
                    }
                }
                    result = "Uploaded Succefully";
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;

            }
              
            }
       
        }

    }

