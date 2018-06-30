using System;
using LibGit2Sharp;

namespace Bulwark.Integration.Repository
{
    public interface IRepositorySession : IDisposable
    {
        IRepository Repository { get; }
        
        string Location { get; }
    }
}