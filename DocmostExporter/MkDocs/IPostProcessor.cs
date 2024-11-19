namespace DocmostExporter.MkDocs;

public interface IPostProcessor
{
    public string Process(string content);
}