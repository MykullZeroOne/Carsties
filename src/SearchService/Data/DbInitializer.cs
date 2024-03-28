using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

/// <summary>
///     Initializes the database for the SearchService application.
/// </summary>
public class DbInitializer
    {
        /// <summary>
        ///     Initializes the database.
        /// </summary>
        /// <param name="app">The <see cref="WebApplication" /> instance.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task InitDb ( WebApplication app )
            {
                await DB.InitAsync ( "SearchDb", MongoClientSettings.FromConnectionString ( app.Configuration.GetConnectionString ( "MongoDbConnection" ) ) );

                await DB.Index< Item > ( ).Key ( x => x.Make, KeyType.Text ).Key ( x => x.Model, KeyType.Text ).Key ( x => x.Year, KeyType.Text ).CreateAsync ( );

                var count = await DB.CountAsync< Item > ( );

                using var scope  = app.Services.CreateScope ( );
                var       client = scope.ServiceProvider.GetRequiredService< AuctionServiceHttpClient > ( );
                var       items  = await client.GetItemsForSearchDb ( );
                Console.WriteLine ( items.Count + " - returned from the auction Service" );
                if ( items.Count > 0 ) await DB.SaveAsync ( items );
            }
    }