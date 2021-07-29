using System;
using Microsoft.Extensions.Logging;

namespace ElasticLoggingService
{
    public class LogRequest
    {
        public LogLevel LogLevel { get; set; }
        public Exception Exception { get; set; }
        public string Message { get; set; }
        public string Secret { get; set; }
    }
}