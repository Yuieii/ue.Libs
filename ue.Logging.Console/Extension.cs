// Copyright (c) 2026 Yuieii.

using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ue.Logging.Console
{
    public enum ConsoleLogStyle
    {
        Compact,
        Cozy,
    }

    public class ConsoleStyleOptions
    {
        public ConsoleLogStyle Style { get; set; } = ConsoleLogStyle.Cozy;
    }
    
    public static class Extension
    {
        extension(ILoggingBuilder builder)
        {
            public ILoggingBuilder AddSpectreConsole() 
                => builder.AddProvider(new ConsoleProvider(new ConsoleStyleOptions()));

            public ILoggingBuilder AddSpectreConsole(ConsoleStyleOptions options)
                => builder.AddProvider(new ConsoleProvider(options));
        }
        

        private class ConsoleProvider(ConsoleStyleOptions options) : ILoggerProvider
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

        private class ConsoleLogger(string name, ConsoleStyleOptions options) : ILogger
        {
            private readonly IAnsiConsole _console = AnsiConsole.Console;
            
            public void Log<TState>(
                LogLevel logLevel, EventId eventId, TState state, Exception? exception, 
                Func<TState, Exception?, string> formatter)
            {
                var contents = formatter(state, exception)
                    .TrimEnd()
                    .Split('\n')
                    .Select(Markup.Escape)
                    .ToList();
                
                var builder = new StringBuilder();

                if (options.Style == ConsoleLogStyle.Compact)
                {
                    RenderOptionalException(contents, exception);
                    
                    foreach (var line in contents)
                    {
                        builder.Append(Markup.Escape("\r\x1b[K"));
                        builder.Append("[gray]" + Markup.Escape(DateTimeOffset.Now.ToString("[yyyy-MM-dd HH:mm:ss]")) + "[/] ");
                        OutputLevelCompact(builder, logLevel);
                        builder.AppendLine(line);
                    }
                }
                else
                {
                    const bool separate = true;
                    const char timeVertical = '┬'; // separate ? '│' : '┼';

                    if (separate)
                        builder.AppendLine(
                            ""); // "[gray]─────────────────────┼──────────────────────────────────────[/]");
                    builder.Append("[gray] " + Markup.Escape(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss")) +
                                   $" {timeVertical}[/] ");
                    OutputLevelCozy(builder, logLevel);
                    builder.AppendLine();
                    
                    RenderOptionalException(contents, exception);

                    var lineNumber = 0;
                    foreach (var line in contents)
                    {
                        builder.Append(Markup.Escape("\r\x1b[K"));
                        if (lineNumber == 0)
                        {
                            builder.Append("".PadLeft(17) + "[gray]--- │[/] ");
                        }
                        else
                        {
                            var num = (lineNumber + 1).ToString("N0");
                            builder.Append($"{num} ".PadLeft(21) + "[gray]│[/] ");
                        }

                        lineNumber++;
                        builder.AppendLine(line);
                    }
                }

                // Output all at once, making it *atomic*
                _console.Markup(builder.ToString());
            }

            private void RenderOptionalException(List<string> contents, Exception? exception)
            {
                if (exception == null) return;
                
                var segments = exception.GetRenderable().Render(
                    new RenderOptions(AnsiConsole.Profile.Capabilities,
                        new Size(AnsiConsole.Profile.Width, AnsiConsole.Profile.Height)),
                    AnsiConsole.Profile.Width);

                if (options.Style == ConsoleLogStyle.Cozy)
                    contents.Add("");
                
                contents
                    .AddRange(string.Join("", segments
                            .Select(s =>
                            {
                                if (s.Text == "\n") return "\n";
                                return $"[{s.Style.ToMarkup()}]{Markup.Escape(s.Text)}[/]";
                            }))
                        .TrimEnd()
                        .Split('\n'));
            }

            private void OutputLevelCompact(StringBuilder builder, LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Information:
                        builder.Append("[green]" + Markup.Escape($"[{name}/Info]") + "[/] ");
                        break;
                    
                    case LogLevel.Warning:
                        builder.Append("[gold1]" + Markup.Escape($"[{name}/Warn]") + "[/] ");
                        break;
                    
                    case LogLevel.Error:
                        builder.Append("[red]" + Markup.Escape($"[{name}/Error]") + "[/] ");
                        break;
                    
                    case LogLevel.Critical:
                        builder.Append("[red]" + Markup.Escape($"[{name}/Critical]") + "[/] ");
                        break;
                    
                    default:
                        builder.Append("[gray]" + Markup.Escape($"[{name}/{logLevel}]") + "[/] ");
                        break;
                }
            }
            
            private void OutputLevelCozy(StringBuilder builder, LogLevel logLevel)
            {
                switch (logLevel)
                {
                    case LogLevel.Information:
                        builder.Append("[green][white on green]" + Markup.Escape(" Info ") + "[/] " + Markup.Escape(name) + "[/] ");
                        break;
                    
                    case LogLevel.Warning:
                        builder.Append("[gold1][white on gold1]" + Markup.Escape(" Warn ") + "[/] " + Markup.Escape(name) + "[/] ");
                        break;
                    
                    case LogLevel.Error:
                        builder.Append("[red][white on red]" + Markup.Escape(" Error ") + "[/] " + Markup.Escape(name) + "[/] ");
                        break;
                    
                    case LogLevel.Critical:
                        builder.Append("[red][white on red]" + Markup.Escape(" Critical ") + "[/] " + Markup.Escape(name) + "[/] ");
                        break;
                    
                    default:
                        builder.Append("[gray][white on gray]" + Markup.Escape($" {logLevel} ") + "[/] " + Markup.Escape(name) + "[/] ");
                        break;
                }
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        }
    }
}
