using Microsoft.AspNetCore.Mvc;
using InventraBackend.Strategies;

namespace InventraBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRetrievalStrategy _retrievalStrategy;

        public UsersController(IUserRetrievalStrategy retrievalStrategy)
        {
            _retrievalStrategy = retrievalStrategy;
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required.");

            var user = await _retrievalStrategy.GetUserProfileAsync(username);

            if (user == null)
                return NotFound("User not found.");

            return Ok(user);
        }
    }
}