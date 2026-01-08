// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Logging;
using ue.Core;
using ue.Logging.Console;

LogUtils.LoggerFactory = LoggerFactory.Create(builder => builder.AddSpectreConsole());

var logger = LogUtils.Logger;

try
{
    throw new Exception("Some test exception.");
} 
catch (Exception e)
{
    logger.LogError(e, "Testing exception logging and output a very long text to see how it wraps! Testing exception logging and output a very long text to see how it wraps! Testing exception logging and output a very long text to see how it wraps!");
}

logger.LogWarning("This is a warning. You have been warned!");
logger.LogInformation("Regular information");
logger.LogDebug("Some debugging logs");
