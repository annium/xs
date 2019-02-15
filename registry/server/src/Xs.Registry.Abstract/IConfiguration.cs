using System;

namespace Xs.Registry.Abstract
{
    public interface IConfiguration
    {
        Uri Location { get; }

        Uri MainLocation { get; }
    }
}