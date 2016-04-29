using System.Runtime.Serialization;

namespace ValuationService.Domain
{
    [DataContract(Name = "vAuthHeader")]
    public class AuthHeader
    {
        [DataMember(Name = "userName")]
        public string UserName { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}