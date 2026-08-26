using Microsoft.Extensions.Configuration;

namespace AccountingInterfaceToken
{
    public class AccountingInterfaceTokenManagerSettings
    {
        public const string Section = "JwtTokenManager";

        public AccountingInterfaceTokenManagerSettings(IConfiguration config)
        {
            ServiceBaseUri = config[$"{Section}:ServiceBaseUri"];
            Issuer = config[$"{Section}:Issuer"];
            ApiKey = config[$"{Section}:ApiKey"];
            PrivateKey = config[$"{Section}:PrivateKey"];
            PrivateKeyPath = config[$"{Section}:PrivateKeyPath"];
            HeaderKey = config[$"{Section}:HeaderKey"];
            TtlMinutes = int.Parse(config[$"{Section}:TtlMinutes"] ?? "0");
        }

        public string ServiceBaseUri { get; }
        public string Issuer { get; }
        public string ApiKey { get; }
        public string PrivateKey { get;  }
        public string PrivateKeyPath { get; }
        public string HeaderKey { get;  }
        public int TtlMinutes { get; }
    }
}
