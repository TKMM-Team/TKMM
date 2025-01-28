// Copyright (c) Tkmm Team and Contributors. MIT.
// https://github.com/Ryubing/Tkmm/blob/master/LICENSE.txt
// 
// Modified by TKMM-Team

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Tkmm.LocaleGenerator;

[Generator]
public class LocaleGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<AdditionalText> localeFile = context.AdditionalTextsProvider.Where(static x => x.Path.EndsWith("locales.json"));

        IncrementalValuesProvider<string> contents = localeFile.Select((text, cancellationToken) => text.GetText(cancellationToken)!.ToString());

        context.RegisterSourceOutput(contents, (spc, content) => {
            IEnumerable<string> lines = content
                .Split('\n')
                .Where(line => IsKey(line.AsSpan()))
                .Select(GetKey);

            StringBuilder enumSourceBuilder = new();
            enumSourceBuilder.AppendLine("namespace Tkmm.Localization;\n");
            enumSourceBuilder.AppendLine("public enum TkLocale");
            enumSourceBuilder.AppendLine("{");
            
            foreach (string? line in lines) {
                enumSourceBuilder.AppendLine($"    {line},");
            }

            enumSourceBuilder.AppendLine("}");

            spc.AddSource("Tkmm.Localization.TkLocale.g.cs", enumSourceBuilder.ToString());
        });
    }

    private static bool IsKey(ReadOnlySpan<char> line)
    {
        if (line.Length < 5) {
            return false;
        }

        return line.Slice(0, 5) is "    \"" && line[line.Length - 2] is '{';
    }

    private static string GetKey(string line)
    {
        int keyEndIndex = line.AsSpan().IndexOf(':');
        return line.Substring(5, keyEndIndex - 6);
    }
}