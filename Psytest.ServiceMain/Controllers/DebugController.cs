using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Psytest.ServiceMain.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        [HttpGet("secure")]
        [Authorize]
        public IActionResult SecureEndpoint()
        {
            return Ok(new
            {
                message = "Вы авторизованы!",
                user = User.Identity?.Name, // email@gmail.com
                claims = User.Claims.Select(c => new { c.Type, c.Value })
            });
        }
    }
}
