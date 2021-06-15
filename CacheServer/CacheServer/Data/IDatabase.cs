using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CacheServer.Data
{
    interface IDatabase
    {
        public void Set(string key, string value);
        public string Get(string key);
    }
}
