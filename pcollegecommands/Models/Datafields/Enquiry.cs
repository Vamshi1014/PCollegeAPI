namespace Flyurdreamcommands.Models.Datafields
{

    public class Enquiry
    {
        public int? Enquiry_Id { get; set; }
        public string? Unique_Id { get; set; }
        public string? FirstName { get; set; }
        public string? _Enquiry { get; set; }
        public string? LastName { get; set; }
        public string? Mobile_Number { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public Country? Country { get; set; }
        public State? State { get; set; }
        public City? City { get; set; }
        public string? Postal_code { get; set; }
        public string? Passport_Number { get; set; }
        public bool Married { get; set; }
        public string? Nationality { get; set; }
        public DateTime Date_Of_Birth { get; set; }
        public string? Current_Education { get; set; }
        public int? Country_Interested { get; set; }
        public bool? Previous_Visa_Refusal { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? response { get; set; }
        public int? CreatedBy { get; set; }
    }
}
