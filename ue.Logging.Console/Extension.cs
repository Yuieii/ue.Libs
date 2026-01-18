// Copyright (c) 2026 Yuieii.

using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;
using ue.Core;

namespace ue.Logging.Console
{
    public enum ConsoleLogStyle
    {
        Compact,
        Cozy,
    }

    public class ConsoleLoggerOptions
    {
        public IAnsiConsole? Console { get; set; }
        public ConsoleLogStyle Style { get; set; } = ConsoleLogStyle.Cozy;
    }
    
    public static class Extension
    {
        extension(ILoggingBuilder builder)
        {
            public ILoggingBuilder AddSpectreConsole() 
                => builder.AddProvider(new ConsoleProvider(new ConsoleLoggerOptions()));

            public ILoggingBuilder AddSpectreConsole(ConsoleLoggerOptions options)
                => builder.AddProvider(new ConsoleProvider(options));
        }
        

        private class ConsoleProvider(ConsoleLoggerOptions options) : ILoggerProvider
        {
            private readonly ConcurrentDictionary<string, ILogger> _loggers = new();
            
            public void Dispose()
            {
                // TODO release managed resources here
            }

            public ILogger CreateLogger(string categoryName)
            {
                return _loggers.GetOrAdd(categoryName, k => new ConsoleLogger(k, options));
            }
        }

        private class ConsoleLogger(string name, ConsoleLoggerOptions options) : ILogger
        {
            private readonly IAnsiConsole _console = options.Console ?? AnsiConsole.Console;
            
            public void Log<TState>(
                LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
                Func<TState, Exception?, string> formatter)
            {
                var formatted = formatter(state, exception);
                
                Renderable entry = options.Style switch
                {
                    ConsoleLogStyle.Compact => new LogEntryCompact(name, logLevel, eventId, formatted, exception),
                    _ => new LogEntryCozy(name, logLevel, eventId, formatted, exception),
                };
                
                _console.Write(entry);
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        }
    }

    internal class LogEntryCompact(string name, LogLevel level, EventId eventId, string formatted, Exception? exception)
        : Renderable
    {
        private List<Segment> CreateHeader()
        {
            var style = new Style(Color.Gray);
            var badgeText = level.ToString();
            
            switch (level)
            {
                case LogLevel.Information:
                    style = new Style(Color.Green);
                    badgeText = "Info";
                    break;
                    
                case LogLevel.Warning:
                    style = new Style(Color.Gold1);
                    badgeText = "Warn";
                    break;
                    
                case LogLevel.Error:
                    style = new Style(Color.Red);
                    break;
                    
                case LogLevel.Critical:
                    style = new Style(Color.Red);
                    break;
            }

            return
            [
                new Segment($"[{name}/{badgeText}]", style)
            ];
        }

        protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            // Start building the panel
            var result = new List<Segment>();

            var timestamp = DateTimeOffset.Now.ToString("[yyyy-MM-dd HH:mm:ss] ");
            
            // Header
            result.Add(new Segment(timestamp, new Style(Color.Gray)));

            var header = CreateHeader();
            result.AddRange(header);
            result.Add(new Segment(": "));

            // Content
            var content = new Text(formatted);
            var padLeft = timestamp.Length + header.Sum(s => s.Text.Length) + 2;
            var segments = ((IRenderable) content).Render(options, maxWidth - padLeft);

            var lineNumber = 1;
            foreach (var line in Segment.SplitLines(segments))
            {
                if (lineNumber > 1)
                {
                    result.Add(new Segment($"[{lineNumber:N0}]".PadLeft(padLeft - 3), new Style(Color.Gray)));
                    result.Add(new Segment(" | ", new Style(Color.Gray)));
                }
                
                result.AddRange(line);
                result.Add(Segment.LineBreak);
                lineNumber++;
            }
            
            if (exception != null)
            {
                var exceptionSegments = exception.GetRenderable().Render(options, maxWidth - padLeft);
            
                foreach (var line in Segment.SplitLines(exceptionSegments))
                {
                    result.Add(new Segment("".PadLeft(padLeft - 3), new Style(Color.Gray)));
                    result.Add(new Segment(" | ", new Style(Color.Gray)));
                    result.AddRange(line);
                    result.Add(Segment.LineBreak);
                }
            }

            return result;
        }
    }

    internal class LogEntryCozy(string name, LogLevel level, EventId eventId, string formatted, Exception? exception) : Renderable
    {
        private List<Segment> CreateHeader(RenderOptions options)
        {
            var style = new Style(Color.Gray);
            var badgeText = level.ToString();
            var badgeColorOverride = null as Color?;
            
            switch (level)
            {
                case LogLevel.Information:
                    style = new Style(Color.DarkOliveGreen1);
                    badgeColorOverride = Color.DarkOliveGreen3;
                    badgeText = "Info";
                    break;
                    
                case LogLevel.Warning:
                    style = new Style(Color.Gold1);
                    badgeColorOverride = Color.DarkOrange3;
                    badgeText = "Warn";
                    break;
                    
                case LogLevel.Error:
                    style = new Style(Color.Red);
                    badgeColorOverride = Color.White;
                    break;
                    
                case LogLevel.Critical:
                    style = new Style(Color.Red);
                    badgeColorOverride = Color.White;
                    break;
            }

            var invertedStyle = new Style(badgeColorOverride ?? style.Background, style.Foreground, Decoration.Bold);

            var noColor = !options.Ansi || options.ColorSystem == ColorSystem.NoColors;
            var prefix = noColor ? "[" : " ";
            var suffix = noColor ? "]" : " ";
            
            return
            [
                new Segment($"{prefix}{badgeText}{suffix}", invertedStyle),
                new Segment(" "),
                new Segment(name, style)
            ];
        }

        protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            // Start building the panel
            var result = new List<Segment>();

            var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var sideColumnWidth = timestamp.Length + 5;
            
            // Top Margin
            result.Add(Segment.LineBreak);
            
            // Header
            result.Add(new Segment(timestamp, new Style(Color.Gray)));
            result.Add(new Segment(" ┬ ", new Style(Color.Gray)));
            result.AddRange(CreateHeader(options));
            result.Add(Segment.LineBreak);
            
            // Content
            var content = new Text(formatted);
            var segments = ((IRenderable) content).Render(options, maxWidth - sideColumnWidth);

            var lineNumber = 1;
            
            foreach (var line in Segment.SplitLines(segments))
            {
                if (lineNumber == 1)
                {
                    result.Add(new Segment("---".PadLeft(timestamp.Length), new Style(Color.Gray)));
                }
                else
                {
                    result.Add(new Segment($"{lineNumber:N0}".PadLeft(timestamp.Length)));
                }

                result.Add(new Segment(" │ ", new Style(Color.Gray)));
                result.AddRange(line);
                result.Add(Segment.LineBreak);
                lineNumber++;
            }

            if (exception != null)
            {
                var panel = new Panel(exception.GetRenderable())
                {
                    Border = BoxBorder.Rounded,
                    BorderStyle = new Style(Color.Red)
                };
                
                var exceptionSegments = ((IRenderable) panel).Render(options, maxWidth - sideColumnWidth);
            
                foreach (var line in Segment.SplitLines(exceptionSegments))
                {
                    result.Add(new Segment("".PadLeft(timestamp.Length)));
                    result.Add(new Segment(" │ ", new Style(Color.Gray)));
                    result.AddRange(line);
                    result.Add(Segment.LineBreak);
                }
            }
            
            return result;
        }
    }
}
