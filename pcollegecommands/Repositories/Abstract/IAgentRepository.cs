using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Concrete;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IAgentRepository
    {
       //  Task<Company> ExecuteUpsertCompanyDetailsAsync(Company company);

        List<Responses> BulkUpsertResponses(List<Responses> responses);
    }
}
