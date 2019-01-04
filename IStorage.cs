using System;
using System.Threading.Tasks;

namespace dackup
{
    public interface IStorage
    {        
        Task Upload(string fileName);
        Task Purge();
    }
}