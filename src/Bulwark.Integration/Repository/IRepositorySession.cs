using System;
using LibGit2Sharp;

namespace Bulwark.Integration.GitLab
{
    public interface IRepositorySession : IDisposable
    {
        IRepository Repository { get; }
    }
}