using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;

/// <summary>
///     CustomProfileService class implements the IProfileService interface and provides methods to retrieve and validate user profile data.
/// </summary>
public class CustomProfileService ( UserManager< ApplicationUser > userManager ) : IProfileService
    {
        /// <summary>
        ///     Retrieves the profile data for a user.
        /// </summary>
        /// <param name="context">The profile data request context.</param>
        /// <returns>A <see cref="Task" /> that represents the asynchronous operation.</returns>
        public async Task GetProfileDataAsync ( ProfileDataRequestContext context )
            {
                var user           = await userManager.GetUserAsync ( context.Subject );
                var existingClaims = await userManager.GetClaimsAsync ( user );

                /****************************************************************************************
                 *                                                                                      *
                 *   The below block is a part of the GetProfileDataAsync method in the                 *
                 *   CustomProfileService class. It deals with adding additional user profile           *
                 *   data to the context.                                                               *
                 *                                                                                      *
                 *   Specifically, it creates a new list of claims called "claims". For now, it         *
                 *   adds a single claim for the username with the value of `user.UserName`. These      *
                 *   claims can include other user-related data which are then issued to the context.   *
                 *                                                                                      *
                 *   This is generally used for authentication/authorization where these claims         *
                 *   can be used to allow or restrict user actions based on their roles, or             *
                 *   specific attributes tied to each claim.                                            *
                 *                                                                                      *
                 ****************************************************************************************/
                var claims = new List< Claim >
                        {
                                new ( "username", user.UserName )
                        };
                context.IssuedClaims.AddRange ( claims );
                context.IssuedClaims.Add ( existingClaims.FirstOrDefault ( x => x.Type == JwtClaimTypes.Name ) );
            }

        /// <summary>
        ///     Determines whether the user is active or not.
        /// </summary>
        /// <param name="context">The context containing information about the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task IsActiveAsync ( IsActiveContext context )
            {
                return Task.CompletedTask;
            }
    }