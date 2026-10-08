using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>Parses C# files in a directory and reports syntax errors.</summary>
class CheckSyntax
{
    /// <summary>Runs the syntax check for all C# files beneath the supplied directory.</summary>
    /// <param name="args">Command-line arguments; the first argument is the source directory.</param>
    /// <returns>Zero when no syntax errors are found; otherwise a nonzero value.</returns>
    static int Main(string[] args)
    {
        int errors = 0, count = 0;
        foreach (var path in Directory.GetFiles(args[0], "*.cs", SearchOption.AllDirectories))
        {
            count++;
            foreach (var diagnostic in CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path).GetDiagnostics())
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                { Console.WriteLine(diagnostic); errors++; }
        }
        Console.WriteLine("C# syntax: " + count + " files, " + errors + " errors. Not an Acumatica compilation.");
        return errors == 0 ? 0 : 1;
    }
}
