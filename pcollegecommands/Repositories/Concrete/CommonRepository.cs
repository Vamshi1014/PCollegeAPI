using System.Data;
using System.Linq.Expressions;
using Flyurdreamcommands.Models.Databasemodel;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Abstract;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class CommonRepository :DataRepositoryBase, ICommonRepository
    {
            private readonly IConfiguration _config;
            private readonly ILogger _logger;
            public CommonRepository(IConfiguration config, ILogger<CommonRepository> logger) : base(logger, config)
            {
                _config = config;
                _logger = logger;
            }
            public List<Country> GetCountries(string? searchKeyword)
        {
            List<Country> countries = new List<Country>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SearchCountry", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        if (string.IsNullOrEmpty(searchKeyword))
                        {
                            command.Parameters.Add(new SqlParameter("@SearchKeyword", DBNull.Value));
                        }
                        else
                        {
                            command.Parameters.Add(new SqlParameter("@SearchKeyword", searchKeyword));
                        }
                        command.Parameters.Add(new SqlParameter("@ReturnMessage", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Country country = new Country();
                                country.CountryID = reader.GetInt32(reader.GetOrdinal("Country_ID"));
                                country.CountryName = reader.IsDBNull(reader.GetOrdinal("Country_Name")) ? null : reader.GetString(reader.GetOrdinal("Country_Name"));
                                country.CountryCode = reader.IsDBNull(reader.GetOrdinal("Country_Code")) ? null : reader.GetString(reader.GetOrdinal("Country_Code"));
                                country.Dial = reader.IsDBNull(reader.GetOrdinal("dial")) ? 0 : reader.GetInt32(reader.GetOrdinal("dial"));
                                country.Currency_Name = reader.IsDBNull(reader.GetOrdinal("currency_name")) ? null : reader.GetString(reader.GetOrdinal("currency_name"));
                                country.Currency = reader.IsDBNull(reader.GetOrdinal("currency")) ? null : reader.GetString(reader.GetOrdinal("currency"));

                                countries.Add(country);
                            }
                        }
                    }
                
                    connection.Close();
                }
            }
            catch (Exception ex) { _logger.LogError(ex.Message); throw ; }
         
            return countries;
        }

             public List<State> GetStates(int CountryId, string? SearchKeyword)
        {
            List<State> states = new List<State>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SearchStateByCountry", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@SearchKeyword", (object)SearchKeyword?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@CountryID", CountryId));
                    command.Parameters.Add(new SqlParameter("@ReturnMessage", SqlDbType.VarChar,100) { Direction = ParameterDirection.Output });


                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            State state = new State();
                            state.CountryID = Convert.ToInt32(reader["Country_ID"]);
                            state.StateName = reader["State_Name"].ToString();
                            state.StateID = Convert.ToInt32( reader["State_ID"]);
                            states.Add(state);
                        }
                    }
                }
                    connection.Close();
                }
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                    throw; }
            return states;
        }

             public List<City> GetCity(int StateId, string? SearchKeyword)
        {
            List<City> cities = new List<City>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SearchCityByState", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@SearchKeyword", (object)SearchKeyword ?? DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@StateID", StateId));
                        command.Parameters.Add(new SqlParameter("@ReturnMessage", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output });

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                City city = new City();
                                city.StateID = Convert.ToInt32(reader["State_ID"]);
                                city.CityID = Convert.ToInt32(reader["City_ID"]);
                                city.CityName = reader["City_Name"].ToString();
                                cities.Add(city);
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ;
            }

            return cities;
        }

        public async Task <List<Types>> GetTypeRecords(string? description =null, string? typeFor =null)
        {
            var types = new List<Types>();

            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("GetTypeRecords", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters with DBNull.Value if they are null
                        command.Parameters.AddWithValue("@description", (object?)description ?? DBNull.Value);
                        command.Parameters.AddWithValue("@typefor", (object?)typeFor ?? DBNull.Value);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                var type = new Types
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                    FieldType = reader.IsDBNull(reader.GetOrdinal("TypeName")) ? null : reader.GetString(reader.GetOrdinal("TypeName")),
                                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                                    IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")),
                                    TypeFor = reader.IsDBNull(reader.GetOrdinal("TypeFor")) ? null : reader.GetString(reader.GetOrdinal("TypeFor"))
                                };

                                types.Add(type);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }

            return types;
        }

    }

}
