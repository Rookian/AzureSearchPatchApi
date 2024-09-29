using System.Dynamic;
using System.Linq.Expressions;
using Azure;
using Azure.Search.Documents;

namespace AzureSearchPatchApi
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            var searchClient = new SearchClient(new Uri("https://patch-api-search.search.windows.net"), "index", new AzureKeyCredential("xxx"));

            //var documents = new List<Document> { new() { Id = "123", Description = "Desc", Name = "Name" } };
            //await searchClient.MergeOrUploadDocumentsAsync(documents, new IndexDocumentsOptions { ThrowOnAnyError = true });


            var x = PatchApi.PatchMultiple<Document>("123",
                (x => x.Description, "Hello world 123"), (x => x.Name, "New Name"));
            var partialDocument = new[] { x };
            await searchClient.MergeOrUploadDocumentsAsync(partialDocument, new IndexDocumentsOptions { ThrowOnAnyError = true });
        }
    }

    public class Document
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public static class PatchApi
    {
        public static object PatchMultiple<T>(string documentId, params (Expression<Func<T, object>> propertyExpression, object value)[] updates) where T : class
        {
            var updateObject = new ExpandoObject() as IDictionary<string, object>;
            updateObject["id"] = documentId;

            foreach (var (propertyExpression, value) in updates)
            {
                var propertyName = GetPropertyName(propertyExpression);
                updateObject[propertyName] = value;
            }

            return updateObject;
        }

        private static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> propertyExpression)
        {
            if (propertyExpression.Body is MemberExpression memberExpression)
            {
                return memberExpression.Member.Name;
            }

            throw new InvalidOperationException("Expected a MemberExpression.");
        }
    }
}