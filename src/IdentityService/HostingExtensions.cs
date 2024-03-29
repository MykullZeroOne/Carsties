using IdentityService.Data;
using IdentityService.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

/// <summary>
///     Contains extension methods for configuring the hosting environment of an ASP.NET Core application.
/// </summary>
internal static class HostingExtensions
    {
        /// <summary>
        ///     Configures services for the ASP.NET Core application.
        /// </summary>
        /// <param name="builder">The <see cref="WebApplicationBuilder" /> used to configure services.</param>
        /// <returns>The configured <see cref="WebApplication" />.</returns>
        public static WebApplication ConfigureServices ( this WebApplicationBuilder builder )
            {
                builder.Services.AddRazorPages ( );

                builder.Services.AddDbContext< ApplicationDbContext > ( options => options.UseNpgsql ( builder.Configuration.GetConnectionString ( "DefaultConnection" ) ) );

                /**********************************************************************************
                 *                                                                                *
                 * This block of code is configuring Identity services for the ASP.NET Core       *
                 * application. It registers 'ApplicationUser' and 'IdentityRole' with Identity   *
                 * system.                                                                        *
                 *                                                                                *
                 * 'AddIdentity<ApplicationUser, IdentityRole>()' adds the default identity       *
                 * system configuration for the specified user and role types.                    *
                 *                                                                                *
                 * '.AddEntityFrameworkStores<ApplicationDbContext>()' registers the DB Context   *
                 * used by the Identity system which is 'ApplicationDbContext' in this case.      *
                 *                                                                                *
                 * '.AddDefaultTokenProviders()' method adds the default token providers used to  *
                 * generate tokens for reset passwords, change email and change UserName          *
                 * operations.                                                                    *
                 *                                                                                *
                 **********************************************************************************/
                builder.Services.AddIdentity< ApplicationUser, IdentityRole > ( ).AddEntityFrameworkStores< ApplicationDbContext > ( ).AddDefaultTokenProviders ( );

                builder.Services.AddIdentityServer ( options =>
                           {
                               options.Events.RaiseErrorEvents       = true;
                               options.Events.RaiseInformationEvents = true;
                               options.Events.RaiseFailureEvents     = true;
                               options.Events.RaiseSuccessEvents     = true;

                               // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                               // options.EmitStaticAudienceClaim = true;
                           } ).AddInMemoryIdentityResources ( Config.IdentityResources ).AddInMemoryApiScopes ( Config.ApiScopes ).AddInMemoryClients ( Config.Clients ).AddAspNetIdentity< ApplicationUser > ( )
                       .AddProfileService< CustomProfileService > ( );

                builder.Services.ConfigureApplicationCookie ( options => { options.Cookie.SameSite = SameSiteMode.Lax; } );
                builder.Services.AddAuthentication ( );

                return builder.Build ( );
            }

        /// <summary>
        ///     Configures the pipeline for the web application.
        /// </summary>
        /// <param name="app">The web application instance.</param>
        /// <returns>The configured web application.</returns>
        public static WebApplication ConfigurePipeline ( this WebApplication app )
            {
                app.UseSerilogRequestLogging ( );

                if ( app.Environment.IsDevelopment ( ) ) app.UseDeveloperExceptionPage ( );

                app.UseStaticFiles ( );
                app.UseRouting ( );
                app.UseIdentityServer ( );
                app.UseAuthorization ( );

                app.MapRazorPages ( ).RequireAuthorization ( );

                return app;
            }
    }