using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonsterEnergy.Application.Dtos;
using MonsterEnergy.Application.Interfaces;

namespace MonsterEnergy.WebApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [AllowAnonymous]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
        {
            var response = await authService.LoginAsync(request, ct);
            if (response is null)
            {
                return Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Invalid credentials",
                    detail: "The username or password is incorrect.");
            }

            return Ok(response);
        }
    }
}
