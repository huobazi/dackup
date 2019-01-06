using System;
using System.Threading.Tasks;

namespace dackup
{
    public interface IStorage
    {        
        Task<UploadResult> UploadAsync(string fileName);
        Task<PurgeResult> PurgeAsync();
    }

    public class UploadResult{
        
    }
    public class PurgeResult{
        
    }
}