using System.Threading.Tasks;

namespace Dackup.Storage
{
    public interface IStorage
    {
        Task<UploadResult> UploadAsync(string fileName);
        Task<PurgeResult> PurgeAsync();
    }
    public class UploadResult
    {
    }
    public class PurgeResult
    {
    }
}