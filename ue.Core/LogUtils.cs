// Copyright (c) 2026 Yuieii.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ue.Core
{
    public static class LogUtils
    {
        public static ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();
        
        public static ILogger GetLogger(string name)
            => LoggerFactory.CreateLogger(name);

        public static ILogger Logger
        {
            get
            {
                var stackTrace = new StackTrace();
                var name = stackTrace.GetFrame(1)
                    .ToOption()
                    .SelectMany(x => x.GetMethod().ToOption())
                    .SelectMany(m => m.DeclaringType.ToOption().Select(GetEffectiveType))
                    .Select(GetDescriptiveName)
                    .OrElse("<unknown>");
                
                return LoggerFactory.CreateLogger(name);
            }
        }

        private static Type GetEffectiveType(Type type)
        {
            while (type.IsSpecialName)
            {
                var t = type.DeclaringType;
                if (t == null) return type;
                type = t;
            }

            return type;
        }

        private static string GetDescriptiveName(Type type)
        {
            var prefix = type.Namespace
                .ToOption()
                // .Select(n => string.Join('.', n.Split('.').Select(s => string.Join("", s.Take(1)))))
                .Select(n => n + ".")
                .OrElse(string.Empty);
            
            return prefix + type.Name;
        }
    }
}