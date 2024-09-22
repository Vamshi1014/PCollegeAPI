using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Models.Databasemodel
{
    public static class ConfigurationData
    {
        public static string? DbConnectionString { get; set; }
        public static string? BlobConnectionString { get; set; }
        public static string? BlobRootURI{ get; set; }
        
        public static string? BlobContainerName { get; set; }
    }
}
