using LocobuzzTelemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcomReviews
{
    public class Logger
    {
        private static ILogger _instance;
        private static readonly object _lock = new object();
        static string configString = AppSettings.TelementryConnectionString;

        public static ILogger Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new AzureTelemetryLogger(configString);
                    }

                    return _instance;
                }
            }
        }
    }
}
