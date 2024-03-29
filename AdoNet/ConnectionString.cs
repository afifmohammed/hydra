﻿using System;
using System.Configuration;

namespace Hydra.AdoNet
{ 
    public static class ConnectionString
    {
        public static Func<string, string> ByName = connectionStringName => ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
    }
}