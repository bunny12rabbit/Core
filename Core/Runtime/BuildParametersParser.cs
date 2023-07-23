using System;
using System.Collections.Generic;
using System.Linq;

namespace Common.Core
{
    public static class BuildParametersParser
    {
        private delegate void ArgumentDelegate(string name, string value);

        public static void Parse(IReadOnlyList<IBuildParameter> parameters)
        {
            ParseEnvironmentVariables(parameters);
            ParseCommandLineArguments(parameters);
        }

        public static void ParseEnvironmentVariables(IEnumerable<IBuildParameter> buildParameters)
        {
            foreach (var parameter in buildParameters)
            {
                var value =
                    Environment.GetEnvironmentVariable(parameter.Name) ??
                    Environment.GetEnvironmentVariable(parameter.Name.ToUpper()) ??
                    Environment.GetEnvironmentVariable(parameter.Name.ToLower());

                if (value != null)
                    parameter.Parse(value);
            }
        }

        public static void ParseCommandLineArguments(IReadOnlyList<IBuildParameter> buildParameters)
        {
            var commandLineArgs = Environment.GetCommandLineArgs();

            ForEachArgument(commandLineArgs, (name, value) =>
            {
                var parameter = buildParameters.FirstOrDefault(o => o.Name == name);
                parameter?.Parse(value);
            });
        }

        private static void ForEachArgument(IEnumerable<string> args, ArgumentDelegate visit)
        {
            bool IsParamName(string arg) => arg.StartsWith("-");

            var paramName = string.Empty;

            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(paramName))
                {
                    if (IsParamName(arg))
                        paramName = arg;

                    continue;
                }

                if (IsParamName(arg))
                {
                    paramName = arg;
                    continue;
                }

                visit(paramName.Substring(1), arg);
                paramName = string.Empty;
            }
        }
    }
}