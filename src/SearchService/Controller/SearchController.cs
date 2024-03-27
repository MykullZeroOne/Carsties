using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controller;

/// <summary>
///     This class handles the search functionality for items.
/// </summary>
[ApiController]
[Route ( "api/search" )]
public class SearchController : ControllerBase
    {
        /// <summary>
        ///     Searches for items based on a search term.
        /// </summary>
        /// <param name="searchParams"></param>
        /// <returns>A list of items matching the search term.</returns>
        [HttpGet]
        public async Task< ActionResult< List< Item > > > SearchItems ( [FromQuery] SearchParams searchParams )
            {
                // Code for search functionality including pagination 
                var query = DB.PagedSearch< Item, Item > ( );

                if ( ! string.IsNullOrEmpty ( searchParams.SearchTerm ) )
                    query.Match ( Search.Full, searchParams.SearchTerm ).SortByTextScore ( );

                query = searchParams.OrderBy switch
                        {
                                "make" => query.Sort ( x => x.Ascending ( a => a.Make ) ),
                                "new"  => query.Sort ( x => x.Descending ( a => a.CreatedAt ) ),
                                _      => query.Sort ( x => x.Ascending ( a => a.AuctionEnd ) )
                        };

                query = searchParams.FilterBy switch
                        {
                                "finished"   => query.Match ( x => x.AuctionEnd < DateTime.UtcNow ),
                                "endingSoon" => query.Match ( x => x.AuctionEnd < DateTime.UtcNow.AddHours ( 6 ) && x.AuctionEnd >= DateTime.UtcNow ),
                                _            => query.Match ( x => x.AuctionEnd > DateTime.UtcNow )
                        };

                if ( ! string.IsNullOrEmpty ( searchParams.Winner ) )
                    query.Match ( x => x.Winner == searchParams.Winner );

                if ( ! string.IsNullOrEmpty ( searchParams.Seller ) )
                    query.Match ( x => x.Seller == searchParams.Seller );

                query.PageNumber ( searchParams.PageNumber );
                query.PageSize ( searchParams.PageSize );

                var results = await query.ExecuteAsync ( );

                return Ok ( new
                        {
                                results    = results.Results,
                                pageCount  = results.PageCount,
                                totalCount = results.TotalCount
                        } );
            }
    }