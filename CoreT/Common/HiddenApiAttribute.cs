
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreT.Common
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public partial class HiddenApiAttribute : Attribute { }

    public class HiddenApiFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            foreach (ApiDescription apiDescription in context.ApiDescriptions.ToList())
            {
                if (apiDescription.CustomAttributes().OfType<HiddenApiAttribute>().Count() == 0
                    && apiDescription.ActionDescriptor.FilterDescriptors.OfType<HiddenApiAttribute>().Count() == 0) continue;

                var key = "/" + apiDescription.RelativePath.TrimEnd('/');
                if (!key.Contains("/test/") && swaggerDoc.Paths.ContainsKey(key))
                    swaggerDoc.Paths.Remove(key);
            }
        }

    }
}
