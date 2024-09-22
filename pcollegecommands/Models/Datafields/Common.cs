namespace Flyurdreamcommands.Models.Datafields
{
    public class Country
    {

        public int? CountryID { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public int? Dial { get; set; }
        public string? Currency_Name { get; set; }
        public string? Currency { get; set; }
        public bool? IsActive { get; set; }
    }
    public class State
    {

        public int? StateID { get; set; }
        public string? StateName { get; set; }
        public int? CountryID { get; set; }
    }
    public class City
    {

        public int? CityID { get; set; }
        public string? CityName { get; set; }
        public int? StateID { get; set; }
    }
    public class Types
    {
        public int? Id { get; set; }
        public string? FieldType { get; set; }
        public string? Description { get; set; }
        public int? IsActive { get; set; }
        public string? TypeFor { get; set; }
    }
}
