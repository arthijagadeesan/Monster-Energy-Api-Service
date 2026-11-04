using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonsterEnergy.Application.Dtos;
using MonsterEnergy.Application.Interfaces;

namespace MonsterEnergy.WebApi.Controllers
{
    [ApiController]
    [Route("api/stores")]
    [Authorize]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, "application/problem+json")]
    public class StoresController(IStoreService storeService) : ControllerBase
    {
        private string Username => User.Identity?.Name
            ?? throw new InvalidOperationException("The access token has no name claim.");

        [HttpGet]
        [ProducesResponseType<PagedResult<StoreDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "application/problem+json")]
        public async Task<ActionResult<PagedResult<StoreDto>>> GetStores([FromQuery] StoreQuery query, CancellationToken ct)
        {
            var result = await storeService.SearchAsync(query, Username, ct);
            return Ok(result);
        }

        [HttpGet("{storeGuid:guid}")]
        [ProducesResponseType<StoreDto>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<ActionResult<StoreDto>> GetStore(Guid storeGuid, CancellationToken ct)
        {
            var store = await storeService.GetByIdAsync(storeGuid, Username, ct);
            if (store is null)
            {
                return StoreNotFound(storeGuid);
            }

            return Ok(store);
        }

        [HttpGet("regions")]
        [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<string>>> GetRegions(CancellationToken ct)
        {
            var regions = await storeService.GetRegionsAsync(ct);
            return Ok(regions);
        }

        [HttpGet("channels")]
        [ProducesResponseType<IReadOnlyList<string>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<string>>> GetChannels(CancellationToken ct)
        {
            var channels = await storeService.GetChannelsAsync(ct);
            return Ok(channels);
        }

        [HttpPut("{storeGuid:guid}/favourite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<IActionResult> AddFavourite(Guid storeGuid, CancellationToken ct)
        {
            return await SetFavourite(storeGuid, true, ct);
        }

        [HttpDelete("{storeGuid:guid}/favourite")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, "application/problem+json")]
        public async Task<IActionResult> RemoveFavourite(Guid storeGuid, CancellationToken ct)
        {
            return await SetFavourite(storeGuid, false, ct);
        }

        private async Task<IActionResult> SetFavourite(Guid storeGuid, bool isFavourite, CancellationToken ct)
        {
            var updated = await storeService.SetFavouriteAsync(storeGuid, Username, isFavourite, ct);
            if (!updated)
            {
                return StoreNotFound(storeGuid);
            }

            return NoContent();
        }

        private ObjectResult StoreNotFound(Guid storeGuid)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Store not found",
                detail: $"No store exists with id '{storeGuid}'.");
        }
    }
}
