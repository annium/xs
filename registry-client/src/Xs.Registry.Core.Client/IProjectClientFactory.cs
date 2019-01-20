using System;
using Xs.Core.Models;

namespace Xs.Registry.Core.Client
{
    public interface IProjectClientFactory
    {
        ProjectType ProjectType { get; }

        IProjectClient Create(Uri uri);
    }
}