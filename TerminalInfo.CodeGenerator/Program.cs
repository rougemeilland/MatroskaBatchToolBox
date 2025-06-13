using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TerminalInfo.CodeGenerator
{
    internal static partial class Program
    {
        [SuppressMessage("Style", "IDE0060:未使用のパラメーターを削除します", Justification = "<保留中>")]
        private static void Main(string[] args)
        {
            var legacyCapabilityNames = new Dictionary<string, string>();

            foreach (var capabilitySymbolItemForType in EnumerateTermInfoCapabilitySymbols().GroupBy(item => item.type).Select(g => new { type = g.Key, symbolItems = g.Select(item => new { item.name, item.value, }).OrderBy(item => item.value) }))
            {
                var enumTypeName =
                    capabilitySymbolItemForType.type == "Booleans"
                    ? "TermInfoBooleanCapabilities"
                    : capabilitySymbolItemForType.type == "Numbers"
                    ? "TermInfoNumberCapabilities"
                    : capabilitySymbolItemForType.type == "Strings"
                    ? "TermInfoStringCapabilities"
                    : throw new Exception();

                Console.WriteLine();
                Console.WriteLine($"internal enum {enumTypeName}");
                Console.WriteLine("{");
                foreach (var capabilitySymbolItem in capabilitySymbolItemForType.symbolItems)
                {
                    Console.WriteLine($"    {ToCamelCase(capabilitySymbolItem.name)} = {capabilitySymbolItem.value},");
                    legacyCapabilityNames.Add(capabilitySymbolItem.name, capabilitySymbolItem.name);
                }

                Console.WriteLine("}");
                Console.WriteLine();
            }

            Console.WriteLine();

            foreach (var capabilitySymbolItemForType in EnumerateTermInfoCapabilitySymbols().GroupBy(item => item.type).Select(g => new { type = g.Key, symbolItems = g.Select(item => new { item.name, item.value, }).OrderBy(item => item.value) }))
            {
                var enumTypeName =
                    capabilitySymbolItemForType.type == "Booleans"
                    ? "TermInfoBooleanCapabilities"
                    : capabilitySymbolItemForType.type == "Numbers"
                    ? "TermInfoNumberCapabilities"
                    : capabilitySymbolItemForType.type == "Strings"
                    ? "TermInfoStringCapabilities"
                    : throw new Exception();

                Console.WriteLine();
                Console.WriteLine($"        public static string ToJsonPropertyName(this {enumTypeName} valueName)");
                Console.WriteLine("            => valueName switch");
                Console.WriteLine("            {");
                foreach (var capabilitySymbolItem in capabilitySymbolItemForType.symbolItems)
                    Console.WriteLine($"                {enumTypeName}.{ToCamelCase(capabilitySymbolItem.name)} => \"{capabilitySymbolItem.name}\",");
                Console.WriteLine("                _ => throw new ArgumentException($\"Not supported value name: {valueName}\", nameof(valueName)),");
                Console.WriteLine("            };");
                Console.WriteLine();
            }

            Console.WriteLine();

            var capabiltyItems =
                EnumerateTermInfoCapabilties()
                .GroupBy(item => item.name)
                .Select(g => new
                {
                    name = g.Key,
                    type =
                        g.Select(item => item.type)
                        .Distinct()
                        .Single(),
                    valueImtems =
                        g.Select(item => item.value)
                        .GroupBy(item => item)
                        .Select(g2 => new { value = g2.Key, count = g2.Count() })
                        .ToList(),
                })
                .OrderBy(item => item.name);

            foreach (var capabilityItem in capabiltyItems)
            {
                if (capabilityItem.name[0] != '"')
                    throw new Exception();
                var capabiltyName = capabilityItem.name[1..^1];
                var canalcasedCapabilityName = ToCamelCase(capabiltyName);
                Console.WriteLine();
                Console.WriteLine($"        // Capability name: {capabiltyName}");
                var terminalCount = capabilityItem.valueImtems.Sum(valueItem => valueItem.count);
                Console.WriteLine($"        // Terminals supporting this capability: {(terminalCount > 1 ? $"{terminalCount} terminals" : $"{terminalCount} terminal")}");
                Console.WriteLine("        // Values of this capability:");
                foreach (var valueItem in capabilityItem.valueImtems.OrderByDescending(valueItem => valueItem.count).ThenBy(valueItem => valueItem.value))
                    Console.WriteLine($"        //   {valueItem.value} ({(valueItem.count > 1 ? $"{valueItem.count} terminals" : $"{valueItem.count} terminal")})");

                var maxVariableNumber =
                    capabilityItem.valueImtems
                        .SelectMany(valueItem =>
                            GetStringValueVariableNamePattern().Matches(valueItem.value)
                            .Select(match => int.Parse(match.Groups["n"].Value, NumberStyles.None, CultureInfo.InvariantCulture)))
                        .Append(0)
                        .Max();

                var isExtendedCapability = !legacyCapabilityNames.ContainsKey(capabiltyName);

                Console.WriteLine($"        /// <summary>Get the value of {(isExtendedCapability ? "extended " : "")}capability \"{capabiltyName}\".</summary>");
                for (var variableNumber = 1; variableNumber <= maxVariableNumber; ++variableNumber)
                    Console.WriteLine($"        /// <param name=\"p{variableNumber}\"></param>");
                Console.WriteLine($"        /// <returns>If not null it is the value of the {(isExtendedCapability ? "extended " : "")}capability \"{capabiltyName}\". If null, this terminal information does not support the {(isExtendedCapability ? "extended " : "")}capability \"{capabiltyName}\".</returns>");

                var (parameterDefinitionList, parameterList, comment) =
                    maxVariableNumber > 0
                    ? ($"({string.Join(", ", Enumerable.Range(1, maxVariableNumber).Select(n => $"int p{n}"))})", $"{string.Concat(Enumerable.Range(1, maxVariableNumber).Select(n => $", p{n}"))}", " // TODO: パラメタの型と名前を変更する")
                    : ("", "", "");
                Console.WriteLine(
                    capabilityItem.type switch
                    {
                        "bool" => $"        public bool? {canalcasedCapabilityName} => _database.GetBooleanCapabilityValue({(isExtendedCapability ? $"\"{capabiltyName}\"" : $"TermInfoBooleanValues.{canalcasedCapabilityName}")});",
                        "int" => $"        public int? {canalcasedCapabilityName} => _database.GetNumberCapabilityValue({(isExtendedCapability ? $"\"{capabiltyName}\"" : $"TermInfoNumberValues.{canalcasedCapabilityName}")});",
                        "string" => $"        public string? {canalcasedCapabilityName}{parameterDefinitionList} => _database.GetStringCapabilityValue({(isExtendedCapability ? $"\"{capabiltyName}\"" : $"TermInfoStringValues.{canalcasedCapabilityName}")}{parameterList});{comment}",
                        _ => throw new Exception(),
                    });
            }
        }

        private static IEnumerable<(string type, string name, string value)> EnumerateTermInfoCapabilties()
        {
            using var reader = new StreamReader("terminfo_variables.txt", Encoding.UTF8);
            while (true)
            {
                var line = reader.ReadLine();
                if (line is null)
                    break;
                var columns = line.Split('\t');
                yield return
                    columns.Length == 3
                    ? (columns[2], columns[0], columns[1])
                    : throw new Exception();
            }
        }

        private static IEnumerable<(string type, string name, int value)> EnumerateTermInfoCapabilitySymbols()
        {
            using var reader = new StreamReader("termInfo_capability_symbols.txt", Encoding.UTF8);
            while (true)
            {
                var line = reader.ReadLine();
                if (line is null)
                    break;
                var columns = line.Split('\t');
                yield return
                    columns.Length == 3
                    ? ((string type, string name, int value))(columns[0], columns[1], int.Parse(columns[2], NumberStyles.None, CultureInfo.InvariantCulture))
                    : throw new Exception();
            }
        }

        private static string ToCamelCase(string source)
            => GetSnakeCasePattern().Replace(source, match => match.Groups["c"].Value.ToUpperInvariant());
        [GeneratedRegex(@"%p(?<n>\d)", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetStringValueVariableNamePattern();

        [GeneratedRegex(@"(^|_)(?<c>[a-z])", RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex GetSnakeCasePattern();
    }
}
