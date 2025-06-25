using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using NuGet.Versioning;
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

    [ExcludeFromCodeCoverage]
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
            RunUpdateCheckAsync().Wait();
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

        var infoCmd = new Command("info", "Prints the version information");
        infoCmd.SetAction(_ =>
        {
            services.PrintVersionInfo();
            RunUpdateCheckAsync().Wait();
        });

        var astCmd = new Command("ast", "Prints the AST of a Vectra project")
        {
            Arguments =
            {
                new Argument<FileInfo>("input")
                {
                    Description = "The .vec file to compile"
                }
            }
        };
        astCmd.SetAction(parse =>
        {
            var file = parse.GetRequiredValue<FileInfo>("input");
            ValidateFile(file, ".vec");
            var module = Compiler.Compiler.GetAST(file.FullName);
            if (module == null)
            {
                Console.Error.WriteLine("Failed to parse AST");
                return 1;
            }
            
            Console.WriteLine(module.RootSpace);
            RunUpdateCheckAsync().Wait();
            return 0;
        });

        var disasmCmd = new Command("disasm", "Disassembles a Vectra project")
        {
            new Argument<FileInfo>("input")
            {
                Description = "Prints out the disassembly of the given .vbc file"
            }
        };
        disasmCmd.SetAction((parse) =>
        {
            var file = parse.GetRequiredValue<FileInfo>("input");
            ValidateFile(file, ".vbc");
            var program = VbcLoader.Load(file.FullName);
            var disassembler = new Disassembler();
            disassembler.Disassemble(program);
            RunUpdateCheckAsync().Wait();
        });

        return new RootCommand("Vectra CLI")
        {
            buildCmd,
            runCmd,
            infoCmd,
            astCmd,
            disasmCmd,
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

    private static async Task RunUpdateCheckAsync()
    {
        var configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".vectra");
        var cacheFile = Path.Combine(configDir, "vecc-version-check.json");
        if (!Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

        if (File.Exists(cacheFile))
        {
            var json = await File.ReadAllTextAsync(cacheFile);
            var obj = JsonDocument.Parse(json).RootElement;

            var lastChecked = obj.GetProperty("lastChecked").GetDateTime();
            if ((DateTime.UtcNow - lastChecked).TotalHours < 24)
            {
                var latest = obj.GetProperty("latest").GetString();
                NotifyIfOutdated(currentVersion!, latest!);
                return;
            }
        }

        var latestVersion = await FetchLatestVersionFromGitHubAsync();
        if (!string.IsNullOrEmpty(latestVersion))
        {
            await File.WriteAllTextAsync(cacheFile, JsonSerializer.Serialize(new
            {
                lastChecked = DateTime.UtcNow,
                latest = latestVersion
            }, new JsonSerializerOptions {WriteIndented = true}));
            
            NotifyIfOutdated(currentVersion!, latestVersion);
        }
    }

    private static void NotifyIfOutdated(string currentVersion, string latestVersion)
    {
        if (NuGetVersion.TryParse(currentVersion, out var current) &&
            NuGetVersion.TryParse(latestVersion, out var latest) &&
            latest > current)
        {
            Console.WriteLine($"\n✨ A new version of VectraCLI is available: {currentVersion} -> {latestVersion}");
            Console.WriteLine("👉 Download from https://github.com/DuncanMcPherson/vectra-cli/releases");
        }
    }

    private static async Task<string?> FetchLatestVersionFromGitHubAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("vecc-cli");
        
        var res = await client.GetAsync("https://api.github.com/repos/DuncanMcPherson/vectra-cli/releases/latest");
        if (!res.IsSuccessStatusCode) return null;
        
        var json = await res.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("tag_name").GetString()?.TrimStart('v');
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

    public virtual void PrintVersionInfo()
    {
        Console.WriteLine($"Vectra CLI v{Assembly.GetEntryAssembly()?.GetName().Version}");
        Console.WriteLine("Copyright (c) 2025 Vectra");
        Console.WriteLine($"- Vectra.AST: {typeof(AST.VectraASTModule).Assembly.GetName().Version}");
        Console.WriteLine($"- Vectra.VM: {typeof(VirtualMachine).Assembly.GetName().Version}");
        Console.WriteLine($"- Vectra.Compiler: {typeof(Compiler.Compiler).Assembly.GetName().Version}");
        Console.WriteLine($"- Vectra.Bytecode: {typeof(Bytecode.BytecodeReader).Assembly.GetName().Version}");
    }
}