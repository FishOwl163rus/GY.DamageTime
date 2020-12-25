using Rocket.API;

namespace GY.DamageTime
{
    public class Config : IRocketPluginConfiguration
    {
        public uint SecToDamage;
        public string BypassPermission;
        public void LoadDefaults()
        {
            BypassPermission = "gy.bypass";
            SecToDamage = 86400;
        }
    }
}