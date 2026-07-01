using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Monivise.Domain.Entities;

namespace Monivise.API.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected Guid UserId =>
            Guid.Parse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                       ?? throw new UnauthorizedAccessException());
    }
}
