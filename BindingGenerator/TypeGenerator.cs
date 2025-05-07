using System.Text;
using CppAst;

namespace BindingGenerator;

public static class TypeGenerator
{
    private static readonly IReadOnlyList<string> PredefinedCandidates = new List<string>()
    {
        "String",
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

    private static string scriptTypeSignature(CppType type)
    {
        switch (type.TypeKind)
        {
        case CppTypeKind.Primitive:
            var primitiveType = (CppPrimitiveType)type;
            return primitiveType.Kind switch
            {
                CppPrimitiveKind.Void => "void",
                CppPrimitiveKind.Bool => "bool",
                CppPrimitiveKind.WChar => "",
                CppPrimitiveKind.Char => "int8",
                CppPrimitiveKind.Short => "int16",
                CppPrimitiveKind.Int => "int32",
                CppPrimitiveKind.Long => "",
                CppPrimitiveKind.LongLong => "int64",
                CppPrimitiveKind.UnsignedChar => "uint8",
                CppPrimitiveKind.UnsignedShort => "uint16",
                CppPrimitiveKind.UnsignedInt => "uint32",
                CppPrimitiveKind.UnsignedLong => "",
                CppPrimitiveKind.UnsignedLongLong => "uint64",
                CppPrimitiveKind.Float => "float",
                CppPrimitiveKind.Double => "double",
                CppPrimitiveKind.LongDouble => "",
                _ => throw new ArgumentOutOfRangeException()
            };
        case CppTypeKind.Pointer:
            break;
        case CppTypeKind.Reference:
            var referenceType = (CppReferenceType)type;
            return scriptTypeSignature(referenceType.ElementType) + "&";
        case CppTypeKind.Array:
            break;
        case CppTypeKind.Qualified:
            break;
        case CppTypeKind.Function:
            break;
        case CppTypeKind.Typedef:
            var typedef = (CppTypedef)type;
            return scriptTypeSignature(typedef.ElementType); // FIXME
        case CppTypeKind.StructOrClass:
            var class_ = (CppClass)type;
            if (GenerateCandidates.Contains(class_.Name) || PredefinedCandidates.Contains(class_.Name))
            {
                return class_.Name;
            }

            break;
        case CppTypeKind.Enum:
            break;
        case CppTypeKind.TemplateParameterType:
            break;
        case CppTypeKind.TemplateParameterNonType:
            break;
        case CppTypeKind.TemplateArgumentType:
            break;
        case CppTypeKind.Unexposed:
            break;
        default:
            throw new ArgumentOutOfRangeException();
        }

        // throw new NotImplementedException("stringifyType not implemented.");
        Console.WriteLine("Missing type handler: " + type.TypeKind + " " + type.FullName);
        return "";
    }

    private static string scriptFunctionSignature(CppFunction function)
    {
        string result = "";

        var resultType = scriptTypeSignature(function.ReturnType);
        if (resultType == "") return "";
        result += resultType;

        result += " " + function.Name + "(";

        for (var index = 0; index < function.Parameters.Count; index++)
        {
            var parameter = function.Parameters[index];

            if (index > 0) result += ", ";

            var parameterType = scriptTypeSignature(parameter.Type);
            if (parameterType == "") result += parameterType;

            result += parameterType + " " + parameter.Name;
        }

        result += ")";

        return result;
    }

    private static void generateClass(CppClass class_, string rootPath)
    {
        string className = class_.Name;
        string includePath = Utils.ExtractRelativePath(class_.SourceFile, "Siv3D");

        var binds = new StringBuilder();

        foreach (var field in class_.Fields)
        {
            var typeSignature = scriptTypeSignature(field.Type);
            if (typeSignature == "") continue;

            binds.AppendLine(
                $$"""
                  bind.property("{{scriptTypeSignature(field.Type)}} {{field.Name}}", &{{className}}::{{field.Name}});
                  """);
        }

        foreach (var method in class_.Functions)
        {
            if (method.IsStatic) continue;

            string functionSignature = scriptFunctionSignature(method);
            if (functionSignature == "") continue;

            string parameterTypes = method.Parameters.Select(p => p.Type.FullName).Join(", ");

            string const_ = method.IsConst ? ", const_" : "";

            binds.AppendLine(
                $$"""
                  bind.method(
                    "{{functionSignature}}",
                    overload_cast<{{parameterTypes}}>(&{{className}}::{{method.Name}}{{const_}}));
                  """);
        }

        string content =
            $$"""
              //-----------------------------------------------
              //
              //	This file is part of the Siv3D Engine.
              //
              //	Copyright (c) 2008-2025 Ryo Suzuki
              //	Copyright (c) 2016-2025 OpenSiv3D Project
              //
              //	Licensed under the MIT License.
              //
              //-----------------------------------------------

              // This file is auto-generated by asSiv3D

              # include <Siv3D/Script.hpp>
              # include <{{includePath}}>

              # include <asbind20/asbind.hpp>
              # include <asbind20/operators.hpp>

              namespace s3d
              {
                  using namespace AngelScript;

                  std::function<void()> ScriptRegister_{{className}}(asIScriptEngine* engine)
                  {
                      using namespace asbind20;
                      auto bind = asbind20::value_class<{{className}}>(engine, "{{className}}", asOBJ_POD | asOBJ_APP_CLASS_ALLINTS);
                      bind.behaviours_by_traits();
                      
                      return [engine, bind]() mutable{
                          {{binds.ToString().Trim().Replace(Environment.NewLine, Environment.NewLine + "            ")}}
                      };
                  }
              }
              """;

        var outputFilepath = Path.Combine(rootPath, GenerateDirectory + $"script_{className}.generated.cpp");
        File.WriteAllText(outputFilepath, content);

        Console.WriteLine("Wrote file to " + outputFilepath);
    }
}