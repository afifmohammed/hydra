using Hydra.Core;
using Hydra.Requests;

namespace Hydra.Configuration
{
    public sealed class ConfiguredSubscribers : IRequest<Subscriber>
    { }

    public sealed class RegisteredSubscriptions : IRequest<Subscription>
    { }
}