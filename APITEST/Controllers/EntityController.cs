using APITEST.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Nest;

namespace APITEST.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class EntityController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching",
            "Freezing2", "Bracing2", "Chilly2", "Cool2", "Mild2", "Warm2", "Balmy2", "Hot2", "Sweltering2", "Scorching2",
            "Freezing3", "Bracing3", "Chilly3", "Cool3", "Mild3", "Warm3", "Balmy3", "Hot3", "Sweltering3", "Scorching3",
            "Freezing4", "Bracing4", "Chilly4", "Cool4", "Mild4", "Warm4", "Balmy4", "Hot4", "Sweltering4", "Scorching4"
        };

        private readonly ILogger<EntityController> _logger;
        private readonly ElasticClient _ecclient;

        public EntityController(ILogger<EntityController> logger, ElasticClient ecclient)
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

            //var result = GetDataBooks(term, page, resultCount);
            var result = GetDataEmployees(term, page, resultCount);
            return new JsonResult(result);
        }

        private object GetDataBooks(string queryString, int page, int resultCount)
        {
            var offSet = (page - 1) * resultCount;

            ISearchResponse<Book> result;
            queryString = queryString?.ToLower();

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                // B�squeda en m�ltiples campos mediante cla�sulas Match y Field
                //results = _ecclient.Search<Book>(s => s
                //    .Query(q => q
                //    .Match(t => t
                //            .Field(f => f.Title).Field(f => f.Authors)
                //            .Query(query)
                //        )
                //    )                    
                //);

                // B�squeda en m�ltiples campos mediante cla�sulas Bool, Should y Term
                result = _ecclient.Search<Book>(s => s
                    .Query(q => q
                        .Bool(b => b
                            .Should(
                                bs => bs.Term(t => t.Title, queryString),
                                bs => bs.Term(t => t.Isbn, queryString),
                                bs => bs.Term(t => t.Authors, queryString),
                                bs => bs.Term(t => t.Categories, queryString)
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

                // Implementaci�n de Aggregations (agregados)
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

        private object GetDataEmployees(string queryString, int page, int resultCount)
        {
            var offSet = (page - 1) * resultCount;           
            queryString = $"*{ queryString?.ToLower() }*";

            var total = _ecclient.Search<Employee>(s => s
                    .Query(q => q
                        .Bool(b => b
                            .Should(
                                bs => bs.Wildcard(t => t.firstName, queryString),
                                bs => bs.Wildcard(t => t.lastName, queryString),
                                bs => bs.Wildcard(t => t.email, queryString)
                            )
                        )
                    )
             ).Total;

            var data = _ecclient.Search<Employee>(s => s
                 .Size(resultCount)
                 .Skip(offSet)
                 .Index("employees-index")
                  .Query(q => q
                        .Bool(b => b
                            .Should(
                                bs => bs.Wildcard(t => t.firstName, queryString),
                                bs => bs.Wildcard(t => t.lastName, queryString),
                                bs => bs.Wildcard(t => t.email, queryString)
                            )
                        )
                    )
             );

            //var total = _ecclient.Search<Employee>(s => s
            //        .Query(q => q
            //            .Bool(b => b
            //                .Should(
            //                    bs => bs.Term(t => t.firstName, queryString),
            //                    bs => bs.Term(t => t.lastName, queryString),
            //                    bs => bs.Term(t => t.email, queryString)
            //                )
            //            )
            //        )
            // ).Total;

            //var data = _ecclient.Search<Employee>(s => s
            //     .Size(resultCount)
            //     .Skip(offSet)
            //     .Index("employees-index")
            //      .Query(q => q
            //            .Bool(b => b
            //                .Should(
            //                    bs => bs.Term(t => t.firstName, queryString),
            //                    bs => bs.Term(t => t.lastName, queryString),
            //                    bs => bs.Term(t => t.email, queryString)
            //                )
            //            )
            //        )
            // );

            var endCount = offSet + resultCount;
            var more = endCount < total;
            var pagination = new { more = more };

            var results = (from c in data.Hits.ToList()
                           select new Item()
                           {
                               Id = c.Source.personID,
                               Text = c.Source.firstName + " " + c.Source.lastName
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
            if (bindingContext.ValueProvider.GetValue("term").FirstOrDefault() != null)
            {
                //if the parameter is not null. get the value.


                model = bindingContext.ValueProvider.GetValue("term").FirstOrDefault();
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
