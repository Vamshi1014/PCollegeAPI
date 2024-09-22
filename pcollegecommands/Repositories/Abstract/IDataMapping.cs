using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;
using Flyurdreamcommands.Repositories.Concrete;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IDataMapping
    {
         Task<string> CompanyQuestionMapping(int companyId, int formDataId);
    }
}
