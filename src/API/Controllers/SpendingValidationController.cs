using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Application.DTOs;
using Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SpendingValidationController : ControllerBase
    {
        private readonly ISpendingValidationService _validationService;

        public SpendingValidationController(ISpendingValidationService validationService)
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
