namespace KnowledgeExtractionTool.Extensions;

using System;
using System.Collections.Generic;

public static class PriorityQueueExtensions {
    public static void RemoveWhere<TElement, TPriority>(this PriorityQueue<TElement, TPriority> queue, Func<TElement, bool> predicate) where TPriority : IComparable<TPriority> {
        var elements = new List<(TElement element, TPriority priority)>();

        while (queue.Count > 0) {
            queue.TryDequeue(out TElement val, out TPriority priorityval);
            elements.Add((val, priorityval));
        }

        foreach (var (element, priority) in elements) {
            if (!predicate(element))
                queue.Enqueue(element, priority);
        }
    }
}
