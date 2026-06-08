using FW.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FW.API;

[Route("api/wallet")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class WalletController(IWalletService walletService, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyWallet(CancellationToken ct)
    {
        var result = await walletService.GetMyWalletAsync(currentUser.UserId, ct);
        return Ok(result);
    }

    //[HttpPatch("daily-limit")]
    //[ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    //[ProducesResponseType(StatusCodes.Status400BadRequest)]
    //public async Task<IActionResult> UpdateDailyLimit([FromBody] UpdateDailyLimitRequest request, CancellationToken ct)
    //{
    //    var result = await walletService.UpdateDailyLimitAsync(currentUser.UserId, request, ct);
    //    return Ok(result);
    //}
}
