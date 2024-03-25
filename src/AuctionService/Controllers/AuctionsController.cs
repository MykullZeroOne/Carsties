using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ ApiController ]
[ Route ( "api/auctions" ) ]
public class AuctionsController ( AuctionDbContext context, IMapper mapper ) : ControllerBase
    {
    // Returns Data in JSON Format

    [ HttpGet ]
    public async Task < ActionResult < List < AuctionDto > > > GetAllAuctions ( )
        {
            var auctions = await context.Auctions.Include ( x => x.Item ).OrderBy ( o => o.Item.Make ).ToListAsync ( );

            return mapper.Map < List < AuctionDto > > ( auctions );
        }

    [ HttpGet ( "{id:guid}" ) ]
    public async Task < ActionResult < AuctionDto > > GetAuctionById ( Guid id )
        {
            var auction = await context.Auctions.Include ( x => x.Item ).FirstOrDefaultAsync ( a => a.Id == id );

            if ( auction == null ) return NotFound ( );

            return mapper.Map < AuctionDto > ( auction );
        }

    [ HttpPost ]
    public async Task < ActionResult < AuctionDto > > CreateAuction ( CreateAuctionDto auctionDto )
        {
            var auction = mapper.Map < Auction > ( auctionDto );
            auction.Seller = "Test";
            context.Auctions.Add ( auction );

            // TODO: add current user as seller

            var result = await context.SaveChangesAsync ( ) > 0;

            if ( !result ) return BadRequest ( "Could not save changes to DB" );

            return CreatedAtAction
                ( nameof ( GetAuctionById ), new { auction.Id }, mapper.Map < AuctionDto > ( auction ) );
        }

    [ HttpPut ( "{id:guid}" ) ]
    public async Task < ActionResult > UpdateAuction ( Guid id, UpdateAuctionDto updateAuctionDto )
        {
            var auction = await context.Auctions.Include ( x => x.Item ).FirstOrDefaultAsync ( x => x.Id == id );

            if ( auction == null ) return NotFound ( );

            // TODO: check Seller == UserName

            auction.Item.Make    = updateAuctionDto.Make    ?? auction.Item.Make;
            auction.Item.Model   = updateAuctionDto.Model   ?? auction.Item.Model;
            auction.Item.Color   = updateAuctionDto.Color   ?? auction.Item.Color;cd
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year    = updateAuctionDto.Year    ?? auction.Item.Year;

            var result = await context.SaveChangesAsync ( ) > 0;

            if ( result ) return Ok ( );

            return BadRequest ( );
        }

    [ HttpDelete ( "{id:guid}" ) ]
    public async Task < ActionResult > DeleteAuction ( Guid id )
        {
            var auction = await context.Auctions.FindAsync ( id );

            if ( auction == null ) return NotFound ( );

            // TODO: check Seller == UserName

            context.Auctions.Remove ( auction );

            var result = await context.SaveChangesAsync ( ) > 0;

            if ( !result ) return BadRequest ( "Could not update DB" );

            return Ok ( );
        }
    }