using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using AdoNet;
using EventSourcing;
using RequestPipeline;
using Requests;

namespace WebApi
{
    /// <summary>
    /// This class is used as place holder to specify that the name of the connection string is <see cref="EventStoreTransportConnectionString"/>
    /// </summary>
    class EventStoreTransportConnectionString { }
}
