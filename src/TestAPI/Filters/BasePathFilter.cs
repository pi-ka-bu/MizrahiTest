using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TestAPI.Filters
{
    public class BasePathFilter : IDocumentFilter
    {
        public BasePathFilter(string basePath)
        {
            BasePath = basePath;
        }

        public string BasePath { get; }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Servers.Add(new OpenApiServer() { Url = this.BasePath });

            var pathsToModify = swaggerDoc
                .Paths.Where(p => p.Key.StartsWith(this.BasePath))
                .ToList();

            foreach (var path in pathsToModify)
            {
                if (path.Key.StartsWith(this.BasePath))
                {
                    string newKey = Regex.Replace(path.Key, $"^{this.BasePath}", string.Empty);
                    swaggerDoc.Paths.Remove(path.Key);
                    swaggerDoc.Paths.Add(newKey, path.Value);
                }
            }
        }
    }
}
