using System.Text;
using CppAst;

namespace BindingGenerator;

internal static class Program
{
    static void Main()
    {
        var currentDirectory = Environment.CurrentDirectory;
        Console.WriteLine(currentDirectory);

        var repositoryRoot = Utils.FindAncestorDirectory(currentDirectory, "asSiv3D");
        if (repositoryRoot == null)
        {
            Console.WriteLine("Could not find the OpenSiv3D repository root.");
            return;
        }

        var headerFile = Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/include/Siv3D.hpp");
        var headerContent = File.ReadAllText(headerFile);

        var parseOption =
            new CppParserOptions().ConfigureForWindowsMsvc(CppTargetCpu.X86_64, (CppVisualStudioVersion)1943);

        parseOption.AdditionalArguments.Add("-std=c++20");

        parseOption.IncludeFolders.AddRange([
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/include"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/include/ThirdParty"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/src"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/src/Siv3D-Platform/WindowsDesktop"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/src/Siv3D-Platform/OpenGL4"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/src/ThirdParty"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/src/ThirdParty/freetype"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/src/ThirdParty/skia"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/src/ThirdParty/soloud/include"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Siv3D/src/ThirdParty-prebuilt"),
            Path.Combine(repositoryRoot, "OpenSiv3D/Dependencies/boost_1_83_0"),

            Path.Combine(repositoryRoot, "BindingGenerator/dummy"), // ?
        ]);

        // Parse a C++ files
        var compilation = CppParser.Parse(headerContent, parseOption);

        var s3d = compilation.Namespaces.First(n => n.Name == "s3d");
        if (s3d == null)
        {
            Console.WriteLine("Could not find the s3d namespace.");
            return;
        }

        var sb = new StringBuilder();
        foreach (var class_ in s3d.Classes)
        {
            sb.AppendLine($"{class_.FullName}");
            foreach (var function in class_.Functions)
            {
                sb.AppendLine(
                    $"    {function.Name}({string.Join(", ", function.Parameters.Select(p => p.Type.ToString()))})");
            }
        }

        var outputFile = Path.Combine(repositoryRoot, "BindingGenerator/output.txt");
        File.WriteAllText(outputFile, sb.ToString());

        Console.WriteLine(sb.ToString());
    }
}