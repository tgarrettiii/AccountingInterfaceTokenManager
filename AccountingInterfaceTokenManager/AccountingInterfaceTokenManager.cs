using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AccountingInterfaceToken
{
    public class AccountingInterfaceTokenManager : IAccountingInterfaceTokenManager
    {
        private const double ExpireBuffer = 1;

        private int Ttl { get; set; }

        private string ServiceBaseUri { get; set; }

        private string Issuer { get; set; }

        private string ApiKey { get; set; }

        private string PrivateKey { get; set; }

        private JwtPayload Payload { get; set; }

        private string AccessToken { get; set; }

        private string HeaderKey { get; set; }

        public AccountingInterfaceTokenManager(IConfiguration config)
        {
            var settings = new AccountingInterfaceTokenManagerSettings(config);
                        
            if(string.IsNullOrEmpty(settings.PrivateKey) && string.IsNullOrEmpty(settings.PrivateKeyPath))
            {
                throw new ArgumentException("Please provide one or the other a PrivateKey or a PrivateKeyPath in the configuration for the AccountingInterfaceTokenManagerManager.");
            }

            var key = settings.PrivateKey;
            if (string.IsNullOrEmpty(key))
            {
                if(string.IsNullOrEmpty(settings.PrivateKeyPath) || !File.Exists(settings.PrivateKeyPath))
                {
                    throw new ArgumentException("Please provide either a PrivateKey or a PrivateKeyPath in the configuration for the AccountingInterfaceTokenManagerManager.");
                }
                using (var reader = new StreamReader(new FileStream(settings.PrivateKeyPath, FileMode.Open, FileAccess.Read), Encoding.UTF8))
                {
                    key = reader.ReadToEnd();
                }
            }

            Initialize(
                key,
                settings.ServiceBaseUri,
                settings.Issuer,
                settings.ApiKey,
                settings.HeaderKey,
                settings.TtlMinutes);
        }

        public AccountingInterfaceTokenManager(string privateKey, string serviceBaseUri, string issuer, string apiKey, string headerKey = "x-jwt", int ttlMinutes = 0)
        {
            Initialize(privateKey, serviceBaseUri, issuer, apiKey, headerKey, ttlMinutes);
        }

        public AccountingInterfaceTokenManager(Stream privateKeyFile, string serviceBaseUri, string issuer, string apiKey, string headerKey = "x-jwt", int ttlMinutes = 0)
        {
            if (privateKeyFile == null)
            {
                throw new ArgumentException("Please provide a stream from a private key file.");
            }

            string privateKey = string.Empty;
            using (var reader = new StreamReader(privateKeyFile, Encoding.UTF8))
            {
                privateKey = reader.ReadToEnd();
            }

            if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentException("Please provide a stream from a private key file.");
            }

            Initialize(privateKey, serviceBaseUri, issuer, apiKey, headerKey, ttlMinutes);
        }

        private void Initialize(string privateKey, string serviceBaseUri, string issuer, string apiKey, string headerKey = "x-jwt", int ttl = 2)
        {
            ServiceBaseUri = serviceBaseUri;
            Issuer = issuer;
            ApiKey = apiKey;
            PrivateKey = privateKey;

            if (string.IsNullOrEmpty(ServiceBaseUri))
            {
                throw new ArgumentException("Please provide a service base uri.");
            }

            if (string.IsNullOrEmpty(Issuer))
            {
                throw new ArgumentException("Please provide the issuer.");
            }

            if (string.IsNullOrEmpty(ApiKey))
            {
                throw new ArgumentException("Please provide your client key.");
            }

            if (string.IsNullOrEmpty(PrivateKey))
            {
                throw new ArgumentException("Please provide the file location of the private pem key.");
            }

            if (ttl <= 0)
            {
                ttl = 2;
            }

            Ttl = ttl;

            if (string.IsNullOrEmpty(headerKey))
            {
                HeaderKey = "x-jwt";
                return;
            }

            HeaderKey = headerKey;
        }

        public async Task<string> GetTokenAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                if (Payload != null)
                {
                    if (UnixTimeStampToDateTime((double) Payload["exp"]) > DateTime.Now.AddMinutes(ExpireBuffer))
                    {
                        return AccessToken;
                    }

                    Reset();
                }

                //Some PayLoad that contain information about the  customer
                Payload = new JwtPayload
                    {
                        {"aud", ServiceBaseUri},
                        {"iss", Issuer}, //"EP-DEV"
                        {"exp", ConvertToUnixTimestamp(DateTime.UtcNow.AddMinutes(Ttl))},
                        {"iat", ConvertToUnixTimestamp(DateTime.UtcNow)},
                        {"jti", Guid.NewGuid()}, //"dccb8f9e-6c8d-4852-a88b-ecbec1441fee"
                        {"client_id", ApiKey} //"2kZQsep5tPnHGCcjQIWaYsZPgJjqldnQ"
                    };

                AccessToken = JwtKey.GenerateToken(PrivateKey, Payload);

                return AccessToken;
            });
        }

        public void Reset()
        {
            Payload = null;
            AccessToken = null;
        }

        public async Task<KeyValuePair<string, string>> GetHeaderTokenAsync()
        {
            string token = await GetTokenAsync();

            return new KeyValuePair<string, string>(HeaderKey, token);
        }

        private static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
