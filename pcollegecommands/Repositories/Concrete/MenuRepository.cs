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
using Microsoft.IdentityModel.Tokens;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class MenuRepository : DataRepositoryBase, IMenuRepository
    {
        protected readonly IConfiguration _config;
        protected readonly ILogger _logger;
        protected readonly IEmailService _emailService;

        public MenuRepository(ILogger<MenuRepository> logger, IConfiguration config, IEmailService emailService) : base(logger, config)
        {
            _config = config;
            _logger = logger;
            _emailService = emailService;
        }
        public List<MenuItem> GetMenuItems()
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            
            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                SqlCommand command = new SqlCommand("GetAllMenuItems", connection);
                command.CommandType = CommandType.StoredProcedure;

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        MenuItem menuItem = new MenuItem();
                        menuItem.MenuItemID = (int)reader["MenuItemID"];
                        menuItem.MenuItemName = (string)reader["MenuItemName"];
                        menuItem.ParentMenuItemID = reader["ParentMenuItemID"] != DBNull.Value ? (int)reader["ParentMenuItemID"] : 0;
                        menuItem.IsActive = (bool)reader["IsActive"];
                        menuItem.MenuPath = (string)reader["MenuPath"];
                        menuItem.MenuURL = Convert.IsDBNull(reader["MenuURL"]) ? null : (string)reader["MenuURL"];
                        menuItem.Level = (int)reader["Level"];
                        

                        menuItems.Add(menuItem);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return menuItems;
        }

        public async Task<List<MenuItem>> GetMenuBasedonGroup(int group)
        {
            List<MenuItem> menuItems = new List<MenuItem>();

            using (SqlConnection connection = new SqlConnection(ConfigurationData.DbConnectionString))
            {
                SqlCommand command = new SqlCommand("GetMenuItemsBasedOnGroup", connection);
                command.CommandType = CommandType.StoredProcedure;
                if (group!=0)
                {
                    command.Parameters.Add(new SqlParameter("@Role", group));
                }

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        MenuItem menuItem = new MenuItem();
                        menuItem.MenuItemID = (int)reader["MenuItemID"];
                        menuItem.MenuItemName = (string)reader["MenuItemName"];
                        menuItem.ParentMenuItemID = reader["ParentMenuItemID"] != DBNull.Value ? (int)reader["ParentMenuItemID"] : 0;
                        menuItem.IsActive = (bool)reader["IsActive"];
                        menuItem.MenuPath = (string)reader["MenuPath"];
                        menuItem.MenuURL = Convert.IsDBNull(reader["MenuURL"]) ? null : (string)reader["MenuURL"];
                        menuItem.Level = (int)reader["Level"];


                        menuItems.Add(menuItem);
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return menuItems;
        }
    }
}

