using FluentAssertions;
using Moq;
using Vectra.CLI.Tests.Helpers;

namespace Vectra.CLI.Tests;

[TestFixture]
public class ProgramTests
{
    private Mock<ProgramServices> _servicesMock;
    
    [SetUp]
    public void Setup()
    {
        _servicesMock = new Mock<ProgramServices>();
    }
    
    [Test]
    public async Task NoArguments_ReturnsNonZero()
    {
        var args = Array.Empty<string>();
        var result = await Program.BuildAndInvokeRootCommand(args, _servicesMock.Object);
        result.Should().NotBe(0);
    }
    
    [Test]
    public async Task Help_ReturnsZero()
    {
        var args = new[] {"--help"};
        var result = await Program.BuildAndInvokeRootCommand(args, _servicesMock.Object);
        result.Should().Be(0);
    }
    
    [TestCase("build")]
    [TestCase("run")]
    public async Task Command_WithoutInput_ReturnsNonZero(string command)
    {
        var args = new[] {command};
        var result = await Program.BuildAndInvokeRootCommand(args, _servicesMock.Object);
        result.Should().NotBe(0);
    }

    [Test]
    public async Task Build_WithValidInput_CallsCompiler()
    {
        using var fixture = new TestFileFixture(".vec", "space Test;");
        var args = new[] {"build", fixture.FilePath};
        _servicesMock.Setup(x => x.Compile(fixture.FilePath))
            .Callback((string _) => { });
        
        var result = await Program.BuildAndInvokeRootCommand(args, _servicesMock.Object);
        result.Should().Be(0);
        _servicesMock.Verify(x => x.Compile(fixture.FilePath), Times.Once);
    }
    
    [Test]
    public async Task Run_WithValidInput_CallsVirtualMachine()
    {
        using var fixture = new TestFileFixture(".vbc", "space Test;");
        var args = new[] {"run", fixture.FilePath};
        _servicesMock.Setup(x => x.RunVirtualMachine(fixture.FilePath))
            .Callback((string _) => { });
        
        var result = await Program.BuildAndInvokeRootCommand(args, _servicesMock.Object);
        result.Should().Be(0);
        _servicesMock.Verify(x => x.RunVirtualMachine(fixture.FilePath), Times.Once);
    }
}