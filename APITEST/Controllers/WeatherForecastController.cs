using APITEST.Controllers;
using APITEST.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Nest;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace APITEST.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
            "Freezing2", "Bracing2", "Chilly2", "Cool2", "Mild2", "Warm2", "Balmy2", "Hot2", "Sweltering2", "Scorching2",
            "Freezing3", "Bracing3", "Chilly3", "Cool3", "Mild3", "Warm3", "Balmy3", "Hot3", "Sweltering3", "Scorching3",
            "Freezing4", "Bracing4", "Chilly4", "Cool4", "Mild4", "Warm4", "Balmy4", "Hot4", "Sweltering4", "Scorching4"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ElasticClient _ecclient;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, ElasticClient ecclient)
        {
            _logger = logger;
            _ecclient = ecclient;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<JsonResult> Get([ModelBinder(BinderType = typeof(CustomModelBinder))] string term, int page)
        {
            var resultCount = 7;
            //var offSet = (page - 1) * resultCount;

            //var data = Enumerable.Range(0, Summaries.Length - 1).Select(index => new WeatherForecast
            //{
            //    Id = Random.Shared.Next(1, 5000),
            //    Text = Summaries[index]
            //})
            //.ToArray();

            //var filterData = data.Where(x => x.Text.Contains(term)).ToArray();

            //var results = filterData.Skip(offSet).Take(resultCount);

            //var endCount = offSet + resultCount;
            //var more = endCount < filterData.Length;
            //var pagination = new { more = more };

            //object result = new
            //{
            //    results,
            //    pagination
            //};

            //return new JsonResult(result);

            var result = GetData(term, page, resultCount);
            return new JsonResult(result);
        }

        private object GetData(string query, int page, int resultCount)
        {
            var offSet = (page - 1) * resultCount;

            ISearchResponse<Book> result;
            query = query?.ToLower();

            if (!string.IsNullOrWhiteSpace(query))
            {
                // Búsqueda en múltiples campos mediante claúsulas Match y Field
                //results = _ecclient.Search<Book>(s => s
                //    .Query(q => q
                //    .Match(t => t
                //            .Field(f => f.Title).Field(f => f.Authors)
                //            .Query(query)
                //        )
                //    )                    
                //);

                // Búsqueda en múltiples campos mediante claúsulas Bool, Should y Term
                result = _ecclient.Search<Book>(s => s
                    .Query(q => q
                        .Bool(b => b
                            .Should(
                                bs => bs.Term(t => t.Title, query),
                                bs => bs.Term(t => t.Isbn, query),
                                bs => bs.Term(t => t.Authors, query),
                                bs => bs.Term(t => t.Categories, query)
                            )
                        )
                    )
                );
            }
            else
            {
                result = _ecclient.Search<Book>(s => s
                 .Query(q => q
                    .MatchAll()
                 )
                 .Size(25) // Indicar cantidad de registros devueltos, por defecto se limita a 10
                );

                // Implementación de Aggregations (agregados)
                result = _ecclient.Search<Book>(s => s
                    .Query(q => q
                        .MatchAll()
                    )
                    .Aggregations(a => a
                        .Terms("categories", t => t
                            .Field("categories.keyword")
                        )
                    )
                );
            }

            var data = result.Hits.Skip(offSet).Take(resultCount);
            var endCount = offSet + resultCount;
            var more = endCount < result.Hits.Count;
            var pagination = new { more = more };

            var results = (from c in data.ToList()
                           select new Item()
                           {
                               Id = c.Source.Isbn,
                               Text = c.Source.Title
                           }).ToArray();

            object finalResult = new
            {
                results,
                pagination
            };

            return finalResult;
        }
    }

    public class Item
    {
        public string Id { get; set; }
        public string Text { get; set; }
    }

    //required reference: using Microsoft.AspNetCore.Mvc.ModelBinding;  
    // using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;  
    public class CustomModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var model = string.Empty;
            if (bindingContext.ValueProvider.GetValue("name").FirstOrDefault() != null)
            {
                //if the parameter is not null. get the value.


                model = bindingContext.ValueProvider.GetValue("name").FirstOrDefault();
            }
            else
            {
                //set the default value.  
                model = "";
            }

            bindingContext.Result = ModelBindingResult.Success(model);
            return Task.CompletedTask;
        }
    }

    public class MyCustomBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // specify the parameter your binder operates on  
            if (context.Metadata.ParameterName == "param2")
            {
                return new BinderTypeModelBinder(typeof(CustomModelBinder));
            }

            return null;
        }
    }
}
