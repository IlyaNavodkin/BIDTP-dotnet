using System.Text.RegularExpressions;

namespace Example.Client.WPF.Args;

/// <summary>
///  Class for parsing command line arguments
/// </summary>
public class ArgumentParseUtil
{
    /// <summary>
    ///  Parse command line arguments
    /// </summary>
    /// <param name="args"> The command line arguments </param>
    /// <returns> The parsed command line arguments </returns>
    public static CommandLineArguments? Parse(string[] args)
    {
        var result = new CommandLineArguments();

        var serverNameRegex = new Regex(@"^--pn=(.*)$", RegexOptions.IgnoreCase);
        var processIdRegex = new Regex(@"^--pid=(.*)$", RegexOptions.IgnoreCase);

        foreach (var arg in args)
        {
            var serverNameIsMatch = serverNameRegex.Match(arg);
            if (serverNameIsMatch.Success)
            {
                result.PipeName = serverNameIsMatch.Groups[1].Value;
                continue;
            }

            var processIdIsMatch = processIdRegex.Match(arg);
            if (processIdIsMatch.Success)
            {
                result.OwnerProcessId = processIdIsMatch.Groups[1].Value;
            }
        }

        return result;
    }
}