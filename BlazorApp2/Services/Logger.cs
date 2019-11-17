using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorApp2.Services
{
    public class FileLogger : ILogger
    {
        private string filePath;
        private string _category;
        private static object _lock = new object();
        public FileLogger(string category, string path)
        {
            //filePath = path;
            filePath = Path.Combine(Directory.GetCurrentDirectory(), "logger.log");
            _category = category;
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            //if(_category == "FileLogger" )
            if ((int)logLevel >= 1)
                return true;
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter != null && IsEnabled(logLevel))
            {
                lock (_lock)
                {
                    File.AppendAllText(filePath, $"{logLevel.ToString()}: {_category}[{eventId.Id}] \r\n    "
                        + formatter(state, exception) + Environment.NewLine);
                    //File.AppendAllText(filePath, $"EventId: {eventId.Id} || EventName: {eventId.Name} || LogLevel: {logLevel.ToString()} || Category: {_category} && " 
                    //    + formatter(state, exception) + Environment.NewLine);
                }
            }
        }
    }

    public class FileLoggerProvider : ILoggerProvider
    {
        private string path;
        public FileLoggerProvider(string _path)
        {
            path = _path;
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(categoryName, path);
        }

        public void Dispose()
        {
        }
    }
}
