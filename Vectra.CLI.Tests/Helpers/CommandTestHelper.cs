
namespace Vectra.CLI.Tests.Helpers;

public class TestFileFixture : IDisposable
{
    public string FilePath { get; }

    public TestFileFixture(string extension, string content = "")
    {
        FilePath = Path.GetTempFileName();
        File.Move(FilePath, Path.ChangeExtension(FilePath, extension));
        FilePath = Path.ChangeExtension(FilePath, extension);

        if (!string.IsNullOrEmpty(content))
        {
            File.WriteAllText(FilePath, content);
        }
    }

    public void Dispose()
    {
        if (File.Exists(FilePath))
        {
            File.Delete(FilePath);
        }
    }
}