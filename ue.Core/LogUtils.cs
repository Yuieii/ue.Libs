// Copyright (c) 2026 Yuieii.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ue.Core
{
    public static class LogUtils
    {
        public static ILoggerFactory LoggerFactory { get; set; } = new NullLoggerFactory();
        
        private static readonly Dictionary<string, ILogger> _loggers = new();
        
        public static ILogger GetLogger(string name)
        {
            if (_loggers.TryGetValue(name, out var logger)) return logger;
            
            logger = LoggerFactory.CreateLogger(name);
            _loggers.Add(name, logger);
            return logger;
        }

        public static ILogger GetLogger<T>()
            => GetLogger(GetDescriptiveName(typeof(T)));

        public static ILogger Logger
        {
#if NET7_0_OR_GREATER
            [RequiresUnreferencedCode("Name inferring requires getting metadata from stack trace")]
#endif
            get
            {
                var stackTrace = new StackTrace();
                var frame = stackTrace.GetFrame(1).ToOption();
                    
                var name = frame
                    .SelectMany(x => x.GetMethod().ToOption())
                    .SelectMany(m => m.DeclaringType.ToOption().Select(GetEffectiveType))
                    .Select(GetDescriptiveName)
                    .OrElse("<unknown>");
                
                return GetLogger(name);
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