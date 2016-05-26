using Hydra.Core;
using Hydra.Requests;

namespace Hydra.Configuration
{
    public sealed class ConfiguredSubscriber : IRequest<Subscriber>
    { }

    public sealed class AvailableSubscriptions : IRequest<Subscription>
    { }
}