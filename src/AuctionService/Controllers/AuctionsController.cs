using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

/// <summary>
///     Represents a controller for managing auctions.
/// </summary>
[ApiController]
[Route ( "api/auctions" )]
public class AuctionsController ( AuctionDbContext context, IMapper mapper ) : ControllerBase
    {
        // Returns Data in JSON Format

        /// <summary>
        ///     Gets all auctions from the database.
        /// </summary>
        /// <returns>A list of AuctionDto objects representing the auctions.</returns>
        [HttpGet]
        public async Task< ActionResult< List< AuctionDto > > > GetAllAuctions ( string date )
            {
                var query                                    = context.Auctions.OrderBy ( x => x.Item.Make ).AsQueryable ( );
                if ( ! string.IsNullOrEmpty ( date ) ) query = query.Where ( x => x.UpdatedAt.CompareTo ( DateTime.Parse ( date ).ToUniversalTime ( ) ) > 0 );

                return await query.ProjectTo< AuctionDto > ( mapper.ConfigurationProvider ).ToListAsync ( );
            }

        /// <summary>
        ///     Retrieves an auction by its ID.
        /// </summary>
        /// <param name="id">The ID of the auction</param>
        /// <returns>Returns an ActionResult containing the AuctionDto if found, or NotFound if not found</returns>
        /// <remarks>
        ///     Example:
        ///     GET /api/auctions/{id}
        /// </remarks>
        [HttpGet ( "{id:guid}" )]
        public async Task< ActionResult< AuctionDto > > GetAuctionById ( Guid id )
            {
                var auction = await context.Auctions.Include ( x => x.Item ).FirstOrDefaultAsync ( a => a.Id == id );

                if ( auction == null ) return NotFound ( );

                return mapper.Map< AuctionDto > ( auction );
            }

        /// <summary>
        ///     Create a new auction.
        /// </summary>
        /// <param name="auctionDto">The auction details.</param>
        /// <returns>The created auction.</returns>
        [HttpPost]
        public async Task< ActionResult< AuctionDto > > CreateAuction ( CreateAuctionDto auctionDto )
            {
                var auction = mapper.Map< Auction > ( auctionDto );
                auction.Seller = "Test";
                context.Auctions.Add ( auction );

                // TODO: add current user as seller

                var result = await context.SaveChangesAsync ( ) > 0;

                if ( ! result ) return BadRequest ( "Could not save changes to DB" );

                return CreatedAtAction ( nameof ( GetAuctionById ), new { auction.Id }, mapper.Map< AuctionDto > ( auction ) );
            }

        /// <summary>
        ///     Updates the specified auction with the provided data.
        /// </summary>
        /// <param name="id">The ID of the auction to update.</param>
        /// <param name="updateAuctionDto">The data to update the auction with.</param>
        /// <returns>Returns an IActionResult indicating the result of the update operation. Returns 200 (OK) if the update is successful, and 400 (Bad Request) otherwise.</returns>
        [HttpPut ( "{id:guid}" )]
        public async Task< ActionResult > UpdateAuction ( Guid id, UpdateAuctionDto updateAuctionDto )
            {
                var auction = await context.Auctions.Include ( x => x.Item ).FirstOrDefaultAsync ( x => x.Id == id );

                if ( auction == null ) return NotFound ( );

                // TODO: check Seller == UserName

                auction.Item.Make    = updateAuctionDto.Make    ?? auction.Item.Make;
                auction.Item.Model   = updateAuctionDto.Model   ?? auction.Item.Model;
                auction.Item.Color   = updateAuctionDto.Color   ?? auction.Item.Color;
                auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
                auction.Item.Year    = updateAuctionDto.Year    ?? auction.Item.Year;

                var result = await context.SaveChangesAsync ( ) > 0;

                if ( result ) return Ok ( );

                return BadRequest ( );
            }

        /// Deletes an auction from the database.
        /// @param id The unique identifier of the auction to delete.
        /// @returns An ActionResult indicating the result of the operation.
        /// - If the auction is successfully deleted, returns an Ok result.
        /// - If the auction is not found, returns a NotFound result.
        /// - If the operation fails, returns a BadRequest result with an error message.
        /// /
        [HttpDelete ( "{id:guid}" )]
        public async Task< ActionResult > DeleteAuction ( Guid id )
            {
                var auction = await context.Auctions.FindAsync ( id );

                if ( auction == null ) return NotFound ( );

                // TODO: check Seller == UserName

                context.Auctions.Remove ( auction );

                var result = await context.SaveChangesAsync ( ) > 0;

                if ( ! result ) return BadRequest ( "Could not update DB" );

                return Ok ( );
            }
    }