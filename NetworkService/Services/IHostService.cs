using System.Collections.Generic;
using NetworkService.Models;

namespace NetworkService.Services
{
    public interface IHostService
    {
        IEnumerable<Host> GetAll();
    }
}