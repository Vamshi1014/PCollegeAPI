using System.Data;
using System.Reflection;
using System.Reflection.Metadata;
using Flyurdreamcommands.Constants;
using Flyurdreamcommands.Helpers;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Flyurdreamcommands.Constants;
using System.Data.Common;
using Flyurdreamcommands.Models.Enum;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Flyurdreamcommands.Repositories.Concrete
{
    //public static class SqlDataReaderExtensions
    //{
    //    public static string GetStringOrNull(this SqlDataReader reader, string column)
    //    {
    //        int index = reader.GetOrdinal(column);
    //        return reader.IsDBNull(index) ? null : reader.GetString(index);
    //    }

    //    public static int GetInt32OrDefault(this SqlDataReader reader, string column)
    //    {
    //        int index = reader.GetOrdinal(column);
    //        return reader.IsDBNull(index) ? 0 : reader.GetInt32(index);
    //    }

    //    public static bool? GetBooleanOrNull(this SqlDataReader reader, string column)
    //    {
    //        int index = reader.GetOrdinal(column);
    //        return reader.IsDBNull(index) ? (bool?)null : reader.GetBoolean(index);
    //    }
    //}


    public class UserRepository : DataRepositoryBase, IUserRepository
    {
        protected readonly IConfiguration _config;
        protected readonly ILogger _logger;
        protected readonly IEmailService _emailService;
        protected readonly IAddressRepository _addressRepository;
        public UserActivity userActivity;

        public UserRepository(ILogger<UserRepository> logger, IConfiguration config, IEmailService emailService,IAddressRepository addressRepository) : base(logger, config)
        {
            _config = config;
            _logger = logger;
            _emailService = emailService;
            _addressRepository =addressRepository;
        }
     
        public async Task<string> GetDatabsucces()
        {

            string success = string.Empty;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    connection.Open();
                    success = (connection.State == ConnectionState.Open) ? "Pass" : "Fail";
                }
            }

            catch (Exception ex)
            {
                return success = "Fail";
                throw ex.InnerException;
            }

            return await Task.FromResult(success);
        }
        

        public async Task<User> GetLoggedInUser(User appUser)
        {
            User user = new User();
            UserActivity userActivity = new UserActivity();
            string storedHash = string.Empty;
            byte[] storedSalt = new byte[100];
            Dictionary<int, CompanyBranches> companyBranchesMap = new Dictionary<int, CompanyBranches>();

            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            using (SqlCommand command = new SqlCommand("LoginUser", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@Email", appUser.Email));

                try
                {
                    await connection.OpenAsync();
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        // Read the first result set (Status)
                        if (await reader.ReadAsync()) // Use await here
                        {
                            user.Response = reader["Status"].ToString();
                            if (user.Response != "Login successful")
                            {
                                return user;
                            }
                            
                        }
                        else
                        {
                            user.Response = "No data found";
                            return user;
                        }

                        // Read the second result set (User details)
                        if (await reader.NextResultAsync()) // Ensure moving to the next result set
                        {
                            while (await reader.ReadAsync()) // Use await here to read each row
                            {
                               bool check = false;
                                if (!check)
                                {
                                    user.UserId = reader.GetInt32(reader.GetOrdinal("UserId"));
                                    user.FirstName = reader.GetSafeString("firstname");
                                    user.lastName = reader.GetSafeString("lastname");
                                    user.Email = appUser.Email;
                                    user.IsActive = reader.GetBoolean(reader.GetOrdinal("isActive"));
                                    user.UserVerified = reader.GetBoolean(reader.GetOrdinal("user_verified"));
                                    user.GroupId = reader.GetInt32(reader.GetOrdinal("groupId"));
                                    // Assuming PasswordHash and Salt are stored as string and byte[] respectively
                                    storedHash = reader["password_hash"].ToString();
                                    storedSalt = reader["Salt"] as byte[];
                                    check = true;
                                }                                  
                                 // Assuming multiple branches for the same user
                                int companyId = reader.GetInt32(reader.GetOrdinal("CompanyId"));

                                // Check if company already exists in the map
                                if (!companyBranchesMap.ContainsKey(companyId))
                                {
                                    Company objCompany = new Company
                                    {
                                        CompanyID = companyId,
                                        CompanyName = reader.GetSafeString("CompanyName"),
                                        CompanyWebAddress = reader.GetSafeString("CompanyWebAddress")
                                    };

                                    companyBranchesMap[companyId] = new CompanyBranches
                                    {
                                        Company = objCompany,
                                        Branches = new List<Branch>()
                                    };
                                }

                                // Create a new branch and add it to the company's branch list
                                Branch objBranch = new Branch
                                {
                                    BranchId = reader.GetInt32(reader.GetOrdinal("BranchId")),
                                    BranchName = reader.GetSafeString("BranchName")
                                };

                                companyBranchesMap[companyId].Branches.Add(objBranch);
                            }
                        }

                        user.ListCompanyBranches = companyBranchesMap.Values.ToList();

                       

                        // Validate password
                        if (!SecurityHelper.VerifyPassword(appUser.PasswordHash, storedHash, storedSalt))
                        {
                            user.Response = "Incorrect password";
                            userActivity.User_Activity = "Incorrect password";
                            InsertUserActivity(userActivity);
                            return user;
                        }
                        else
                        {
                            // Generate JWT token for authenticated user
                            // Generate JWT token and refresh token for authenticated user
                            string refreshToken;
                            (user.Token, user.RefreshToken) = SecurityHelper.GenerateJSONWebToken(user, _config);
                            if (!string.IsNullOrEmpty(user.RefreshToken))
                            {
                                // Convert the string to bytes
                                byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(user.RefreshToken);
                                // Encode the bytes to a Base64 string
                                user.RefreshToken = Convert.ToBase64String(tokenBytes);
                            }
                            await StoreRefreshTokenAsync(user.UserId, user.RefreshToken);
                        
                            // Store the refresh token in the database
                            

                        }
                        userActivity.UserId = user.UserId;
                        userActivity.User_Activity = "Login successful";
                        InsertUserActivity(userActivity);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    user.Response = "Error occurred";
                }
            }

            return user;
        }
        public async Task<User> UserRegistration(User appUser, HttpRequest request)
        {
            User user = new User(); userActivity = new UserActivity();

            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                SqlCommand command = new SqlCommand("RegisterUser", connection);
                command.CommandType = CommandType.StoredProcedure;
                // Set parameters
                (string hashedPassword, byte[] salt) = SecurityHelper.HashPassword(appUser.PasswordHash);

                // Add parameters with DBNull handling
                command.Parameters.Add(new SqlParameter("@firstname", appUser.FirstName ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@lastname", appUser.lastName ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@email", appUser.Email ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@password_hash", hashedPassword ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@Salt", salt ?? (object)DBNull.Value));

                user.Token = appUser.Token = Guid.NewGuid().ToString();
                user.FirstName = appUser.FirstName;
                user.VerificationUrl = SendVerificationEmail(user.Token, request);
                user.Email = appUser.Email;

                command.Parameters.Add(new SqlParameter("@token", appUser.Token));
                command.Parameters.Add(new SqlParameter("@verification_url", appUser.VerificationUrl ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@GroupId", appUser.GroupId));
                command.Parameters.Add(new SqlParameter("@salutation", appUser.Salutation ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@mobile", appUser.Mobile ?? (object)DBNull.Value));
                command.Parameters.Add(new SqlParameter("@countrycode", appUser.CountryCode));
                command.Parameters.Add(new SqlParameter("@createdby", appUser.Createdby));
                // Add output parameters
                command.Parameters.Add(new SqlParameter("@Status", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Direction = ParameterDirection.Output });
                connection.Open();
                await command.ExecuteNonQueryAsync();
                // Check output parameter
                user.Response = command.Parameters["@Status"].Value.ToString();
                CompanyUser objCompanyUser = new CompanyUser();
                if (command.Parameters["@UserID"].Value != DBNull.Value)
                {

                    objCompanyUser.User.UserId=user.UserId = Convert.ToInt16(command.Parameters["@UserID"].Value);
                }
              
                //objCompanyUser.User.UserId = user.UserId;
                objCompanyUser.IsParent = 1;
                //objCompanyUser.CompanyId = 1;

                //if (appUser.GroupId == 3)
                //{
                //    //Company Details Insert
                //    SqlCommand commandForCompany = new SqlCommand("UpsertCompanyDetails", connection);
                //    commandForCompany.CommandType = CommandType.StoredProcedure;
                //    commandForCompany.Parameters.AddWithValue("@CompanyId", DBNull.Value);
                //    commandForCompany.Parameters.AddWithValue("@CompanyName", string.Empty);
                //    commandForCompany.Parameters.AddWithValue("@BusinessRegistrationNumber", string.Empty);
                //    commandForCompany.Parameters.AddWithValue("@CompanyWebAddress", DBNull.Value);
                //    commandForCompany.Parameters.AddWithValue("@IsActive", string.Empty);
                //    SqlParameter resultAddressIdParam = new SqlParameter("@ResultAddressID", SqlDbType.Int)
                //    {
                //        Direction = ParameterDirection.Output
                //    };
                //    commandForCompany.Parameters.Add(resultAddressIdParam);
                //    SqlParameter successMessageParam = new SqlParameter("@SuccessMessage", SqlDbType.NVarChar, 100)
                //    {
                //        Direction = ParameterDirection.Output
                //    };
                //    commandForCompany.Parameters.Add(successMessageParam);
                //    await commandForCompany.ExecuteNonQueryAsync();
                //    var CompanyID = resultAddressIdParam.Value != DBNull.Value ? (int)resultAddressIdParam.Value : (int?)null;
                //    var Result = successMessageParam.Value as string;

                //    //Company User Insert
                //    SqlCommand commandForCompanyUser = new SqlCommand("UpsertCompanyUser", connection);
                //    commandForCompanyUser.CommandType = CommandType.StoredProcedure;

                //    // Add parameters to the stored procedure
                //    commandForCompanyUser.Parameters.AddWithValue("@CompanyUserId", 0);
                //    commandForCompanyUser.Parameters.AddWithValue("@UserId", objCompanyUser.User.UserId);
                //    commandForCompanyUser.Parameters.AddWithValue("@CompanyId", CompanyID);
                //    commandForCompanyUser.Parameters.AddWithValue("@IsPrimaryContact", 1);
                //    commandForCompanyUser.Parameters.AddWithValue("@BranchId", 0);
                //    commandForCompanyUser.Parameters.AddWithValue("@IsParent", 0);
                //    commandForCompanyUser.Parameters.Add("@CompanyUserIdOutput", SqlDbType.Int).Direction = ParameterDirection.Output;


                //    // Add an output parameter for @CompanyUserId

                //    // Execute the command
                //    await commandForCompanyUser.ExecuteNonQueryAsync();

                //    // Update the CompanyUserId property with the output parameter value
                //    var CompanyUserId = Convert.ToInt32(commandForCompanyUser.Parameters["@CompanyUserIdOutput"].Value);

                //}

                // Handle the status as needed
                switch (user.Response)
                {
                    case "Email already exists":
                          userActivity.UserId = user.UserId;
                          userActivity.User_Activity = Const.User_Already_Verified;                          
                        // Handle the case when email already exists
                        break;
                    case "User registered successfully":
                            userActivity.UserId = user.UserId;
                            UpsertCompanyBranchUserAsync(objCompanyUser);
                            Email mailArgs = new Email();
                            mailArgs.Subject = Const.VerifyEmailSubject;
                            mailArgs.Message = Const.RegistrationEmailMessage;
                            mailArgs.MailTo = appUser.Email;
                            _emailService.EmailNotification(user, mailArgs, null);
                            _emailService.InsertEmailHistory(user.UserId, mailArgs);
                            userActivity.User_Activity = Const.User_Registered_Succesfylly;
                        // Handle the case when registration is successful
                        break;
                    default:
                        // Handle unexpected status
                        break;
                }
                InsertUserActivity(userActivity);
                connection.Close();
            }

            return user;
        }
        public async Task<User> UpdateUser(User user, SqlTransaction transaction)
        {
            try
            {
                // Use the transaction's connection
                using (SqlCommand command = new SqlCommand("UpdateUser", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters with DBNull handling
                    command.Parameters.AddWithValue("@UserID", user.UserId);
                    command.Parameters.Add(new SqlParameter("@firstname", user.FirstName ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@lastname", user.lastName ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@salutation", user.Salutation ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@mobile", user.Mobile ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@countrycode", user.CountryCode));

                    // Add output parameters
                    SqlParameter statusParam = new SqlParameter("@Status", SqlDbType.VarChar, 100)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(statusParam);

                    await command.ExecuteNonQueryAsync();

                   user.Response = (string)statusParam.Value;
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw ex;
            }

            return user;
        }

        public async Task<User> UpdateUserAddress(User user)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                await connection.OpenAsync();
                SqlTransaction transaction = connection.BeginTransaction();
                try
                {
                    // Check if user or user.Address is null
                    if (user == null)
                    {
                        throw new ArgumentException("User object is null");
                    }

                    if (user.Address == null || user.Address.Count == 0)
                    {
                        throw new ArgumentException("User Address list is null or empty");
                    }

                    // Upsert user details
                    user = await UpdateUser(user, transaction);
                    // Iterate through each address in the user.Address list
                    for (int i = 0; i < user.Address.Count; i++)
                    {
                        var address = user.Address[i];
                        if (address.IsUpdate)
                        {
                            if (address == null)
                            {
                                transaction.Commit();
                                await connection.CloseAsync();
                                return user;
                            }
                            if (address.Addresses == null)
                            {
                                transaction.Commit();
                                await connection.CloseAsync();
                                return user; // throw new ArgumentException($"Address object at index {i} is null");
                            }
                            address.UserId = user.UserId;
                            // Upsert address details
                            address.Addresses = await _addressRepository.UpsertAddressAsync(address.Addresses, transaction);
                            // Upsert user address mapping and get updated UserAddress
                            if (address.UserAddressId == 0)
                            {
                                user.Address[i] = await _addressRepository.UpsertUserAddressesAsync(address, transaction);
                            }
                        }
                    }
                    // Commit transaction
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    // Rollback transaction on error
                    transaction.Rollback();
                    Console.WriteLine($"Error updating user addresses: {ex.Message}");
                    throw new Exception("Error updating user addresses", ex);
                }
                finally
                {
                    // Close connection
                    await connection.CloseAsync();
                }
            }
            return user;
        }


        public async Task<User> GetUserDetailsAsync(int? userId)
        {
            if (userId == null)
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null.");
            }

            User user = null; // Initialize user as null

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("GetUserDetails", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@p_UserId", userId.HasValue ? (object)userId.Value : DBNull.Value);

                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int userIdValue = reader.GetInt32(reader.GetOrdinal("UserId"));
                                int addressTypeValue = 0;
                                AddressType addressType = AddressType.None;

                                // Check if the "AddressType" column is NULL before calling GetInt32
                                if (!reader.IsDBNull(reader.GetOrdinal("AddressType")))
                                {
                                    addressTypeValue = reader.GetInt32(reader.GetOrdinal("AddressType"));

                                    // Attempt to parse the integer to the AddressType enum
                                    addressType = Enum.IsDefined(typeof(AddressType), addressTypeValue)
                                        ? (AddressType)addressTypeValue
                                        : AddressType.Home; // Default to Home if parsing fails
                                }

                                if (user == null)
                                {
                                    user = new User
                                    {
                                        UserId = userIdValue,
                                        Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
                                        CreatedAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ? DateTime.MinValue : reader.GetDateTime(reader.GetOrdinal("created_at")),
                                        Salutation = reader.IsDBNull(reader.GetOrdinal("salutation")) ? null : reader.GetString(reader.GetOrdinal("salutation")),
                                        GroupId = reader.GetInt32(reader.GetOrdinal("groupId")),
                                        FirstName = reader.IsDBNull(reader.GetOrdinal("firstname")) ? null : reader.GetString(reader.GetOrdinal("firstname")),
                                        lastName = reader.IsDBNull(reader.GetOrdinal("lastname")) ? null : reader.GetString(reader.GetOrdinal("lastname")),
                                        Mobile = reader.IsDBNull(reader.GetOrdinal("mobile")) ? null : reader.GetString(reader.GetOrdinal("mobile")),
                                        CountryCode = reader.GetInt32(reader.GetOrdinal("countrycode")),
                                        LastLogin = reader.IsDBNull(reader.GetOrdinal("Lastlogin")) ? DateTime.Now.AddDays(-1) : reader.GetDateTime(reader.GetOrdinal("Lastlogin")),
                                        Address = new List<UserAddress>() // Initialize the list of addresses
                                    };
                                }

                                var userAddress = new UserAddress
                                {
                                    UserAddressId = reader.IsDBNull(reader.GetOrdinal("UserAddressId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserAddressId")),
                                    UserId = userIdValue,
                                    Addresses = new Address
                                    {
                                        AddressId = reader.IsDBNull(reader.GetOrdinal("AddressID")) ? 0 : reader.GetInt32(reader.GetOrdinal("AddressID")),
                                        HouseNumber = reader.IsDBNull(reader.GetOrdinal("HouseNumber")) ? null : reader.GetString(reader.GetOrdinal("HouseNumber")),
                                        BuildingName = reader.IsDBNull(reader.GetOrdinal("BuildingName")) ? null : reader.GetString(reader.GetOrdinal("BuildingName")),
                                        AddressLine1 = reader.IsDBNull(reader.GetOrdinal("AddressLine1")) ? null : reader.GetString(reader.GetOrdinal("AddressLine1")),
                                        AddressLine2 = reader.IsDBNull(reader.GetOrdinal("AddressLine2")) ? null : reader.GetString(reader.GetOrdinal("AddressLine2")),
                                        CityID = reader.IsDBNull(reader.GetOrdinal("CityID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CityID")),
                                        DistrictID = reader.IsDBNull(reader.GetOrdinal("DistrictID")) ? 0 : reader.GetInt32(reader.GetOrdinal("DistrictID")),
                                        StateID = reader.IsDBNull(reader.GetOrdinal("StateID")) ? 0 : reader.GetInt32(reader.GetOrdinal("StateID")),
                                        CountryID = reader.IsDBNull(reader.GetOrdinal("CountryID")) ? 0 : reader.GetInt32(reader.GetOrdinal("CountryID")),
                                        ZipCode = reader.IsDBNull(reader.GetOrdinal("ZipCode")) ? null : reader.GetString(reader.GetOrdinal("ZipCode")),
                                        AddressType = addressType // Assign the parsed AddressType
                                    }
                                };

                                // Add the address to the user's list of addresses
                                if (user != null)
                                {
                                    user.Address.Add(userAddress);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching user details.");
                throw; // Re-throw the exception after logging it
            }

            return user; // Return the user object with addresses
        }


        public string SendVerificationEmail(string token, HttpRequest request)
        {
            // Get the root domain name
            string clienturl = _config["EmailVerificationClientUrl"];
            string rootDomain = SecurityHelper.GetRootDomain(request);           
            string verificationUrl = clienturl+="{token}";
            //string verificationUrl = $"{rootDomain}/api/emailVerification?{token}";


            // Example usage (e.g., setting it to a user object)
         

            // Perform further actions like sending email
            // ...

            // Return the verification URL
            return verificationUrl;
        }

        public async Task<string> VerifyEmail(string token)
        {
            string status = string.Empty;  userActivity = new UserActivity();

            // Create a SqlConnection and SqlCommand objects
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                using (SqlCommand command = new SqlCommand("VerifyEmail", connection))
                {
                    // Specify the command type as stored procedure
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters
                    command.Parameters.Add("@token", SqlDbType.NVarChar, 255).Value = token.Trim();
                    // Add output parameter for status message
                    command.Parameters.Add("@status", SqlDbType.NVarChar, 100).Direction = ParameterDirection.Output;
                    SqlParameter userIdParameter = command.Parameters.Add("@userId", SqlDbType.Int);
                    userIdParameter.Direction = ParameterDirection.Output;

                    try
                    {
                        // Open the connection
                        await connection.OpenAsync();
                        // Execute the command asynchronously
                        await command.ExecuteNonQueryAsync();
                        // Retrieve the status message from the output parameter
                        status = command.Parameters["@status"].Value.ToString();
                        userActivity.UserId = Convert.ToInt32(command.Parameters["@userId"].Value);
                        userActivity.User_Activity = status;
                        InsertUserActivity(userActivity);
                    }
                    catch (Exception ex)
                    {
                        // Handle exception
                        status = "Error: " + ex.Message;
                    }
                }
            }

            return status;
        }

        public async Task<UserPasscode> GenerateAndInsertOTP(string email)
        {
            UserPasscode userPasscode = new UserPasscode();
            userPasscode.Passcode = new Passcode();
            userPasscode.User = new User();

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("GenerateAndInsertOTP", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Input parameter
                        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar, 50) { Value = email });
                        // Output parameters
                        var passcodeParam = new SqlParameter("@Passcode", SqlDbType.Char, 6) { Direction = ParameterDirection.Output };
                        var expiryParam = new SqlParameter("@Expiry", SqlDbType.DateTime) { Direction = ParameterDirection.Output };
                        var resultParam = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };
                        var firstname = new SqlParameter("@FirstName", SqlDbType.VarChar,50) { Direction = ParameterDirection.Output };

                        command.Parameters.Add(passcodeParam);
                        command.Parameters.Add(expiryParam);
                        command.Parameters.Add(resultParam);
                        command.Parameters.Add(firstname);

                        // Open the connection and execute the command
                        await  connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        // Retrieve output parameter values
                        userPasscode.Passcode.OTP = passcodeParam.Value != null ? passcodeParam.Value.ToString() : string.Empty;
                        userPasscode.Passcode.Expiry = expiryParam.Value != null && expiryParam.Value != DBNull.Value ? (DateTime)expiryParam.Value : DateTime.MinValue;
                        userPasscode.User.FirstName = firstname.Value != null ? firstname.Value.ToString() : string.Empty;


                        if ((int)resultParam.Value > 0)
                        {
                            userPasscode.Passcode.Response = "OTP Generated";
                            Email mailArgs = new Email();
                            mailArgs.Subject = Const.ForgotpasswordOTPEmail;
                            mailArgs.Message = Const.ForgotpasswordOTPEmail;
                            mailArgs.MailTo = userPasscode.User.Email = email;
                            _emailService.SendOtpEmial(userPasscode, mailArgs, null);
                            _emailService.InsertEmailHistory(userPasscode.User.UserId, mailArgs);
                        }
                        else
                        {
                            userPasscode.Passcode.Response = "Email does not exists";
                        }


                        userPasscode.Passcode.OTP = userPasscode.User.FirstName = string.Empty;
                        userPasscode.Passcode.Expiry = DateTime.Now;


                        return userPasscode;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (log it, rethrow it, etc.)
                Console.WriteLine("An error occurred: " + ex.Message);
                throw;
            }
        }
    
        public async Task<string> ChangePassword(string email, string newPassword)
        {
            string status = string.Empty;    userActivity = new UserActivity();

            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                try
                {
                    (string hashedPassword, byte[] salt) = SecurityHelper.HashPassword(newPassword);
                    SqlCommand command = new SqlCommand("ChangePassword", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    // Set parameters
                    command.Parameters.Add(new SqlParameter("@Email", email));
                    command.Parameters.Add(new SqlParameter("@NewPassword", hashedPassword));
                    command.Parameters.Add(new SqlParameter("@Salt", salt));
                    command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 100) { Direction = ParameterDirection.Output });
                    SqlParameter userIdParameter = command.Parameters.Add("@UserId", SqlDbType.Int);
                    userIdParameter.Direction = ParameterDirection.Output;

                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync(); // Assuming ForgotPassword doesn't return data
                    // Retrieve the output parameter value
                    status = command.Parameters["@Status"].Value.ToString();
                    userActivity.UserId = Convert.ToInt32(userIdParameter.Value);
                    userActivity.User_Activity = Const.Changed_Password;
                    InsertUserActivity(userActivity);
                }
                catch (Exception ex)
                {
                    // Handle any exceptions
                    Console.WriteLine($"An error occurred: {ex.Message}");
                    status = "Error occurred";
                }
            }

            return status;
        }
        public async Task<CompanyUser> UpsertCompanyBranchUserAsync(CompanyUser companyUser)
        {
            CompanyUser? updatedCompanyUser = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {                   

                using (var command = new SqlCommand("UpsertCompanyUser", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    // Add parameters to the stored procedure
                    command.Parameters.AddWithValue("@CompanyUserId", companyUser.CompanyUserId);
                    command.Parameters.AddWithValue("@UserId", companyUser.User.UserId);
                    command.Parameters.AddWithValue("@CompanyId", companyUser.CompanyId);
                    command.Parameters.AddWithValue("@IsPrimaryContact", companyUser.IsPrimaryContact);
                    command.Parameters.AddWithValue("@BranchId", companyUser.BranchId ?? 0);
                    command.Parameters.AddWithValue("@IsParent", companyUser.IsParent ?? 0);
                    command.Parameters.Add("@CompanyUserIdOutput", SqlDbType.Int).Direction = ParameterDirection.Output;

                        await connection.OpenAsync();
                        // Add an output parameter for @CompanyUserId
                        // Execute the command
                        await command.ExecuteNonQueryAsync();

                    // Update the CompanyUserId property with the output parameter value
                    companyUser.CompanyUserId = Convert.ToInt32(command.Parameters["@CompanyUserIdOutput"].Value);
                    return companyUser;
                }
            }
            }
            catch (Exception ex) { throw new Exception("Error in UpsertCompanyUserAsync", ex); }


            return updatedCompanyUser;
        }
        public async Task<CompanyUser> UpsertCompanyUserAsync(CompanyUser companyUser, SqlTransaction transaction)
        {
            CompanyUser? updatedCompanyUser = null;
            try
            {

                using (var command = new SqlCommand("UpsertCompanyUser", transaction.Connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Add parameters to the stored procedure
                    command.Parameters.AddWithValue("@CompanyUserId", companyUser.CompanyUserId);
                    command.Parameters.AddWithValue("@UserId", companyUser.User.UserId);
                    command.Parameters.AddWithValue("@CompanyId", companyUser.CompanyId);
                    command.Parameters.AddWithValue("@IsPrimaryContact", companyUser.IsPrimaryContact);
                    command.Parameters.AddWithValue("@BranchId", companyUser.BranchId ??0);
                    command.Parameters.AddWithValue("@IsParent", companyUser.IsParent??0);
                    command.Parameters.Add("@CompanyUserIdOutput", SqlDbType.Int).Direction = ParameterDirection.Output;


                    // Add an output parameter for @CompanyUserId

                    // Execute the command
                    await command.ExecuteNonQueryAsync();

                    // Update the CompanyUserId property with the output parameter value
                    companyUser.CompanyUserId = Convert.ToInt32(command.Parameters["@CompanyUserIdOutput"].Value);

                    return companyUser;
                }
            }
            catch (Exception ex) { throw new Exception("Error in UpsertCompanyUserAsync", ex); }
            

            return updatedCompanyUser;
        }

        public async Task<bool> ValidateOTP(string email, string passcode)
        {
            bool isValid = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    using (SqlCommand command = new SqlCommand("ValidateOTP", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Input parameters
                        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.VarChar, 50) { Value = email });
                        command.Parameters.Add(new SqlParameter("@EnteredPasscode", SqlDbType.VarChar, 8) { Value = passcode });

                        // Output parameter
                        var isValidParam = new SqlParameter("@IsValid", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                        command.Parameters.Add(isValidParam);

                        // Open the connection and execute the command
                       await connection.OpenAsync();
                      await  command.ExecuteNonQueryAsync();

                        // Retrieve output parameter value
                        isValid = Convert.ToBoolean(isValidParam.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle the exception
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return isValid;
        }

        public async void InsertUserActivity(UserActivity userActivity)
        {
            userActivity.IpAddress =  await  NetworkHelper.GetPublicIpAddressAsync();
            userActivity.MacAddress = await NetworkHelper.GetLocalMacAddress();
            using (SqlConnection conn = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("InsertUserActivity", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    userActivity.CreatedOn = DateTime.Now;
                    cmd.Parameters.AddWithValue("@UserId", userActivity.UserId);
                    cmd.Parameters.AddWithValue("@UserActivity", (object)userActivity.User_Activity ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Description", (object)userActivity.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Createdon", userActivity.CreatedOn);
                    cmd.Parameters.AddWithValue("@ip_address", (object)userActivity.IpAddress ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@mac_address", userActivity.MacAddress);

                    await conn.OpenAsync();
                    await cmd.ExecuteScalarAsync();
                    await conn.CloseAsync();

                    //return Convert.ToInt32(result);
                }
            }
        }

        public async Task<UserActivity> GetUserActivityById(UserActivity userActivityId)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                SqlCommand command = new SqlCommand("GetUserActivities", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@UserActivityId", userActivityId);

                try
                {
                   await connection.OpenAsync();
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    if (reader.Read())
                    {
                        userActivity = new UserActivity
                        {
                            UserActivityId = reader.GetInt32(0),
                            UserId = reader.GetInt32(1),
                            User_Activity = reader.IsDBNull(2) ? null : reader.GetString(2),
                            Description = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CreatedOn = reader.GetDateTime(4),
                            IpAddress = reader.IsDBNull(5) ? null : reader.GetString(5),
                            MacAddress = reader.GetString(6) // Assuming mac_address column is NOT NULL in the database
                        };
                    }

                    await connection.CloseAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error retrieving user activity: " + ex.Message);
                }
            }

            return userActivity;
        }
        //public static async Task<int> GetUserIdFromTokenAsync(string token)
        //{
        //    return await Task.Run(() =>
        //    {
        //        var handler = new JwtSecurityTokenHandler();
        //        JwtSecurityToken jwtToken = null;

        //        try
        //        {
        //            // Read the JWT token
        //            jwtToken = handler.ReadJwtToken(token);
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle token reading errors
        //            throw new InvalidOperationException("Invalid JWT token.", ex);
        //        }

        //        // Extract the user ID from the claims
        //        var userIdClaim = jwtToken?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

        //        if (userIdClaim == null)
        //        {
        //            throw new InvalidOperationException("User ID claim not found in JWT token.");
        //        }

        //        if (!int.TryParse(userIdClaim.Value, out int userId))
        //        {
        //            throw new InvalidOperationException("Invalid user ID format in JWT token.");
        //        }

        //        return userId;
        //    });
        //}
        public async Task<string> GetStoredRefreshTokenAsync(int userId)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            using (SqlCommand command = new SqlCommand("GetStoredRefreshToken", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserId", userId));

                SqlParameter refreshTokenParam = new SqlParameter("@RefreshToken", SqlDbType.NVarChar, 500) // Adjust size to 500
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(refreshTokenParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                var refreshToken = refreshTokenParam.Value as string;

                return refreshToken;
            }
        }



        public async Task StoreRefreshTokenAsync(int userId, string refreshToken)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            using (SqlCommand command = new SqlCommand("StoreRefreshToken", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@UserId", userId));
                command.Parameters.Add(new SqlParameter("@RefreshToken", refreshToken));
                command.Parameters.Add(new SqlParameter("@ExpiryDate", DateTime.UtcNow.AddDays(30))); // Set refresh token expiry

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> GetUserIdFromRefreshTokenAsync(string refreshToken)
        {
           
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            using (SqlCommand command = new SqlCommand("GetUserIdFromRefreshToken", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@RefreshToken", refreshToken));

                SqlParameter userIdParam = new SqlParameter("@UserId", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(userIdParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                if (userIdParam.Value != DBNull.Value)
                {
                    return (int)userIdParam.Value;
                }

                return 0; // Return 0 if the token is invalid or expired
            }
        }


    }
}


    


