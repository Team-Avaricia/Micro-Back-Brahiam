using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Application.DTOs;
using Core.Application.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpendingValidationController : ControllerBase
    {
        private readonly SpendingValidationService _validationService;

        public SpendingValidationController(SpendingValidationService validationService)
        {
            _validationService = validationService;
        }

        [HttpPost("validate")]
        public async Task<ActionResult<SpendingValidationResponse>> ValidateSpending([FromBody] SpendingValidationRequest request)
        {
            try
            {
                var response = await _validationService.ValidateSpendingAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new SpendingValidationResponse
                {
                    IsApproved = false,
                    Verdict = "Error",
                    Reason = $"Validation error: {ex.Message}"
                });
            }
        }
    }
}
