using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using AccountingInterfaceToken;

namespace TestToken
{
    class Program
    {
        static void Main(string[] args)
        {
            string serviceUri = "https://expenses.api.ep.com/v1/accountinginterfaces"; //URI done per environment
            string issuer = "EP-LEDGER"; //Provided by EP
            string apiKey = "oA9zPRwdd6b35aBGSWAV7ZYWG9iV6Ay3"; //Provided by EP
            string PrivateKey = "-----BEGIN RSA PRIVATE KEY-----\r\n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\r\n-----END RSA PRIVATE KEY-----\r\n";
            string PathPrivateKey = "D:\\GIT-REPOS\\SERVICES\\VendorManagementService\\VendorManagementService\\VendorManagementService.Web\\private.txt";
            //int ttlMinutes = 2;

            IAccountingInterfaceTokenManager tokenManager;
            KeyValuePair<string, string> headerToken;
            IConfiguration config;

            //using file
            tokenManager = new AccountingInterfaceTokenManager(PrivateKey, serviceUri, issuer, apiKey);

            //tokenManager = new AccountingInterfaceTokenManagerManager(new FileStream(PathPrivateKey, FileMode.Open, FileAccess.Read), serviceUri, issuer, apiKey);
            headerToken = tokenManager.GetHeaderTokenAsync().Result;

            //using key as string
            string privateKey = File.ReadAllText(PathPrivateKey);
            tokenManager = new AccountingInterfaceTokenManager(privateKey, serviceUri, issuer, apiKey);
            headerToken = tokenManager.GetHeaderTokenAsync().Result;


            //using config with key as string 

            // appsettings.json file
            //  "AccountingInterfaceTokenManagerManager": {
            //  "ServiceBaseUri": "ServiceBaseUri",
            //  "Issuer": "Issuer",
            //  "ApiKey": "ApiKey",
            //  "PrivateKey": "-----BEGIN RSA PRIVATE KEY-----\r\nMIIEogIBAAKCAQEAnMQCozTtkeSsHW9UDf8ymjYBJnJkiIiZy5RjGoHYSscMrPsx\r\n61y2+sME3GOi7lfiUP5vDzfQUk0HvAmg5NlvxjeRHIQ79M7CSKfMzdsZG5dHf0oS\r\nSE3VuDdpDwAwkBd3BBd7l9PH6aRbsoFR8iCzpcZApHNvUoPVepf+Dq59EXgL7QSw\r\nnjBqxnWhbduc51tvJmeMSFFk4Gv8UKuu2YPXHimUbBlsqP/yuN5NNl/7TeUgDu5b\r\netthN47CmwX78dTxT3exgNLcVNlx2NMBoDT3xlJM+XQJk2yRhTZrUB2E5rJZ2zny\r\nzXfdC2Na74xkWaIaM1Aq6Rd9GYPnSZ5Ix8ln0wIDAQABAoIBABH8t49sbO820rzU\r\nikt02E8zb8cD4cnSCqYo54TMwzmJRJ4QeyDV/t907aOFVyuL2pmP0sRnPg2YwxDD\r\nS2UWwdQiNLyV5knnngk3dQYQY/1zdS5YxxI+xA1NZk45/QMmKMTHwP73ENLGcLMM\r\nUuUPoC1JkHSOE4bfPj5ryX3RGDlJuTHQRjzHoXrP+AxgsPIBrejls26KZq7cdNo5\r\nX+IPx6Be59nQ8MpV5wV557i6dPxn1g+4PaEilAOrMTnaVEkW0RaanlUpK4fD86W3\r\nA7mqeEdjStZmC4EckHKwp9GjLjeP9b5fd5GOkhPLsNOoOTsKgVkrKJo1iH9byuLN\r\nmCSCBhECgYEA1VMN3wlp5FThquXdeezmkqRnsYDUT79wpQ2uLXxJ/iPB7iIseATj\r\nshd+HIxWDEPYXdrCclJ4RdL+dbCWZ28QJYThaq6umMXAMGWJMOxYMscJVXanjJD2\r\nZFYErx7b3ErY3GwcPztzWoHNyAqWBdfI97ls05WZfHJgocdClOSEVAkCgYEAvCBs\r\nFl76oF7CUCVmDQHsx1uwv2k0HlWhwgvMDvBGFHtnBJPddM67jh/dAA/QKMtEbW0v\r\nXxLXbjPIaJ1gEArfmBNQq4fc8708EZWa2/KwkiPloUPgxUHTnqfpNx2UO1YKt/eX\r\nbBxvn/7rWuFLldTG9AdHSnqgfOIjOqm1SSrhq/sCgYA1jTf6OZ6/lx9fi3zh0rq/\r\nLU5qnPCvZFue06RZ/s2EYu2YHjQTnQab+pHKEOC5C38RcI1HrbRLsv/2A3J+XL8s\r\n7AK6iUeDSoFIpPSft3UcqiKJtdOx0eJIpd9fJtwCnTd7fhhJN19p5RQHl8C5jsYA\r\niifrpa52MzBW7HKmdgdSIQKBgDSr6zdUPl2aSlNANVNc/5tNi6372ZDIrvJydR6V\r\nnTls14oLkEx8+TwwpaK3yrMxPiZZFJNM5i4/p1PLBDNZqAKfHc34yx8fE2tk9Key\r\nSkeQb2F4bL0t0f8wyfYZfQAj9fpOAwJM79/e8qxOOehoW4/FXet3a+mrMuvgyhYD\r\nQI3zAoGAVkxTi7CoYpIPLz7OxkCB921DmLKhEsOEGkKiybf0OlKVL7rnr7Yt92bH\r\nkDxo0XY/Fh/+9KdJUZSZTI8Pu4fIF4WPQAMvh58NfyXrTAxvrB5FMTeWBV7wS4yJ\r\nzM0Z3joXp49tklAOKfV1a4MhB4iGG6G3vvwsjp6NJMeLrX0N5ws=\r\n-----END RSA PRIVATE KEY-----\r\n",          
            //  "TtlMinutes": "2"
            //}


            //using config with key as filePath

            // appsettings.json file
            //  "AccountingInterfaceTokenManagerManager": {
            //  "ServiceBaseUri": "ServiceBaseUri",
            //  "Issuer": "Issuer",
            //  "ApiKey": "ApiKey",
            //  "PrivateKeyPath": "{PATH TO YOUR FILE}",
            //  "TtlMinutes": "2"
            //}

            config = new ConfigurationBuilder()
          .AddJsonFile("appsettings.json", true, true)
          .Build();
            tokenManager = new AccountingInterfaceTokenManager(config);
            headerToken = tokenManager.GetHeaderTokenAsync().Result;
        }
    }
}
