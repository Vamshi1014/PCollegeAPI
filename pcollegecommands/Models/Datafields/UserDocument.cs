namespace Flyurdreamcommands.Models.Datafields
{
    public class UserDocument
    {
        public int UserDocumentID { get; set; }
        public User? User { get; set; }
        public Document? Document { get; set; }
        public int CompanyId { get; set; }
    }

}
