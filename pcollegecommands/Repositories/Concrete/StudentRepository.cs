using System.Data;
using System.Data.Common;
using System.Reflection.Metadata;
using Flyurdreamcommands.Helpers;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Models.Enum;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class StudentRepository : DataRepositoryBase, IStudentRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        protected readonly IAddressRepository _addressRepository;
        DataTables objDataTables = new DataTables();
        public StudentRepository(IConfiguration config, ILogger<EnquiryRepository> logger, IAddressRepository addressRepository) : base(logger, config)
        {
            _config = config;
            _logger = logger;
            _addressRepository = addressRepository;
        }
        public async Task<Student> UpsertStudentAsync(Student student, int? companyId, int? branchId, SqlTransaction transaction)
        {

            using (SqlCommand cmd = new SqlCommand("upsert_student_personal_information", transaction.Connection, transaction))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@student_id", student.StudentId == 0 ? (object)DBNull.Value : student.StudentId);
                cmd.Parameters.AddWithValue("@first_name", student.FirstName);
                cmd.Parameters.AddWithValue("@last_name", student.LastName);
                cmd.Parameters.AddWithValue("@gender", student.Gender);
                cmd.Parameters.AddWithValue("@date_of_birth", student.DateOfBirth);
                cmd.Parameters.AddWithValue("@marital_status", student.MaritalStatus);
                cmd.Parameters.AddWithValue("@mobile", student.Mobile);
                cmd.Parameters.AddWithValue("@dial", student.Dial);
                cmd.Parameters.AddWithValue("@email", student.Email);
                cmd.Parameters.AddWithValue("@CompanyId", companyId);
                cmd.Parameters.AddWithValue("@BranchId", branchId);
                cmd.Parameters.AddWithValue("@IsActive", student.IsActive);
                cmd.Parameters.AddWithValue("@created_by", student.CreatedBy);

                SqlParameter successMessage = new SqlParameter("@success_message", SqlDbType.VarChar, 255)
                {
                    Direction = ParameterDirection.Output
                };
                SqlParameter scopeIdentity = new SqlParameter("@StudId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(scopeIdentity);
                cmd.Parameters.Add(successMessage);
                await cmd.ExecuteNonQueryAsync();
                student.StudentId = Convert.ToInt32(scopeIdentity.Value);
                student.Response = successMessage.Value.ToString();
                return student;
            }
        }

        public async Task<Student> UpdateStudentndAddress(Student student, int? companyId, int? branchId)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // Check if user or user.Address is null
                    if (student == null)
                    {
                        throw new ArgumentException("User object is null");
                    }


                    // Upsert user details
                    student = await UpsertStudentAsync(student, companyId, branchId, transaction);
                    // Iterate through each address in the user.Address list

                    var address = student.Address;
                    if (address.IsUpdate)
                    {
                        if (address == null)
                        {
                            transaction.Commit();
                            await connection.CloseAsync();
                            return student; // throw new ArgumentException($"Address is null");
                        }
                        if (address.Address == null)
                        {
                            transaction.Commit();
                            await connection.CloseAsync();
                            return student;
                            //throw new ArgumentException($"Address object is null");
                        }

                        address.StudentId = student.StudentId;
                        // Upsert address details
                        address.Address = await _addressRepository.UpsertAddressAsync(address.Address, transaction);
                        // Upsert user address mapping and get updated UserAddress
                        student.Address = await _addressRepository.UpsertStudentAddressesAsync(address, transaction);
                    }

                    // Commit transaction
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    transaction.Rollback();
                    throw new Exception("Error updating student addresses", ex);
                }
                finally
                {
                    // Close connection
                    await connection.CloseAsync();
                }
            }
            return student;
        }

        public async Task<Student> GetStudentByIdAsync(int studentId)
        {
            Student student = null;

            using (SqlConnection conn = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                string sql = "SELECT * FROM student_personal_information WHERE student_id = @student_id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@student_id", studentId);

                    await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            student = new Student
                            {
                                StudentId = (int)reader["student_id"],
                                FirstName = reader["first_name"].ToString(),
                                Student_UniqueId = reader["unique_id"].ToString(),
                                LastName = reader["last_name"].ToString(),
                                Gender = reader["gender"].ToString(),
                                DateOfBirth = (DateTime)reader["date_of_birth"],
                                MaritalStatus = reader["marital_status"].ToString(),
                                Mobile = reader["mobile"].ToString(),
                                Dial = reader["dial"].ToString(),
                                Email = reader["email"].ToString(),
                                IsActive = (int)reader["IsActive"],
                                CreatedBy = (int)reader["created_by"]
                            };
                        }
                    }
                }
            }

            return student;
        }

        public async Task<List<EducationData>> UpsertEducationAsync(List<EducationData> listEducationData)
        {
            List<EducationData> updatedListEducationData = new List<EducationData>();

            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                using (SqlCommand command = new SqlCommand("UpsertEducationDetails", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Create and populate table-valued parameter
                    DataTable educationDataTable = objDataTables.EducationDataTable(); // Ensure this method creates the DataTable correctly
                    foreach (var educationData in listEducationData)
                    {
                        educationDataTable.Rows.Add(
                            educationData.EducationId,
                            educationData.EducationLevel,
                            educationData.SchoolName,
                            educationData.Percentage,
                            educationData.Grade,
                            educationData.GPA,
                            educationData.EnglishMarks,
                            educationData.MathsMarks,
                            educationData.PhysicsMarks,
                            educationData.ChemistryMarks,
                            educationData.StudentId
                        );
                    }

                    SqlParameter parameter = command.Parameters.AddWithValue("@EducationData", educationDataTable);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.EducationDetailsTableType"; // Ensure this matches your table type name

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
                                EducationData updatedEducationData = new EducationData
                                {
                                    // Adjust column indices based on your result set
                                    EducationId = reader.GetInt32(0),
                                    EducationLevel = reader.GetString(1),
                                    SchoolName = reader.GetString(2),
                                    Percentage = reader.GetDecimal(3),
                                    Grade = reader.GetString(4),
                                    GPA = reader.GetDecimal(5),
                                    EnglishMarks = reader.GetInt32(6),
                                    MathsMarks = reader.GetInt32(7),
                                    PhysicsMarks = reader.GetInt32(8),
                                    ChemistryMarks = reader.GetInt32(9),
                                    StudentId = reader.GetInt32(10)
                                };

                                updatedListEducationData.Add(updatedEducationData);
                            }
                        }
                    }

                    // Retrieve the success message from the output parameter
                    string successMessage = (string)successMessageParam.Value;

                    // Assign the success message to each item in the list
                    foreach (var educationData in updatedListEducationData)
                    {
                        educationData.Response = successMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error upserting education data: {ex.Message}");
                throw; // Throw exception to propagate it further if needed
            }

            return updatedListEducationData;
        }

        public async Task<List<VisaRefusal>> UpsertVisaRefusalsAsync(List<VisaRefusal> visaRefusals)
        {
            List<VisaRefusal> updatedVisaRefusals = new List<VisaRefusal>();

            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                using (var command = new SqlCommand("UpsertVisaRefusal", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Create and populate table-valued parameter
                    DataTable visaRefusalDataTable = objDataTables.VisaRefusalDataTable(); // Ensure this method creates the DataTable correctly
                    foreach (var visaRefusal in visaRefusals)
                    {
                        visaRefusalDataTable.Rows.Add(
                            visaRefusal.VisarefusalId,
                            visaRefusal.RefusalCountry,
                            visaRefusal.RefusalDate,
                            visaRefusal.Description,
                            visaRefusal.StudentId
                        );
                    }


                    SqlParameter tableParameter = command.Parameters.AddWithValue("@VisaRefusalData", visaRefusalDataTable);
                    tableParameter.SqlDbType = SqlDbType.Structured;
                    tableParameter.TypeName = "dbo.VisaRefusalTableType"; // Ensure this matches your table type name
                    // Add success message output parameter
                    SqlParameter successMessageParam = new SqlParameter("@SuccessMessage", SqlDbType.NVarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(successMessageParam);

                    await connection.OpenAsync();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                var updatedVisaRefusal = new VisaRefusal
                                {
                                    VisarefusalId = reader.GetInt32(0),
                                    RefusalCountry = reader.GetString(1),
                                    RefusalDate = reader.GetDateTime(2),
                                    Description = reader.GetString(3),
                                    StudentId = reader.GetInt32(4)
                                };

                                updatedVisaRefusals.Add(updatedVisaRefusal);
                            }
                        }
                    }
                    string successMessage = (string)successMessageParam.Value;

                    // Assign the success message to each item in the list
                    foreach (var visaRefusal in updatedVisaRefusals)
                    {
                        visaRefusal.Response = successMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }

            return updatedVisaRefusals;
        }
        public async Task<List<WorkExperience>> UpsertWorkExperienceAsync(List<WorkExperience> listWorkExperienceData)
        {
            List<WorkExperience> updatedListWorkExperienceData = new List<WorkExperience>();

            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                using (SqlCommand command = new SqlCommand("UpsertWorkExperiences", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Create and populate table-valued parameter
                    DataTable workExperienceDataTable = objDataTables.WorkExperienceDataTable(); // Ensure this method creates the DataTable correctly
                    foreach (var workExperienceData in listWorkExperienceData)
                    {
                        workExperienceDataTable.Rows.Add(
                            workExperienceData.WorkExperienceId,
                            workExperienceData.StartDate,
                            workExperienceData.EndDate,
                            workExperienceData.Designation,
                            workExperienceData.Responsibilities,
                            workExperienceData.ProjectName,
                            workExperienceData.CreatedOn,
                            workExperienceData.UpdatedOn,
                            workExperienceData.CreatedBy,
                            workExperienceData.StudentId,
                            workExperienceData.UpdatedBy,
                            workExperienceData.CompanyName,
                            workExperienceData.Till_Now
                        );
                    }

                    SqlParameter parameter = command.Parameters.AddWithValue("@WorkExperiences", workExperienceDataTable);
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.WorkExperienceType"; // Ensure this matches your table type name

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
                                WorkExperience updatedWorkExperienceData = new WorkExperience
                                {
                                    // Adjust column indices based on your result set
                                    WorkExperienceId = reader.GetInt32(0),
                                    StartDate = reader.GetDateTime(1),
                                    EndDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                    Designation = reader.GetString(3),
                                    Responsibilities = reader.GetString(4),
                                    ProjectName = reader.GetString(5),
                                    CreatedOn = reader.GetDateTime(6),
                                    UpdatedOn = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                                    CreatedBy = reader.GetInt32(8),
                                    StudentId = reader.GetInt32(9),
                                    UpdatedBy = reader.GetInt32(10),
                                    CompanyName = reader.GetString(11),
                                    Till_Now = reader.GetBoolean(12)
                                };

                                updatedListWorkExperienceData.Add(updatedWorkExperienceData);
                            }
                        }
                    }

                    // Retrieve the success message from the output parameter
                    string successMessage = (string)successMessageParam.Value;

                    // Assign the success message to each item in the list
                    foreach (var workExperienceData in updatedListWorkExperienceData)
                    {
                        workExperienceData.Response = successMessage;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error upserting work experience data: {ex.Message}");
                throw; // Throw exception to propagate it further if needed
            }

            return updatedListWorkExperienceData;
        }
        public async Task<List<EnglishExam>> UpsertEnglishExamscoreAchieved(List<EnglishExam> listexamAchievedData)
        {
            string successMessage = "";
            List<EnglishExam> outputTable = new List<EnglishExam>();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    await connection.OpenAsync(); // Open connection asynchronously

                    using (SqlCommand command = new SqlCommand("UpsertEnglishExamAchieved", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Create TVP DataTable
                        var tvp = objDataTables.EnglishExamScoreDataTable();
                        // Add data from listexamAchievedData to TVP
                        foreach (var examAchieved in listexamAchievedData)
                        {
                            tvp.Rows.Add(
                                examAchieved.ExamRequirementId,
                                examAchieved.ExamTypeId,
                                examAchieved.StudentId,
                                examAchieved.ListeningScore,
                                examAchieved.SpeakingScore,
                                examAchieved.ReadingScore,
                                examAchieved.WritingScore,
                                examAchieved.VerbalReasoningScore,
                                examAchieved.QuantitativeReasoningScore,
                                examAchieved.AnalyticalWritingScore,
                                examAchieved.MinimumScore,
                                examAchieved.OverallScore,
                                examAchieved.LiteracyScore,
                                examAchieved.ConversationScore,
                                examAchieved.ComprehensionScore,
                                examAchieved.ProductionScore,
                                examAchieved.Createdon
                            );
                        }

                        // Add TVP parameter to command
                        SqlParameter tvpParam = command.Parameters.AddWithValue("@ExamAchievedTable", tvp);
                        tvpParam.SqlDbType = SqlDbType.Structured;

                        // Output parameter for success message
                        command.Parameters.Add("@SuccessMessage", SqlDbType.VarChar, 255).Direction = ParameterDirection.Output;

                        // Execute command and read result asynchronously
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {

                            while (reader.Read())
                            {
                                EnglishExam examAchieved = new EnglishExam();
                                examAchieved.ExamRequirementId = reader.GetInt32("ExamRequirementId"); // Adjust column index if necessary
                                outputTable.Add(examAchieved);
                            }
                        }
                        successMessage = command.Parameters["@SuccessMessage"].Value.ToString();

                        // Retrieve success message from output parameter after reader closes
                        listexamAchievedData = listexamAchievedData.Zip(outputTable, (original, updated) =>
                        {
                            original.ExamRequirementId = updated.ExamRequirementId;
                            original.Response = successMessage;
                            return original;
                        }).ToList();


                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error upserting EnglishExamAchieved records", ex);
            }

            return listexamAchievedData;
        }


        public async Task<StudentDetails> GetStudentDetailsAsyncByStudentId_1(string UniqueId)
        {
            StudentDetails studentDetails = new StudentDetails();
            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("GetStudentDetailsByUniqueId", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UniqueId", UniqueId);  // Replace 'stud18' with the actual unique ID you want to query

                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {


                            while (reader.Read())
                            {
                                // Populate Student Information
                                if (studentDetails.Student == null)
                                {
                                    studentDetails.Student = new Student
                                    {
                                        StudentId = reader.GetInt32(reader.GetOrdinal("student_id")),
                                        FirstName = reader.IsDBNull(reader.GetOrdinal("first_name"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("first_name")),
                                        LastName = reader.IsDBNull(reader.GetOrdinal("last_name"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("last_name")),
                                        Gender = reader.IsDBNull(reader.GetOrdinal("gender"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("gender")),
                                        DateOfBirth = reader.IsDBNull(reader.GetOrdinal("date_of_birth"))
                                            ? DateTime.Now
                                            : reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                                        MaritalStatus = reader.IsDBNull(reader.GetOrdinal("marital_status"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("marital_status")),
                                        Mobile = reader.IsDBNull(reader.GetOrdinal("mobile"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("mobile")),
                                        Dial = reader.IsDBNull(reader.GetOrdinal("dial"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("dial")),
                                        Email = reader.IsDBNull(reader.GetOrdinal("email"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("email")),
                                        CreatedBy = reader.GetInt32(reader.GetOrdinal("created_by")),
                                        CreatedOn = reader.IsDBNull(reader.GetOrdinal("created_on"))
                                            ? DateTime.Now
                                            : reader.GetDateTime(reader.GetOrdinal("created_on")),
                                        Student_UniqueId = reader.IsDBNull(reader.GetOrdinal("unique_id"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("unique_id")),
                                        IsActive = reader.GetInt32(reader.GetOrdinal("StudentIsActive"))
                                    };
                                }



                                // Populate Education Data
                                if (!reader.IsDBNull(reader.GetOrdinal("education_details_Id")))
                                {
                                    if (studentDetails.EducationData == null)
                                    {
                                        studentDetails.EducationData = new List<EducationData>();
                                    }
                                    EducationData education = new EducationData
                                    {
                                        EducationId = reader.GetInt32(reader.GetOrdinal("education_details_Id")),
                                        EducationLevel = reader.IsDBNull(reader.GetOrdinal("education_level"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("education_level")),
                                        SchoolName = reader.IsDBNull(reader.GetOrdinal("school_name"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("school_name")),
                                        Percentage = reader.IsDBNull(reader.GetOrdinal("percentage"))
                                        ? (decimal?)null
                                        : reader.GetDecimal(reader.GetOrdinal("percentage")),
                                        Grade = reader.IsDBNull(reader.GetOrdinal("grade"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("grade")),
                                        GPA = reader.IsDBNull(reader.GetOrdinal("gpa"))
                                        ? (decimal?)null
                                        : reader.GetDecimal(reader.GetOrdinal("gpa")),
                                        EnglishMarks = reader.IsDBNull(reader.GetOrdinal("english_marks"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("english_marks")),
                                        MathsMarks = reader.IsDBNull(reader.GetOrdinal("maths_marks"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("maths_marks")),
                                        PhysicsMarks = reader.IsDBNull(reader.GetOrdinal("physics_marks"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("physics_marks")),
                                        ChemistryMarks = reader.IsDBNull(reader.GetOrdinal("chemistry_marks"))
                                        ? (int?)null
                                        : reader.GetInt32(reader.GetOrdinal("chemistry_marks"))
                                    };

                                    studentDetails.EducationData.Add(education);
                                }


                                // Populate english language  Data
                                // Populate Work Experience
                                // Populate English language data
                                if (!reader.IsDBNull(reader.GetOrdinal("ExamRequirementId")))
                                {
                                    if (studentDetails.EnglishExamAchieved == null)
                                    {
                                        studentDetails.EnglishExamAchieved = new List<EnglishExam>();
                                    }

                                    EnglishExam englishExamAchieved = new EnglishExam
                                    {
                                        ExamRequirementId = reader.GetInt32(reader.GetOrdinal("ExamRequirementId")),
                                        ExamTypeId = reader.GetInt32(reader.GetOrdinal("ExamTypeId")),
                                        ListeningScore = reader.IsDBNull(reader.GetOrdinal("ListeningScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("ListeningScore")),
                                        SpeakingScore = reader.IsDBNull(reader.GetOrdinal("SpeakingScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("SpeakingScore")),
                                        ReadingScore = reader.IsDBNull(reader.GetOrdinal("ReadingScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("ReadingScore")),
                                        WritingScore = reader.IsDBNull(reader.GetOrdinal("WritingScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("WritingScore")),
                                        VerbalReasoningScore = reader.IsDBNull(reader.GetOrdinal("VerbalReasoningScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("VerbalReasoningScore")),
                                        QuantitativeReasoningScore = reader.IsDBNull(reader.GetOrdinal("QuantitativeReasoningScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("QuantitativeReasoningScore")),
                                        AnalyticalWritingScore = reader.IsDBNull(reader.GetOrdinal("AnalyticalWritingScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("AnalyticalWritingScore")),
                                        MinimumScore = reader.IsDBNull(reader.GetOrdinal("MinimumScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("MinimumScore")),
                                        OverallScore = reader.IsDBNull(reader.GetOrdinal("OverallScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("OverallScore")),
                                        LiteracyScore = reader.IsDBNull(reader.GetOrdinal("LiteracyScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("LiteracyScore")),
                                        ConversationScore = reader.IsDBNull(reader.GetOrdinal("ConversationScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("ConversationScore")),
                                        ComprehensionScore = reader.IsDBNull(reader.GetOrdinal("ComprehensionScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("ComprehensionScore")),
                                        ProductionScore = reader.IsDBNull(reader.GetOrdinal("ProductionScore"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("ProductionScore")),
                                        Createdon = reader.IsDBNull(reader.GetOrdinal("EnglishExamCreatedOn"))
                                            ? DateTime.Now
                                            : reader.GetDateTime(reader.GetOrdinal("EnglishExamCreatedOn")),
                                    };

                                    studentDetails.EnglishExamAchieved.Add(englishExamAchieved);
                                }

                                // Populate Work Experience
                                if (!reader.IsDBNull(reader.GetOrdinal("WorkExperienceId")))
                                {
                                    if (studentDetails.WorkExperience == null)
                                    {
                                        studentDetails.WorkExperience = new List<WorkExperience>();
                                    }
                                    WorkExperience workExperience = new WorkExperience
                                    {
                                        WorkExperienceId = reader.GetInt32(reader.GetOrdinal("WorkExperienceId")),
                                        StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                        EndDate = reader.IsDBNull(reader.GetOrdinal("EndDate"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                        Designation = reader.IsDBNull(reader.GetOrdinal("Designation"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("Designation")),
                                        Responsibilities = reader.IsDBNull(reader.GetOrdinal("Responsibilities"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("Responsibilities")),
                                        ProjectName = reader.IsDBNull(reader.GetOrdinal("ProjectName"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("ProjectName")),
                                        CreatedOn = reader.IsDBNull(reader.GetOrdinal("WorkExperienceCreatedOn"))
                                        ? DateTime.Now
                                        : reader.GetDateTime(reader.GetOrdinal("WorkExperienceCreatedOn")),
                                        UpdatedOn = reader.IsDBNull(reader.GetOrdinal("WorkExperienceUpdatedOn"))
                                        ? (DateTime?)null
                                        : reader.GetDateTime(reader.GetOrdinal("WorkExperienceUpdatedOn"))
                                    };
                                    studentDetails.WorkExperience.Add(workExperience);
                                }

                                // Populate Emergency Contact
                                if (!reader.IsDBNull(reader.GetOrdinal("EmergencyId")))
                                {
                                    if (studentDetails.EmergencyContact == null)
                                    {
                                        studentDetails.EmergencyContact = new List<EmergencyContact>();
                                    }
                                    EmergencyContact emergencyContact = new EmergencyContact
                                    {
                                        EmergencyId = reader.GetInt32(reader.GetOrdinal("EmergencyId")),
                                        FirstName = reader.IsDBNull(reader.GetOrdinal("EmergencyContactFirstName"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("EmergencyContactFirstName")),
                                        LastName = reader.IsDBNull(reader.GetOrdinal("EmergencyContactLastName"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("EmergencyContactLastName")),
                                        Telephone = reader.IsDBNull(reader.GetOrdinal("EmergencyContactTelephone"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("EmergencyContactTelephone")),
                                        Email = reader.IsDBNull(reader.GetOrdinal("EmergencyContactEmail"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("EmergencyContactEmail"))
                                    };
                                    studentDetails.EmergencyContact.Add(emergencyContact);
                                }

                                if (!reader.IsDBNull(reader.GetOrdinal("VisarefusalId")))
                                {
                                    if (studentDetails.VisaRefusal == null)
                                    {
                                        studentDetails.VisaRefusal = new List<VisaRefusal>();
                                    }
                                    VisaRefusal visaRefusal = new VisaRefusal
                                    {
                                        VisarefusalId = reader.GetInt32(reader.GetOrdinal("VisarefusalId")),
                                        RefusalCountry = reader.IsDBNull(reader.GetOrdinal("RefusalCountry"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("RefusalCountry")),
                                        RefusalDate = reader.IsDBNull(reader.GetOrdinal("RefusalDate"))
                                        ? DateTime.Now  // Handle as nullable DateTime
                                        : reader.GetDateTime(reader.GetOrdinal("RefusalDate")),
                                        Description = reader.IsDBNull(reader.GetOrdinal("VisaRefusalDescription"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("VisaRefusalDescription"))
                                    };

                                    studentDetails.VisaRefusal.Add(visaRefusal);
                                }

                                // Ensure the DocumentUploadModel is initialized
                                if (studentDetails.DocumentUploadModel == null)
                                {
                                    studentDetails.DocumentUploadModel = new DocumentUploadModel
                                    {
                                        // Initialize the collections
                                        ListStudentDocument = new List<StudentDocument>(),
                                        Documents = new List<Models.Datafields.Document>()
                                    };
                                }
                                // Process each record from the SqlDataReader
                                //while (reader.Read())
                                //{
                                // Handling StudentDocument
                                // Populate StudentDocument
                                if (!reader.IsDBNull(reader.GetOrdinal("StudentDocumentId")))
                                {
                                    StudentDocument studentDocument = new StudentDocument
                                    {
                                        StudentDocumentId = reader.GetInt32(reader.GetOrdinal("StudentDocumentId")),
                                        // Handle as int with default value of 0
                                        //CompanyId = reader.IsDBNull(reader.GetOrdinal("CompanyId")) ? 0 : reader.GetInt32(reader.GetOrdinal("CompanyId")),
                                        //  BranchId = reader.IsDBNull(reader.GetOrdinal("BranchId")) ? 0 : reader.GetInt32(reader.GetOrdinal("BranchId")),
                                        DocumentId = reader.GetInt32(reader.GetOrdinal("document_id")),
                                        StudentId = studentDetails.Student.StudentId,
                                        // UniqueId = reader.IsDBNull(reader.GetOrdinal("UniqueId")) ? null : reader.GetString(reader.GetOrdinal("UniqueId")),
                                        //sResponse = reader.IsDBNull(reader.GetOrdinal("Response")) ? null : reader.GetString(reader.GetOrdinal("Response"))
                                    };
                                    studentDetails.DocumentUploadModel.ListStudentDocument.Add(studentDocument);
                                }

                                // Handling Document
                                if (!reader.IsDBNull(reader.GetOrdinal("document_id")))
                                {
                                    Models.Datafields.Document document = new Models.Datafields.Document
                                    {
                                        DocumentId = studentDetails.DocumentUploadModel.ListStudentDocument[0].DocumentId,// reader.GetInt32(reader.GetOrdinal("document_id")),
                                        DocumentName = reader.IsDBNull(reader.GetOrdinal("document_name")) ? null : reader.GetString(reader.GetOrdinal("document_name")),
                                        FilePath = reader.IsDBNull(reader.GetOrdinal("file_path")) ? null : reader.GetString(reader.GetOrdinal("file_path")),
                                        ContainerName = reader.IsDBNull(reader.GetOrdinal("container_name")) ? null : reader.GetString(reader.GetOrdinal("container_name")),
                                        //UploadedBy = reader.IsDBNull(reader.GetOrdinal("uploaded_by")) ? null : reader.GetString(reader.GetOrdinal("uploaded_by")),
                                        //UploadedAt = reader.IsDBNull(reader.GetOrdinal("uploaded_at")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("uploaded_at")),
                                        //// Handle DocumentType and Content if needed
                                        // DocumentType = reader.IsDBNull(reader.GetOrdinal("document_type")) ? default(DocumentType) : reader.GetFieldValue<DocumentType>(reader.GetOrdinal("document_type")),
                                        // IsUpload = reader.IsDBNull(reader.GetOrdinal("is_upload")) ? false : reader.GetBoolean(reader.GetOrdinal("is_upload"))
                                    };
                                    studentDetails.DocumentUploadModel.Documents.Add(document);
                                }

                                // }

                            }

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return studentDetails;
        }
        public async Task<StudentDetails> GetStudentDetailsAsyncByStudentId(string uniqueId)
        {
            StudentDetails studentDetails = new StudentDetails();

            try
            {
                using (SqlConnection conn = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("GetStudentDetailsByStudentId", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@UniqueId", uniqueId);

                        await conn.OpenAsync();

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            do
                            {
                                if (reader.HasRows)
                                {
                                    //var columnNames = Enumerable.Range(0, reader.FieldCount)
                                    //                            .Select(i => reader.GetName(i))
                                    //                            .ToArray();

                                    //// Log column names for debugging
                                    //_logger.LogInformation("Columns: " + string.Join(", ", columnNames));

                                    if (reader.Read())
                                    {
                                        if (studentDetails.Student == null)
                                        {
                                            studentDetails.Student = new Student
                                            {
                                                StudentId = reader.GetInt32OrDefault("student_id"),
                                                FirstName = reader.GetSafeString("first_name"),
                                                LastName = reader.GetSafeString("last_name"),
                                                Gender = reader.GetSafeString("gender"),
                                                DateOfBirth = reader.GetSafeDateTime("date_of_birth"),
                                                MaritalStatus = reader.GetSafeString("marital_status"),
                                                Mobile = reader.GetSafeString("mobile"),
                                                Dial = reader.GetSafeString("dial"),
                                                Email = reader.GetSafeString("email"),
                                                CreatedBy = reader.GetInt32OrDefault("created_by"),
                                                CreatedOn = reader.GetSafeDateTime("created_on"),
                                                Student_UniqueId = reader.GetSafeString("unique_id"),
                                                IsActive = reader.GetInt32OrDefault("StudentIsActive")
                                            };
                                        }
                                    }
                                    if (reader.NextResult() && reader.HasRows)
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("AddressID"))
                                            {
                                                studentDetails.Student.Address ??= new StudentAddress
                                                {
                                                    Address = new Address
                                                    {
                                                        AddressId = reader.GetInt32OrDefault("AddressID"),
                                                        HouseNumber = reader.GetSafeString("HouseNumber"),
                                                        BuildingName = reader.GetSafeString("BuildingName"),
                                                        AddressLine1 = reader.GetSafeString("AddressLine1"),
                                                        AddressLine2 = reader.GetSafeString("AddressLine2"),
                                                        CityID = reader.GetInt32OrDefault("CityID"),
                                                        DistrictID = reader.GetInt32OrDefault("DistrictID"),
                                                        StateID = reader.GetInt32OrDefault("StateID"),
                                                        CountryID = reader.GetInt32OrDefault("CountryID"),
                                                        ZipCode = reader.GetSafeString("ZipCode"),
                                                        IsActive = reader.GetBoolean("AddressIsActive"),
                                                        AddressType = (AddressType)reader.GetInt32OrDefault("AddressType") // Assuming AddressType is an enum
                                                    }
                                                };
                                            }
                                        }
                                    }

                                    //if (reader.NextResult()) // Move to the next result set
                                    //{
                                    //    while (reader.Read())
                                    //    {
                                    //        if (reader.HasColumn("AddressID"))
                                    //        {
                                    //            studentDetails.Student.Address ??= new Address;

                                    //            EducationData education = new EducationData
                                    //            {
                                    //                EducationId = reader.GetInt32OrDefault("education_details_Id"),
                                    //                EducationLevel = reader.GetSafeString("education_level"),
                                    //                SchoolName = reader.GetSafeString("school_name"),
                                    //                Percentage = reader.GetSafeDecimal("percentage"),
                                    //                Grade = reader.GetSafeString("grade"),
                                    //                GPA = reader.GetSafeDecimal("gpa"),
                                    //                EnglishMarks = reader.GetSafeInt("english_marks"),
                                    //                MathsMarks = reader.GetSafeInt("maths_marks"),
                                    //                PhysicsMarks = reader.GetSafeInt("physics_marks"),
                                    //                ChemistryMarks = reader.GetSafeInt("chemistry_marks")
                                    //            };

                                    //            studentDetails.EducationData.Add(education);
                                    //        }
                                    //    }
                                    //}


                                    if (reader.NextResult()) // Move to the next result set
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("education_details_Id"))
                                            {
                                                studentDetails.EducationData ??= new List<EducationData>();

                                                EducationData education = new EducationData
                                                {
                                                    EducationId = reader.GetInt32OrDefault("education_details_Id"),
                                                    EducationLevel = reader.GetSafeString("education_level"),
                                                    SchoolName = reader.GetSafeString("school_name"),
                                                    Percentage = reader.GetSafeDecimal("percentage"),
                                                    Grade = reader.GetSafeString("grade"),
                                                    GPA = reader.GetSafeDecimal("gpa"),
                                                    EnglishMarks = reader.GetSafeInt("english_marks"),
                                                    MathsMarks = reader.GetSafeInt("maths_marks"),
                                                    PhysicsMarks = reader.GetSafeInt("physics_marks"),
                                                    ChemistryMarks = reader.GetSafeInt("chemistry_marks")
                                                };

                                                studentDetails.EducationData.Add(education);
                                            }
                                        }
                                    }

                                    if (reader.NextResult()) // Move to the next result set
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("EmergencyId"))
                                            {
                                                studentDetails.EmergencyContact ??= new List<EmergencyContact>();

                                                EmergencyContact emergencyContact = new EmergencyContact
                                                {
                                                    EmergencyId = reader.GetInt32OrDefault("EmergencyId"),
                                                    FirstName = reader.GetSafeString("EmergencyContactFirstName"),
                                                    LastName = reader.GetSafeString("EmergencyContactLastName"),
                                                    Telephone = reader.GetSafeString("EmergencyContactTelephone"),
                                                    Email = reader.GetSafeString("EmergencyContactEmail")
                                                };

                                                studentDetails.EmergencyContact.Add(emergencyContact);
                                            }
                                        }
                                    }

                                    if (reader.NextResult()) // Move to the next result set
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("WorkExperienceId"))
                                            {
                                                studentDetails.WorkExperience ??= new List<WorkExperience>();

                                                WorkExperience workExperience = new WorkExperience
                                                {
                                                    WorkExperienceId = reader.GetInt32OrDefault("WorkExperienceId"),
                                                    StartDate = reader.GetSafeDateTime("StartDate"),
                                                    EndDate = reader.GetSafeDateTime("EndDate"),
                                                    Designation = reader.GetSafeString("Designation"),
                                                    Responsibilities = reader.GetSafeString("Responsibilities"),
                                                    ProjectName = reader.GetSafeString("ProjectName"),
                                                    CreatedOn = reader.GetSafeDateTime("WorkExperienceCreatedOn"),
                                                    CreatedBy = reader.GetInt32("WorkExperienceCreatedBy"),
                                                    UpdatedOn = reader.GetSafeDateTime("WorkExperienceUpdatedOn"),
                                                    UpdatedBy = reader.GetInt32("WorkExperienceUpdatedBy"),
                                                    CompanyName = reader.GetSafeString("CompanyName"),
                                                    Till_Now = reader.GetBooleanOrDefault("Till_Now")
                                                };

                                                studentDetails.WorkExperience.Add(workExperience);
                                            }
                                        }
                                    }



                                    if (reader.NextResult()) // Move to the next result set
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("VisarefusalId"))
                                            {
                                                studentDetails.VisaRefusal ??= new List<VisaRefusal>();

                                                VisaRefusal visaRefusal = new VisaRefusal
                                                {
                                                    VisarefusalId = reader.GetInt32OrDefault("VisarefusalId"),
                                                    RefusalCountry = reader.GetSafeString("RefusalCountry"),
                                                    RefusalDate = reader.GetSafeDateTime("RefusalDate"),
                                                    Description = reader.GetSafeString("VisaRefusalDescription")
                                                };

                                                studentDetails.VisaRefusal.Add(visaRefusal);
                                            }
                                        }
                                    }
                                    if (reader.NextResult()) // Move to the next result set
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("ExamRequirementId"))
                                            {
                                                studentDetails.EnglishExamAchieved ??= new List<EnglishExam>();

                                                EnglishExam englishExam = new EnglishExam
                                                {
                                                    ExamRequirementId = reader.GetInt32OrDefault("ExamRequirementId"),
                                                    ExamTypeId = reader.GetInt32OrDefault("ExamTypeId"),
                                                    ListeningScore = reader.GetSafeString("ListeningScore"),
                                                    SpeakingScore = reader.GetSafeString("SpeakingScore"),
                                                    ReadingScore = reader.GetSafeString("ReadingScore"),
                                                    WritingScore = reader.GetSafeString("WritingScore"),
                                                    VerbalReasoningScore = reader.GetSafeString("VerbalReasoningScore"),
                                                    QuantitativeReasoningScore = reader.GetSafeString("QuantitativeReasoningScore"),
                                                    AnalyticalWritingScore = reader.GetSafeString("AnalyticalWritingScore"),
                                                    MinimumScore = reader.GetSafeString("MinimumScore"),
                                                    OverallScore = reader.GetSafeString("OverallScore"),
                                                    LiteracyScore = reader.GetSafeString("LiteracyScore"),
                                                    ConversationScore = reader.GetSafeString("ConversationScore"),
                                                    ComprehensionScore = reader.GetSafeString("ComprehensionScore"),
                                                    ProductionScore = reader.GetSafeString("ProductionScore"),
                                                    Createdon = reader.GetSafeDateTime("EnglishExamCreatedOn")
                                                };

                                                studentDetails.EnglishExamAchieved.Add(englishExam);
                                            }
                                        }
                                    }


                                    if (reader.NextResult()) // Move to the next result set
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.HasColumn("StudentDocumentId"))
                                            {
                                                studentDetails.DocumentUploadModel ??= new DocumentUploadModel
                                                {
                                                    ListStudentDocument = new List<StudentDocument>(),
                                                    Documents = new List<Models.Datafields.Document>()
                                                };

                                                StudentDocument studentDocument = new StudentDocument
                                                {
                                                    StudentDocumentId = reader.GetInt32OrDefault("StudentDocumentId"),
                                                    DocumentId = reader.GetInt32OrDefault("document_id"),
                                                    StudentId = studentDetails.Student.StudentId
                                                };

                                                studentDetails.DocumentUploadModel.ListStudentDocument.Add(studentDocument);
                                            }

                                            if (reader.HasColumn("document_id"))
                                            {
                                                Models.Datafields.Document document = new Models.Datafields.Document
                                                {
                                                    DocumentId = reader.GetInt32OrDefault("document_id"),
                                                    DocumentName = reader.GetSafeString("document_name"),
                                                    FilePath = reader.GetSafeString("file_path"),
                                                    ContainerName = reader.GetSafeString("container_name")
                                                };

                                                studentDetails.DocumentUploadModel.Documents.Add(document);
                                            }
                                        }
                                    }
                                }
                            } while (await reader.NextResultAsync()); // Move to the next result set
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving student details.");
            }

            return studentDetails;
        }

        public async Task<string> UpsertApplicationDataAsync(Application applicationData)
        {
            string successMessage = string.Empty;

            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                using (var command = new SqlCommand("UpsertApplicationData", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Unique_ID", applicationData.Unique_ID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@First_Name", applicationData.First_Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Last_Name", applicationData.Last_Name ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DOB", applicationData.DOB);
                    command.Parameters.AddWithValue("@University", applicationData.University);
                    command.Parameters.AddWithValue("@Course", applicationData.Course);                    
                     command.Parameters.AddWithValue("@AcademicLevel", applicationData.AcademicLevel);
                    command.Parameters.AddWithValue("@Intake", applicationData.Intake);
                    command.Parameters.AddWithValue("@Assigned_Users", applicationData.Assigned_Users);
                    command.Parameters.AddWithValue("@Status", applicationData.Status);
                    command.Parameters.AddWithValue("@Created_By", applicationData.Created_By);
                    command.Parameters.AddWithValue("@Comments", applicationData.Comments ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EmailID", applicationData.EmailID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@student_unique_id", applicationData.StudentUniqueId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Passport_No", applicationData.Passport_No ?? (object)DBNull.Value);

                    SqlParameter successMessageParam = new SqlParameter("@SuccessMessage", SqlDbType.NVarChar, 255)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(successMessageParam);

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    successMessage = (string)successMessageParam.Value;
                }
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., logging)
                throw new ApplicationException("An error occurred while upserting application data.", ex);
            }

            return successMessage;
        }
        public async Task<List<Application>> GetApplicationDataByUniqueIDAsync(
    string? uniqueId,    string? passportNo, string? studentUniqueId, string? assignedUsers,
    int? createdBy,    string? emailId, int? status)
        {
            var applications = new List<Application>();

            try
            {
                using (var connection = new SqlConnection(ConfigurationData.DbConnectionString))
                using (var command = new SqlCommand("GetApplicationDataByUniqueID", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.AddWithValue("@Unique_ID", (object)uniqueId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Passport_No", (object)passportNo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StudentUniqueId", (object)studentUniqueId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Assigned_Users", (object)assignedUsers ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Created_By", (object)createdBy ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EmailID", (object)emailId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Status", (object)status ?? DBNull.Value);



                    await connection.OpenAsync();

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var application = new Application
                            {
                               // ApplicationId = reader.IsDBNull(reader.GetOrdinal("ApplicationId")) ? 0 : reader.GetInt32(reader.GetOrdinal("ApplicationId")),
                                Unique_ID = reader.IsDBNull(reader.GetOrdinal("Unique_ID")) ? null : reader.GetString(reader.GetOrdinal("Unique_ID")),
                                First_Name = reader.IsDBNull(reader.GetOrdinal("First_Name")) ? null : reader.GetString(reader.GetOrdinal("First_Name")),
                                Last_Name = reader.IsDBNull(reader.GetOrdinal("Last_Name")) ? null : reader.GetString(reader.GetOrdinal("Last_Name")),
                                DOB =  reader.GetDateTime(reader.GetOrdinal("DOB")),
                                University = reader.IsDBNull(reader.GetOrdinal("University")) ? 0 : reader.GetInt32(reader.GetOrdinal("University")),
                                Course = reader.IsDBNull(reader.GetOrdinal("Course")) ? 0 : reader.GetInt32(reader.GetOrdinal("Course")),
                                AcademicLevel = reader.IsDBNull(reader.GetOrdinal("AcademicLevel")) ? 0 : reader.GetInt32(reader.GetOrdinal("AcademicLevel")),
                                Intake = reader.IsDBNull(reader.GetOrdinal("Intake")) ? null : reader.GetString(reader.GetOrdinal("Intake")),
                                Assigned_Users = reader.IsDBNull(reader.GetOrdinal("Assigned_Users")) ? 0 : reader.GetInt32(reader.GetOrdinal("Assigned_Users")),
                                Status = reader.IsDBNull(reader.GetOrdinal("Status")) ? 0 : reader.GetInt32(reader.GetOrdinal("Status")),
                                Created_By = reader.IsDBNull(reader.GetOrdinal("Created_By")) ? 0 : reader.GetInt32(reader.GetOrdinal("Created_By")),
                                Created_On = reader.IsDBNull(reader.GetOrdinal("Created_On")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("Created_On")),
                                Comments = reader.IsDBNull(reader.GetOrdinal("Comments")) ? null : reader.GetString(reader.GetOrdinal("Comments")),
                                EmailID = reader.IsDBNull(reader.GetOrdinal("EmailID")) ? null : reader.GetString(reader.GetOrdinal("EmailID")),
                                Passport_No = reader.IsDBNull(reader.GetOrdinal("Passport_No")) ? null : reader.GetString(reader.GetOrdinal("Passport_No")),
                                Updated_On = reader.IsDBNull(reader.GetOrdinal("Updated_On")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Updated_On")),
                                Updated_By = reader.IsDBNull(reader.GetOrdinal("Updated_By")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Updated_By")),
                                StudentUniqueId = reader.IsDBNull(reader.GetOrdinal("student_unique_id")) ? null : reader.GetString(reader.GetOrdinal("student_unique_id"))
                            };

                            applications.Add(application);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception (e.g., logging)
                throw new ApplicationException("An error occurred while retrieving application data.", ex);
            }

            return applications;
        }
    }
}
