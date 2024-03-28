using AuctionService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

/// <summary>
///     Represents the DbContext for the AuctionService.
/// </summary>
public class AuctionDbContext : DbContext
    {
        /// ModelCreating method is overridden to configure the database entities.
        public AuctionDbContext ( DbContextOptions options ) : base ( options ) { }

        /// <summary>
        ///     Represents a database context for managing auctions.
        /// </summary>
        public DbSet< Auction > Auctions { get; set; }


        /// <summary>
        ///     Configures the model that is used to define the database schema, relationships, and constraints.
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model for the database context being created.</param>
        protected override void OnModelCreating ( ModelBuilder modelBuilder )
            {
                base.OnModelCreating ( modelBuilder );
                modelBuilder.AddInboxStateEntity ( );
                modelBuilder.AddOutboxMessageEntity ( );
                modelBuilder.AddOutboxStateEntity ( );
            }
    }