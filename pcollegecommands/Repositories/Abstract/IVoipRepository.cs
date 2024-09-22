using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IVoipRepository
    {
        Task<JsonObject> GetRequestYay();
        Task<string> InitiateCallAsync();
    }
}
