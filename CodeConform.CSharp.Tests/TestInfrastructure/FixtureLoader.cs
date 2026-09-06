namespace CodeConform.CSharp.Tests.TestInfrastructure;

internal static class FixtureLoader
{
    public static string Load(params string[] pathParts)
    {
        var path = Path.Combine([AppContext.BaseDirectory, "Fixtures", .. pathParts]);

        return File.ReadAllText(path);
    }
}