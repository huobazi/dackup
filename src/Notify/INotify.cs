using System.Threading.Tasks;

namespace Dackup.Notify
{
    public interface INotify
    {        
        Task<NotifyResult> NotifyAsync(Statistics statistics);
    }

    public class NotifyResult
    {
    }
}