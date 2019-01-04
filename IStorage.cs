using System;
using System.Threading.Tasks;
public interface IStorage
{
     Task Upload(string fileName);

     Task Purge(DateTime removeThreshold);
}