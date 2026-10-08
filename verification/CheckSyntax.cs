using System;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

class CheckSyntax
{
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
