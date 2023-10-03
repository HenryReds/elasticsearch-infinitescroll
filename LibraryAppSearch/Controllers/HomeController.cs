using LibraryAppSearch.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryAppSearch.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ElasticClient _ecclient;

        public HomeController(ILogger<HomeController> logger, ElasticClient elasticClient)
        {
            _logger = logger;
            _ecclient = elasticClient;
        }

        public IActionResult Index(string query)
        {
            ISearchResponse<Book> results = new SearchResponse<Book>();
            //query = query?.ToLower();

            //if (!string.IsNullOrWhiteSpace(query))
            //{
            //    // Búsqueda en múltiples campos mediante claúsulas Match y Field
            //    //results = _ecclient.Search<Book>(s => s
            //    //    .Query(q => q
            //    //    .Match(t => t
            //    //            .Field(f => f.Title).Field(f => f.Authors)
            //    //            .Query(query)
            //    //        )
            //    //    )                    
            //    //);

            //    // Búsqueda en múltiples campos mediante claúsulas Bool, Should y Term
            //    results = _ecclient.Search<Book>(s => s
            //        .Query(q => q
            //            .Bool(b => b
            //                .Should(
            //                    bs => bs.Term(t => t.Title, query),
            //                    bs => bs.Term(t => t.Isbn, query),
            //                    bs => bs.Term(t => t.Authors, query),
            //                    bs => bs.Term(t => t.Categories, query)
            //                )
            //            )
            //        )
            //    );
            //}
            //else
            //{
            //    results = _ecclient.Search<Book>(s => s
            //     .Query(q => q
            //        .MatchAll()
            //     )
            //     .Size(25) // Indicar cantidad de registros devueltos, por defecto se limita a 10
            //    );

            //    // Implementación de Aggregations (agregados)
            //    results = _ecclient.Search<Book>(s => s
            //        .Query(q => q
            //            .MatchAll()
            //        )
            //        .Aggregations(a => a
            //            .Terms("categories", t => t
            //                .Field("categories.keyword")
            //            )
            //        )
            //    );
            //}

            return View(results);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
