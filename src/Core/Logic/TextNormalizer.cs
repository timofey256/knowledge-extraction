namespace KnowledgeExtractionTool.Core.Logic;

/// <summary>
/// Removes punctuation, stop words and converts text to lowercase.
/// </summary>
public static class TextNormalizer {

    private static readonly string[] _stopWords = {
        "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't", "as",
        "at", "be", "because", "been", "before", "being", "below", "between", "both", "but", "by", "can", "could",
        "d", "did", "didn't", "do", "does", "doesn't", "doing", "don", "down", "during", "each", "few", "for", "from",
        "further", "had", "hadn't", "has", "hasn't", "have", "haven't", "having", "he", "her", "here", "hers", "herself",
        "him", "himself", "his", "how", "i", "if", "in", "into", "is", "isn't", "it", "its", "itself", "just", "ll",
        "m", "ma", "me", "might", "more", "most", "must", "my", "myself", "need", "no", "nor", "not", "now", "o", "of",
        "off", "on", "once", "only", "or", "other", "our", "ours", "ourselves", "out", "over", "own", "re", "s", "same",
        "shan't", "she", "she's", "should", "shouldn't", "so", "some", "such", "t", "than", "that", "the", "their",
        "theirs", "them", "themselves", "then", "there", "these", "they", "this", "those", "through", "to", "too", "under",
        "until", "up", "ve", "very", "was", "wasn't", "we", "were", "weren't", "what", "when", "where", "which", "while",
        "who", "whom", "why", "will", "with", "won't", "would", "y", "you", "your", "yours", "yourself", "yourselves"
    };

    private static readonly string[] _punctuation = { ".", ",", ":", ";", "!", "?" };

    public static string Process(string text) { 
        var filteredWords = text
                           .ToLower()
                           .Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                           .Where(word => !_stopWords.Contains(word.ToLower()))
                           .Where(word => !_punctuation.Contains(word.ToLower())).ToArray();
        
        return string.Join(" ", filteredWords);
    }
}