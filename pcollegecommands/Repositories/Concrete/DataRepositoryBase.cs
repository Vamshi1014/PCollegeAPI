using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flyurdreamcommands.Service.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flyurdreamcommands.Repositories.Concrete
{
    public class DataRepositoryBase
    {
        protected readonly IConfiguration _config;
        protected readonly ILogger _logger;

        public DataRepositoryBase(ILogger logger, IConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));

        }
    }
}
