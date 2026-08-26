using System.Collections.Generic;
using System.Threading.Tasks;

namespace AccountingInterfaceToken
{
    public interface IAccountingInterfaceTokenManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<string> GetTokenAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Task<KeyValuePair<string, string>> GetHeaderTokenAsync();

        /// <summary>
        /// 
        /// </summary>
        void Reset();
    }
}
