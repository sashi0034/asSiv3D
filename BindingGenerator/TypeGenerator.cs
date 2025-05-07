using System.Text;
using CppAst;

namespace BindingGenerator;

public static class TypeGenerator
{
    private static readonly IReadOnlyList<string> PredefinedCandidates = new List<string>()
    {
        "String",
        "size_t",
    };

    private static readonly IReadOnlyList<string> GenerateCandidates = new List<string>()
    {
        "Color"
    };

    private const string GenerateDirectory = "OpenSiv3D/Siv3D/src/Siv3D/Script/Bind_Generated/";

    public static void Generate(CppCompilation ast, string rootPath)
    {
        var s3d = ast.Namespaces.First(n => n.Name == "s3d");
        if (s3d == null)
        {
            Console.WriteLine("Could not find the s3d namespace.");
            return;
        }

        var sb = new StringBuilder();
        foreach (var class_ in s3d.Classes)
        {
            if (GenerateCandidates.Contains(class_.Name))
            {
                generateClass(class_, rootPath);
            }
        }

        // var outputFile = Path.Combine(rootPath, "BindingGenerator/output.txt");
        // File.WriteAllText(outputFile, sb.ToString());
        //
        // Console.WriteLine(sb.ToString());
    }

    private static void generateClass(CppClass class_, string rootPath)
    {
        var sb = new StringBuilder();
        foreach (var function in class_.Functions)
        {
            sb.AppendLine(
                $"    {function.Name}({string.Join(", ", function.Parameters.Select(p => p.Type.ToString()))})");
        }

        string className = class_.Name;
        var outputFile = Path.Combine(rootPath, GenerateDirectory + $"script_{className}.generated.cpp");
        File.WriteAllText(outputFile, sb.ToString());

        Console.WriteLine(sb.ToString());
    }
}