﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Bulwark.Strategy.CodeOwners
{
    public interface ICodeOwnersBuilder
    {
        Task<List<string>> GetOwners(IFileProvider provider, string path);
    }
}