using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Models.Datafields;

namespace Flyurdreamcommands.Repositories.Abstract
{
    public interface ICommonRepository
    {
        List<Country> GetCountries(string? searchKeyword);
        List<State> GetStates(int CountryId, string? SearchKeyword);
        List<City> GetCity(int StateId, string? SearchKeyword);
        Task<List<Types>> GetTypeRecords(string? description = null, string? typeFor = null);
    }
}
