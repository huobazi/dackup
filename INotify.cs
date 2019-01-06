using System;
using System.Threading.Tasks;

namespace dackup
{
    public interface INotify
    {        
        Task<NotifyResult> NotifyAsync();
    }

    public class NotifyResult{

    }
}