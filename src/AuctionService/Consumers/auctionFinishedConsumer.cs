using AuctionService.Data;
using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers;

public class auctionFinishedConsumer ( AuctionDbContext context ) : IConsumer< AuctionFinished >

    {
        public async Task Consume ( ConsumeContext< AuctionFinished > consumeContext )
            {
                Console.WriteLine ( "--> Consuming Auction Finished!" );
                var auction = await context.Auctions.FindAsync ( consumeContext.Message.AuctionId );
                if ( consumeContext.Message.ItemSold )
                    {
                        auction.Winner     = consumeContext.Message.Winner;
                        auction.SoldAmount = consumeContext.Message.Amount;
                    }

                auction.Status = auction.ReservePrice > auction.ReservePrice ? Status.Finished : Status.ReserveNotMet;
                await context.SaveChangesAsync ( );
            }
    }