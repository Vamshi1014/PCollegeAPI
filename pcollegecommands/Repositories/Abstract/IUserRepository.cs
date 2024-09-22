using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IUserRepository
    {
        Task<string> GetDatabsucces();
        Task<User> GetLoggedInUser(User appuser);
        Task<User> UserRegistration(User appUser, HttpRequest request);
        Task<string> VerifyEmail(string token);
        Task<string> ChangePassword(string email, string newPassword);
        Task<CompanyUser> UpsertCompanyUserAsync(CompanyUser companyUser, SqlTransaction transaction);
        Task<UserPasscode> GenerateAndInsertOTP(string email);
        Task<bool> ValidateOTP(string email, string passcode);
        Task<User> UpdateUser(User user, SqlTransaction transaction);
        Task<User> GetUserDetailsAsync(int? userId);
        void InsertUserActivity(UserActivity userActivity);
        Task<UserActivity> GetUserActivityById(UserActivity userActivityId);
        Task<User> UpdateUserAddress(User user);
        Task StoreRefreshTokenAsync(int userId, string refreshToken);
        Task<string> GetStoredRefreshTokenAsync(int userId);
         Task<int> GetUserIdFromRefreshTokenAsync(string refreshToken);

    }
}
