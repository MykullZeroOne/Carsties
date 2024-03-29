using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder ( args );

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer ( );

builder.Services.AddControllers ( );

builder.Services.AddDbContext< AuctionDbContext > ( opt => { opt.UseNpgsql ( builder.Configuration.GetConnectionString ( "DefaultConnection" ) ); } );

builder.Services.AddAutoMapper ( AppDomain.CurrentDomain.GetAssemblies ( ) ); // Adds Automapper for Dependency Injection

// adds MassTransit to RabbitMg Service Bus
builder.Services.AddMassTransit ( x =>
    {
        /***********************************************************************************
         *                                                                                  *
         * This code configures EntityFramework's outbox feature with a few specifics:      *
         *                                                                                  *
         *    1. It designates `AuctionDbContext` as the Dbcontext to be used by the outbox *
         *                                                                                  *
         *    2. It sets the query delay of this outbox to 10 seconds. This means that      *
         *       if the application is stopping, any remaining outbox messages will be      *
         *       delivered within this delay interval.                                      *
         *                                                                                  *
         *    3. It sets the outbox to use a PostgreSQL database.                           *
         *                                                                                  *
         *    4. Lastly, it enables the use of outbox in the service bus. Using a bus       *
         *       outbox ensures consistency between the database state and the published    *
         *       messages.                                                                  *
         *                                                                                  *
         ***********************************************************************************/
        x.AddEntityFrameworkOutbox< AuctionDbContext > ( opt =>
            {
                opt.QueryDelay = TimeSpan.FromSeconds ( 10 );
                opt.UsePostgres ( );
                opt.UseBusOutbox ( );
            } );
        x.AddConsumersFromNamespaceContaining< AuctionsCreatedFaultConsumer > ( );
        x.SetEndpointNameFormatter ( new KebabCaseEndpointNameFormatter ( "auction", false ) );
        x.UsingRabbitMq ( ( context, cfg ) => { cfg.ConfigureEndpoints ( context ); } );
    } );
builder.Services.AddAuthentication ( JwtBearerDefaults.AuthenticationScheme ).AddJwtBearer ( option =>
    {
        option.Authority                                  = builder.Configuration[ "IdentityServiceUrl" ];
        option.RequireHttpsMetadata                       = false;
        option.TokenValidationParameters.ValidateAudience = false;
        option.TokenValidationParameters.NameClaimType    = "userName";
    } );
var app = builder.Build ( );

// Configure the HTTP request pipeline.
app.UseAuthentication ( );
app.UseAuthorization ( );

app.MapControllers ( );

try
    {
        DbInitializer.InitDb ( app );
    }
catch ( Exception e )
    {
        Console.WriteLine ( e );
    }

app.Run ( );