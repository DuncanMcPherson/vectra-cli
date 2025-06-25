using Vectra.Bytecode.Models;

namespace Vectra.CLI;

public class Disassembler
{
    private const int IndentSize = 4;
    private int _indent;
    
    public void Disassemble(VbcProgram program)
    {
        Console.WriteLine($"Module: {program.ModuleName}");
        Console.WriteLine($"Module Type: {program.ModuleType}");
        if (program.ModuleType == VbcModuleType.Executable)
        {
            Console.WriteLine($"Module entry point: {program.EntryPointMethod}");
        }
        PrintSpace(program.RootSpace);
    }

    private void PrintSpace(VbcSpace space)
    {
        try
        {
            Console.WriteLine(space.Name.PadLeft(_indent * IndentSize));
            Console.WriteLine("{".PadLeft(_indent * IndentSize));
            _indent++;
            foreach (var type in space.Types)
            {
                PrintType(type);
            }

            foreach (var subspace in space.Subspaces)
            {
                PrintSpace(subspace);
            }
        }
        finally
        {
            _indent--;
            Console.WriteLine("}".PadLeft(_indent * IndentSize));
        }
    }

    private void PrintType(VbcType type)
    {
        switch (type)
        {
            case VbcClass cls:
                Console.WriteLine("Type: Class".PadLeft(_indent * IndentSize));
                PrintClass(cls);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }

    private void PrintClass(VbcClass cls)
    {
        try
        {
            Console.WriteLine(cls.Name.PadLeft(_indent * IndentSize));
            Console.WriteLine("{".PadLeft(_indent * IndentSize));
            _indent++;
            Console.WriteLine("Fields:".PadLeft(_indent * IndentSize));
            foreach (var field in cls.Fields)
            {
                PrintField(field);
            }

            foreach (var property in cls.Properties)
            {
                PrintProperty(property);
            }

            foreach (var method in cls.Methods)
                PrintMethod(method);
        }
        finally
        {
            _indent--;
            Console.WriteLine("}".PadLeft(_indent * IndentSize));
        }
    }

    private void PrintField(VbcField field)
    {
        Console.Write(field.Name.PadLeft(_indent * IndentSize));
        if (field.TypeName != null)
            Console.Write($" {field.TypeName}");
        if (field.InitialValue != null)
            Console.WriteLine($" = {field.InitialValue}");
    }

    private void PrintProperty(VbcProperty property)
    {
        Console.Write(property.Name.PadLeft(_indent * IndentSize));
        Console.Write($" {property.Type}");
        Console.WriteLine($"\n{"".PadLeft((_indent + 1) * IndentSize)}Has Getter: {property.HasGetter}");
        Console.WriteLine($"Has Setter: {property.HasSetter}".PadLeft((_indent + 1) * IndentSize));
    }

    private void PrintMethod(VbcMethod method)
    {
        try
        {
            Console.WriteLine($"{method.Name} ({StringifyParameters(method.Parameters)})");
            Console.WriteLine("{".PadLeft(_indent * IndentSize));
            _indent++;

            foreach (var instruction in method.Instructions)
            {
                Console.WriteLine($"{instruction.OpCode} {instruction.Operand}".PadLeft(_indent + IndentSize));
            }
        }
        finally
        {
            _indent--;
            Console.WriteLine("}".PadLeft(_indent * IndentSize));
        }
    }

    private static string StringifyParameters(List<VbcParameter> parameters)
    {
        return parameters.Aggregate("", (curr, param) =>
        {
            curr = $"{curr}{(curr.Length > 0 ? ", " : "")}{param.TypeName} {param.Name}";
            return curr;
        });
    }
}