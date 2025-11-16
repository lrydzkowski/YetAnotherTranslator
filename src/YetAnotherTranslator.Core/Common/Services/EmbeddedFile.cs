using System.Reflection;

namespace YetAnotherTranslator.Core.Common.Services;

public interface IEmbeddedFile
{
    string GetContent(string path, Assembly? assembly = null);
    byte[] Get(string path, Assembly? assembly = null);
}

internal class EmbeddedFile : IEmbeddedFile
{
    public string GetContent(string path, Assembly? assembly = null)
    {
        assembly ??= typeof(EmbeddedFile).Assembly;
        path = ParsePath(path, assembly);

        using Stream? stream = assembly.GetManifestResourceStream(path);
        using StreamReader reader = new(stream!);
        string content = reader.ReadToEnd();

        return content;
    }

    public byte[] Get(string path, Assembly? assembly = null)
    {
        assembly ??= typeof(EmbeddedFile).Assembly;
        path = ParsePath(path, assembly);

        using Stream? stream = assembly.GetManifestResourceStream(path);
        MemoryStream memoryStream = new();
        stream?.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }

    private string ParsePath(string path, Assembly assembly)
    {
        path = TransformPath(path, assembly);
        if (!Exists(path, assembly))
        {
            throw new InvalidOperationException($"Embedded file doesn't exist: '{path}'.");
        }

        return path;
    }

    private string TransformPath(string path, Assembly assembly)
    {
        string transformedPath = path.Replace('/', '.').Replace('\\', '.');

        return $"{assembly.GetName().Name}.{transformedPath}";
    }

    private bool Exists(string path, Assembly assembly)
    {
        return assembly.GetManifestResourceNames().Any(x => x == path);
    }
}
