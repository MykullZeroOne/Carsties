using System.Security.Claims;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityService.Pages.Account.Register;

[SecurityHeaders]
[AllowAnonymous]
public class Index ( UserManager< ApplicationUser > userManager ) : PageModel
    {
        [BindProperty] public RegisterViewModel Input { get; set; }

        [BindProperty] public bool RegisterSuccess { get; set; }

        public IActionResult OnGet ( string returnUrl )
            {
                Input = new RegisterViewModel
                        {
                                ReturnUrl = returnUrl
                        };
                return Page ( );
            }

        public async Task< IActionResult > OnPost ( )
            {
                if ( Input.Button != "Register" ) return Redirect ( "~/" );
                if ( ModelState.IsValid )
                    {
                        var user = new ApplicationUser
                                {
                                        UserName       = Input.UserName,
                                        Email          = Input.Email,
                                        EmailConfirmed = true
                                };
                        var result = await userManager.CreateAsync ( user, Input.Password );

                        if ( result.Succeeded )
                            {
                                await userManager.AddClaimsAsync ( user, new Claim[ ]
                                        {
                                                new ( ClaimTypes.Name, Input.FullName )
                                        } );
                                // Registration successful
                                RegisterSuccess = true;
                            }
                        else
                            {
                                // Registration failed
                                foreach ( var error in result.Errors ) ModelState.AddModelError ( string.Empty, error.Description );
                            }

                        return Page ( );
                    }

                return null;
            }
    }