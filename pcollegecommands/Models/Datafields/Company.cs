namespace Flyurdreamcommands.Models.Datafields
{
    public class Company
    {
        public int? CompanyID { get; set; }

        public string? CompanyName { get; set; }

        public string? TradeName { get; set; }

        public string? BusinessRegistrationNumber { get; set; }

        public string? CompanyWebAddress { get; set; }

        public bool IsActive { get; set; }

        public string? Result { get; set; }

        public string? CompanyLogo { get; set; }
        public string? PortalWebAddress { get; set; }

        public string? PortalWebAddress2 { get; set; }
        public string? PortalocalAddress { get; set; }

        public List<CompanyAddress>? Address { get; set; }


    }
}
