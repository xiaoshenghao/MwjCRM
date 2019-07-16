using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WJewel.Basic;

namespace CRM.Customer.Service
{
    /// <summary>
    /// Redis单例
    /// </summary>
    public class RedisService
    {
        public static RedisHelper db = null;

        private static readonly object objLock = new object();

        public static RedisHelper Instance
        {
            get
            {
                if (db == null)
                {
                    lock (objLock)
                    {
                        if (db == null)
                        {
                            db = new RedisHelper(AppConfigs.RedisDb, AppConfigs.RedisConnectHost);
                        }
                    }
                }
                return db;
            }
        }
    }
}
