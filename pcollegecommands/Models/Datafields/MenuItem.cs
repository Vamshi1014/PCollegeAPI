namespace Flyurdreamcommands.Models.Datafields
{
    public class MenuItem
    {
        public int MenuItemID { get; set; }
        public string? MenuItemName { get; set; }
        public int ParentMenuItemID { get; set; }
        public bool IsActive { get; set; }
        public string? MenuPath { get; set; }
        public int Level { get; set; }
        public string? MenuURL { get; set; }
    }
}
