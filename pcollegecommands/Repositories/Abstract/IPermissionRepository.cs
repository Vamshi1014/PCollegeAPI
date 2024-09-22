using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IPermissionRepository
    {
        Task<bool> UserHasPermissionAsync(int userId, string methodName);
    }
}
