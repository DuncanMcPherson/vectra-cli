using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Vectra.VM.Runtime;
using Vectra.VM.Execution;

[assembly: InternalsVisibleTo("Vectra.CLI.Tests")]

namespace Vectra.CLI;

internal static class Program
{
    [ExcludeFromCodeCoverage]
    private static async Task<int> Main(string[] args)
    {
        // TODO: Add support for '.vmod' and '.vproj' files
        // TODO: Add support for running directly from source code (maybe in an 'interpret' command?)
        return await BuildAndInvokeRootCommand(args, new ProgramServices());
    }

    internal static async Task<int> BuildAndInvokeRootCommand(string[] args, ProgramServices services)
    {
        var root = CreateRootCommand(services);
        var parseResult = root.Parse(args);
        var result = await parseResult.InvokeAsync();
        return result;
    }

    private static RootCommand CreateRootCommand(ProgramServices services)
    {
        var buildCmd = new Command("build", "Compiles a Vectra project to .vbc")
        {
            new Argument<FileInfo>("input")
            {
                Description = "The .vec file to compile"
            }
        };

        buildCmd.SetAction(res =>
        {
            var file = res.GetRequiredValue<FileInfo>("input");
            ValidateFile(file, ".vec");
            services.Compile(file.FullName);
        });

        var runCmd = new Command("run", "Runs a Vectra project")
        {
            new Argument<FileInfo>("input")
            {
                Description = "The .vbc file to run"
            }
        };

        runCmd.SetAction(res =>
        {
            var file = res.GetRequiredValue<FileInfo>("input");
            ValidateFile(file, ".vbc");
            services.RunVirtualMachine(file.FullName);
        });

        return new RootCommand("Vectra CLI")
        {
            buildCmd,
            runCmd
        };
    }

    [ExcludeFromCodeCoverage]
    private static void ValidateFile(FileInfo file, string expectedExtension)
    {
        if (!file.Exists)
            throw new FileNotFoundException($"File not found: {file.FullName}");

        if (!file.Extension.Equals(expectedExtension, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(
                $"Invalid file extension. Expected: {expectedExtension}, Got: {file.Extension}");
    }
}

[ExcludeFromCodeCoverage]
public class ProgramServices
{
    public virtual void Compile(string input) => Compiler.Compiler.Compile(input);

    public virtual void RunVirtualMachine(string path)
    {
        var vbcProgram = VbcLoader.Load(path);
        var vm = new VirtualMachine(vbcProgram);
        vm.Run();
    }
}