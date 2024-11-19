namespace DocmostExporter.MkDocs.PostProcessors;

public class AdmotionsPostProcessor : IPostProcessor
{
    public string Process(string content)
    {
        var lastIndex = 0;

        while (true)
        {
            var nextOccurrence = content.IndexOf(":::", lastIndex, StringComparison.InvariantCultureIgnoreCase);
            
            if(nextOccurrence == -1)
                break;

            var end = content.IndexOf(":::", nextOccurrence + 3, StringComparison.InvariantCultureIgnoreCase);
            var nextLine = content.IndexOf("\n", nextOccurrence, StringComparison.InvariantCultureIgnoreCase);
            var level = content.Substring(nextOccurrence + 3, nextLine - nextOccurrence - 3).Trim();

            var contentStart = nextOccurrence + 3 + level.Length + 1;
            var contentEnd = end;
            var itemContent = content.Substring(contentStart, contentEnd - contentStart);

            //Console.WriteLine(itemContent);
            
            itemContent = itemContent.Replace("\n", "\n    ");

            var builtItem = $"!!! {level}\n\n    " + itemContent;
            var toReplace = content.Substring(nextOccurrence, end + 3 - nextOccurrence);
            
            content = content.Replace(toReplace, builtItem);
        }

        return content;
    }
}