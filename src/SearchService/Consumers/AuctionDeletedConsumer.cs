using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

//MassTransit is Convention based and expects to see Consumer in the name of the class
/// to handle message queuing and routing.
public class AuctionDeletedConsumer ( IMapper mapper ) : IConsumer< AuctionDeleted >
    {
        /// <summary>
        ///     Consumes an instance of AuctionDeleted and performs necessary operations.
        /// </summary>
        /// <param name="context">The context containing the AuctionDeleted message.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public async Task Consume ( ConsumeContext< AuctionDeleted > context )
            {
                Console.WriteLine ( "--> Consuming auction deleted: " + context.Message.Id );

                var result = await DB.DeleteAsync< Item > ( context.Message.Id );
                if ( ! result.IsAcknowledged ) throw new MessageException ( typeof ( AuctionDeleted ), "Problem deleting auction" );
            }
    }