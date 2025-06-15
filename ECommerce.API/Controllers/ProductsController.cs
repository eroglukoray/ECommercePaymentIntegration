using ECommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{

    [ApiController]
    [Route("api/products")]
    public class ProductsController(IBalanceManagementService balanceService) : ControllerBase
    {
        private readonly IBalanceManagementService _balanceService = balanceService;

        [HttpGet]
        public async Task<IActionResult> Get()
            => Ok(await _balanceService.GetProductsAsync());
    }
}
