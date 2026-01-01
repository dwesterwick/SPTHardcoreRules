using System.Runtime.Serialization;

namespace DansDevTools.Configuration
{
    [DataContract]
    public class ModConfig
    {
        [DataMember(Name = "enabled")]
        public bool Enabled { get; set; } = false;
    }
}
