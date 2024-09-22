using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface IMenuRepository
    {
        List<MenuItem> GetMenuItems();
        Task<List<MenuItem>> GetMenuBasedonGroup(int group);
    }
}
