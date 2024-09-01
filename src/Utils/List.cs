namespace KnowledgeExtractionTool.Utils;

public static class ListExtensions {
    public static List<TResult> Map<TSource, TResult>(this List<TSource> source, Func<TSource, TResult> selector) {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (selector == null) throw new ArgumentNullException(nameof(selector));

        List<TResult> result = new List<TResult>(source.Count);

        foreach (var item in source) {
            result.Add(selector(item));
        }

        return result;
    }
}