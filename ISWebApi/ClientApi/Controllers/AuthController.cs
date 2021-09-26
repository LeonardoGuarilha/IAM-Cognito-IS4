using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClientApi.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class AuthController : ControllerBase
    {

        [Authorize]
        [HttpGet("signin")]
        public IActionResult SignIn()
        {
            return Ok("Logado");
        }

        [HttpGet("signout")]
        public IActionResult Signout()
        {
            return SignOut("Cookies", "oidc");
        }
        
        
    }
}