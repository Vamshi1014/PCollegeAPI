using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Datafields
{
    public class UserActivity
    {
        public int UserActivityId { get; set; }
        public int UserId { get; set; }
        public string? User_Activity { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? IpAddress { get; set; }
        public string? MacAddress { get; set; }
    }
}
