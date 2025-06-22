using System.CommandLine;
using Vectra.VM.Runtime;
using Vectra.VM.Execution;

namespace Vectra.CLI;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        // TODO: Add support for '.vmod' and '.vproj' files
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
            Compiler.Compiler.Compile(file.FullName);
        });
        
        // TODO: Add support for running directly from source code (maybe in an 'interpret' command?)
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
                var vbcProgram = VbcLoader.Load(file.FullName);
                var vm = new VirtualMachine(vbcProgram);
                vm.Run();
            }
        );

        var root = new RootCommand("Vectra CLI")
        {
            buildCmd,
            runCmd
        };

        var res = root.Parse(args);
        await res.InvokeAsync();
    }
}