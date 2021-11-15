using EvaluationAPI.DTO;
using EvaluationAPI.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EvaluationAPI.Controllers
{
    /// <summary>
    /// <c>Authentication</c> and <c>authorization</c> rest api controller class.
    /// Contains all methods for performing auth rest api logic.
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("AuthPolicy")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region Private Properties
        private readonly UserManager<EvaluationUser> userManager_;

        private readonly RoleManager<IdentityRole> roleManager_;

        private readonly IConfiguration configuration_;
        #endregion

        #region Public Constructors
        public AuthController(UserManager<EvaluationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            userManager_ = userManager;
            roleManager_ = roleManager;
            configuration_ = configuration;
        }
        #endregion

        #region Public Methods
        #region POST
        /// <summary>
        /// Sign in rest api controller.
        /// </summary>
        /// <param name="model">Transfers the username and the password to authenticate user.</param>
        /// <exception cref="Exception">Thrown when <paramref name="model"/> is invalid or unhandled error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkObjectResult"/> with Bearer token and expiration date if no error has occurred.
        /// Returns <see cref="UnauthorizedResult"/> if user is unauthorized.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if any error has occurred.
        /// </returns>
        [HttpPost("signin")]
        public async Task<IActionResult> Signin(UserDTO model)
        {
            try
            {
                EvaluationUser user = await userManager_.FindByNameAsync(model.Username);

                if (user != null && await userManager_.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await userManager_.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration_["JWT:Secret"]));

                    var token = new JwtSecurityToken(
                        issuer: configuration_["JWT:ValidIssuer"],
                        audience: configuration_["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(3),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256));

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

            return Unauthorized();
        }

        /// <summary>
        /// Sign up rest api controller.
        /// </summary>
        /// <param name="model">Transfers the username and the password to generate user's credentials.</param>
        /// <exception cref="Exception">Thrown when <paramref name="model"/> is invalid or unhandled error has occurred.</exception>
        /// <returns>
        /// Returns <see cref="OkResult"/> if no error has occurred.
        /// Returns <see cref="StatusCodeResult"/> with <see cref="StatusCodes.Status500InternalServerError"/> status code if user already exists or unhandled error has occurred.
        /// </returns>
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(UserDTO model)
        {
            try
            {
                EvaluationUser userExists = await userManager_.FindByNameAsync(model.Username);

                if (userExists != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, Constants.USER_ALREADY_EXISTS_ERROR);

                EvaluationUser user = new EvaluationUser()
                {
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username
                };

                var result = await userManager_.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, Constants.USER_CREATION_FAILED);

                // Check if user roles exist in the database.
                if (!await roleManager_.RoleExistsAsync(UserRoles.Admin))
                    await roleManager_.CreateAsync(new IdentityRole(UserRoles.Admin)); // Create if user role doesn't exist.
                if (!await roleManager_.RoleExistsAsync(UserRoles.User))
                    await roleManager_.CreateAsync(new IdentityRole(UserRoles.User)); // Create if user role doesn't exist.

                if (!string.IsNullOrEmpty(model.Role) && await roleManager_.RoleExistsAsync(model.Role))
                {
                    await userManager_.AddToRoleAsync(user, model.Role);
                }
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }

            return Ok();
        }
        #endregion
        #endregion
    }
}
