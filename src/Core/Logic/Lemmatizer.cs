namespace KnowledgeExtractionTool.Core.Logic;

// using edu.stanford.nlp.ling;
// using edu.stanford.nlp.pipeline;
// using edu.stanford.nlp.util;
// using java.util;

// public class Lemmatizer
// {
//     private static StanfordCoreNLP pipeline;

//     static Lemmatizer()
//     {
//         // Set up the pipeline
//         var props = new Properties();
//         props.setProperty("annotators", "tokenize, ssplit, pos, lemma");
//         pipeline = new StanfordCoreNLP(props);
//     }

//     public static string Lemmatize(string text)
//     {
//         // Create an empty Annotation just with the given text
//         var document = new Annotation(text);

//         // Run all Annotators on this text
//         pipeline.annotate(document);

//         // Get the list of lemmas
//         var lemmas = new List<string>();
//         foreach (var sentence in document.get(typeof(CoreAnnotations.SentencesAnnotation)) as List<CoreMap>)
//         {
//             foreach (var token in sentence.get(typeof(CoreAnnotations.TokensAnnotation)) as List<CoreLabel>)
//             {
//                 lemmas.Add(token.get(typeof(CoreAnnotations.LemmaAnnotation)) as string);
//             }
//         }

//         return string.Join(" ", lemmas);
//     }
// }