using System.Security.Claims;
using IdentityModel;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

public class SeedData
    {
        public static void EnsureSeedData ( WebApplication app )
            {
                using var scope   = app.Services.GetRequiredService< IServiceScopeFactory > ( ).CreateScope ( );
                var       context = scope.ServiceProvider.GetRequiredService< ApplicationDbContext > ( );
                context.Database.Migrate ( );

                /****************************************************************************************
                 *                                                                                      *
                 *   The code retrieves an instance of the UserManager service for the ApplicationUser  *
                 *   model. This service provides several APIs for managing users in ASP.NET Core.      *
                 *                                                                                      *
                 *   An instance of UserManager<ApplicationUser> is being fetched from the IService     *
                 *   Provider, which provides access to the application's service container. The        *
                 *   UserManager class is primarily used for creating and managing users. It provides   *
                 *   APIs for operations such as creating, deleting, updating the users and managing    *
                 *   their roles, claims, passwords etc.                                                *
                 *                                                                                      *
                 *   The user manager object (userMgr) can now be used to invoke any of these           *
                 *   user-related operations. In this specific use case, it is to be used to fetch      *
                 *   users by their username, create new users, assign claims, and other such user      *
                 *   management tasks as shown in the SeedData class.                                   *
                 *                                                                                      *
                 ****************************************************************************************/
                var userMgr = scope.ServiceProvider.GetRequiredService< UserManager< ApplicationUser > > ( );

                if ( userMgr.Users.Any ( ) ) return;

                var alice = userMgr.FindByNameAsync ( "alice" ).Result;


                if ( alice == null )
                    {
                        alice = new ApplicationUser
                                {
                                        UserName       = "alice",
                                        Email          = "AliceSmith@email.com",
                                        EmailConfirmed = true
                                };
                        var result = userMgr.CreateAsync ( alice, "Pass123$" ).Result;
                        if ( ! result.Succeeded ) throw new Exception ( result.Errors.First ( ).Description );

                        result = userMgr.AddClaimsAsync ( alice, new Claim[ ]
                                {
                                        new ( JwtClaimTypes.Name, "Alice Smith" )
                                } ).Result;
                        if ( ! result.Succeeded ) throw new Exception ( result.Errors.First ( ).Description );
                        Log.Debug ( "alice created" );
                    }
                else
                    {
                        Log.Debug ( "alice already exists" );
                    }

                var bob = userMgr.FindByNameAsync ( "bob" ).Result;
                if ( bob == null )
                    {
                        bob = new ApplicationUser
                                {
                                        UserName       = "bob",
                                        Email          = "BobSmith@email.com",
                                        EmailConfirmed = true
                                };
                        var result = userMgr.CreateAsync ( bob, "Pass123$" ).Result;
                        if ( ! result.Succeeded ) throw new Exception ( result.Errors.First ( ).Description );

                        result = userMgr.AddClaimsAsync ( bob, new Claim[ ]
                                {
                                        new ( JwtClaimTypes.Name, "Bob Smith" )
                                } ).Result;
                        if ( ! result.Succeeded ) throw new Exception ( result.Errors.First ( ).Description );
                        Log.Debug ( "bob created" );
                    }
                else
                    {
                        Log.Debug ( "bob already exists" );
                    }
            }
    }