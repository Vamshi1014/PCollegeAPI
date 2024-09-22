using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IQuestionsRepository
    {
        Task<List<Questions>> GetQuestionsFromDatabase(int companyId, int formId);
    }
}
