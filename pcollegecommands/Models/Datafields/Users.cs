namespace Flyurdreamcommands.Models.Datafields
{
    public class User
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? lastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool? IsActive { get; set; }
        public string? Token { get; set; }
        public bool? UserVerified { get; set; }
        public string? VerificationUrl { get; set; }
        public string? Salutation { get; set; }
        public string? Response { get; set; }
        public string? RefreshToken { get; set; }
        public string? logintoken { get; set; }
        public int GroupId { get; set; }
        public string? Mobile { get; set; }
        public int CountryCode { get; set; }
        public int Createdby { get; set; }
        public DateTime LastLogin { get; set; }
        public List<UserAddress>? Address{ get; set; }
        public List<CompanyBranches>? ListCompanyBranches { get; set; }
    }


    public class UserAddress
    {
        public int UserAddressId { get; set; }
        public int? UserId { get; set; }
        public Address? Addresses { get; set; }
        public bool IsUpdate { get; set; }
    }
    public class Passcode
    {
        public string? Response { get; set; }
        public string? OTP { get; set; }
        public DateTime? Createdon { get; set; }
        public DateTime? Expiry { get; set; }
    }

    public class UserPasscode
    {
        public Passcode? Passcode { get; set; }
        public User? User { get; set; }
    }
}
