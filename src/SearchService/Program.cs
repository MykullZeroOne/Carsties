// Imports namespaces needed for this code
// System.Net carries types for network/Internet communication
// Polly is a .NET resilience and transient-fault-handling library
// Polly.Extensions.Http provides extension methods for handling HttpClient failures
// SearchService.Data and SearchService.Services are project-specific namespaces

using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

// The WebApplication.CreateBuilder method creates a WebApplication builder 
// with pre-configured defaults, and application arguments are passed as input.
var builder = WebApplication.CreateBuilder ( args );

// This line adds the controllers service to the app's service container.
builder.Services.AddControllers ( );

// Allows exploring of endpoints in the API.
builder.Services.AddEndpointsApiExplorer ( );
builder.Services.AddAutoMapper ( AppDomain.CurrentDomain.GetAssemblies ( ) );
// Adds the AuctionServiceHttpClient service to the HttpClient factory
// and configures a policy for it using GetPolicy function.
builder.Services.AddHttpClient< AuctionServiceHttpClient > ( ).AddPolicyHandler ( GetPolicy ( ) );


/***************************************************************************************
 *                                                                                     *
 *    The configuration block for RabbitMq using the MassTransit library is defined    *
 *    in this section.                                                                 *
 *                                                                                     *
 *    The system is configured to process messages from the RabbitMq queue titled      *
 *    "search-auction-created" as soon as they are encountered.                        *
 *                                                                                     *
 *    Specifically, the AuctionCreatedConsumer is utilized for this process.           *
 *                                                                                     *
 *    If message consumption fails, a retry logic is triggered, in this scenario,      *
 *    the set interval is 5 times every 5ms.                                           *
 *                                                                                     *
 *    Finally, as per the prescribed application context, the RabbitMq endpoints       *
 *    are properly configured.                                                         *
 *                                                                                     *
 ***************************************************************************************/
builder.Services.AddMassTransit ( x =>
    {
        /********************************************************************************************
         *                                                                                          *
         *  The provided code block is a part of the application configuration setup related to     *
         *  message serving using the RabbitMQ service via the MassTransit library.                 *
         *                                                                                          *
         *  In context of the RabbitMQ setup, the code is dedicated to setting up a "listening"     *
         *  station or Receive Endpoint, specifically targeting a queue identified as               *
         *  "search-auction-created".                                                               *
         *                                                                                          *
         *  Any message arriving at the named queue is handed over to a handler known as            *
         *  AuctionCreatedConsumer, which is expected to process or "consume" the message           *
         *  following application-specific logic.                                                   *
         *                                                                                          *
         *  For reliability and resilience, the code block specifies a retry mechanism in case      *
         *  of any failure during the processing of the message. In this case, retries occur        *
         *  after an interval set to trigger 5 times every 5 milliseconds.                          *
         *                                                                                          *
         *  Lastly, the configuration of all endpoints associated with the service bus is ensured   *
         *  to mirror the broader application context.                                              *
         *                                                                                          *
         ********************************************************************************************/
        x.UsingRabbitMq ( ( context, cfg ) =>
            {
                cfg.ReceiveEndpoint ( "search-auction-created", e =>
                    {
                        e.UseMessageRetry ( r => r.Interval ( 5, 5 ) );
                        e.ConfigureConsumer< AuctionCreatedConsumer > ( context );
                    } );

                cfg.ReceiveEndpoint ( "search-auction-updated", e =>
                    {
                        e.UseMessageRetry ( r => r.Interval ( 5, 5 ) );
                        e.ConfigureConsumer< AuctionUpdatedConsumer > ( context );
                    } );

                cfg.ReceiveEndpoint ( "search-auction-deleted", e =>
                    {
                        e.UseMessageRetry ( r => r.Interval ( 5, 5 ) );
                        e.ConfigureConsumer< AuctionDeletedConsumer > ( context );
                    } );
                cfg.ConfigureEndpoints ( context );
            } );
        /****************************************************************************************
         *                                                                                      *
         *   This code block is part of the RabbitMQ setup using the MassTransit library.       *
         *                                                                                      *
         *   Specifically, it is configuring to use the 'AuctionCreatedConsumer' for            *
         *   consuming messages. This occurs when a message arrives at a named RabbitMQ         *
         *   queue identified during the service configuration.                                 *
         *                                                                                      *
         *   Any message observed at this queue is processed by the 'AuctionCreatedConsumer'.   *
         *                                                                                      *
         *   This is integral to the real-time processing capabilities of the system,           *
         *   enabling the application to respond promptly to respective events triggered        *
         *   within the given context of the application.                                       *
         *                                                                                      *
         ****************************************************************************************/
        x.AddConsumersFromNamespaceContaining< AuctionCreatedConsumer > ( );
        x.AddConsumersFromNamespaceContaining< AuctionUpdatedConsumer > ( );
        x.AddConsumersFromNamespaceContaining< AuctionDeletedConsumer > ( );

        /**********************************************************************
         *                                                                    *
         * Here, the application is setting a naming convention for the       *
         * RabbitMQ endpoints. This is achieved by using the                  *
         * KebabCaseEndpointNameFormatter from the MassTransit library.       *
         *                                                                    *
         * This formatter allows the configuration of the endpoint names      *
         * to use kebab-case, which is a common naming convention that        *
         * involves writing compound words or phrases in lower case with      *
         * hyphens between words. This formatter is set to use the prefix     *
         * "search" for endpoint names.                                       *
         *                                                                    *
         * This is part of the overall Message Bus configuration, which       *
         * provides scaling capabilities to the system by organizing          *
         * communication between services.                                    *
         *                                                                    *
        // ********************************************************************/
        x.SetEndpointNameFormatter ( new KebabCaseEndpointNameFormatter ( "search", false ) );
    } );

// The Build method creates a WebApplication object which bootstraps the app.
var app = builder.Build ( );

// This line maps controller actions to routes.
app.MapControllers ( );

// Registers the callback function to run when the application starts
app.Lifetime.ApplicationStarted.Register ( Callback );

// Tries to initialize the database when the application starts
try
    {
        await DbInitializer.InitDb ( app );
    }
catch ( Exception e )
    {
        Console.WriteLine ( e );
    }

// The Run method runs the application and begins listening for incoming HTTP requests
app.Run ( );
return;

// Defines a callback function for when the application starts
async void Callback ( )
    {
        try
            {
                await DbInitializer.InitDb ( app );
            }
        catch ( Exception e )
            {
                Console.WriteLine ( e );
            }
    }

// This function returns a policy that handles the transient errors
// and reruns the action when response status code is NotFound, 
// or a transient error occurred while sending the request 
// It waits for 3 seconds before making the next attempt.
static IAsyncPolicy< HttpResponseMessage > GetPolicy ( )
    {
        return HttpPolicyExtensions.HandleTransientHttpError ( ).OrResult ( msg => msg.StatusCode == HttpStatusCode.NotFound ).WaitAndRetryForeverAsync ( _ => TimeSpan.FromSeconds ( 3 ) );
    }