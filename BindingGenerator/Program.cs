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

        TypeGenerator.Generate(compilation, repositoryRoot);
    }
}