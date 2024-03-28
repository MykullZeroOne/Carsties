using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

//MassTransit is Convention based and expects to see Consumer in the name of the class
public class AuctionCreatedConsumer ( IMapper mapper ) : IConsumer< AuctionCreated >
    {
        public async Task Consume ( ConsumeContext< AuctionCreated > context )
            {
                Console.WriteLine ( "--> Consuming auction Created: " + context.Message.Id );

                var item = mapper.Map< Item > ( context.Message );

                if ( item.Model.Equals ( "Foo" ) ) throw new ArgumentException ( "Can not sell cars with name of Foo" );

                await item.SaveAsync ( );
            }
    }