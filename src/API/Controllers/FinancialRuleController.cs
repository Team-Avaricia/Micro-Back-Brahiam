using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialRuleController : ControllerBase
    {
        private readonly IFinancialRuleRepository _ruleRepository;

        public FinancialRuleController(IFinancialRuleRepository ruleRepository)
        {
            _ruleRepository = ruleRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRule([FromBody] CreateFinancialRuleRequest request)
        {
            try
            {
                var userId = Guid.Parse(request.UserId);
                var rule = new FinancialRule(userId, request.Type, request.Category, request.AmountLimit, request.Period);
                
                await _ruleRepository.AddAsync(rule);
                
                return Ok(new
                {
                    id = rule.Id,
                    userId = rule.UserId,
                    type = rule.Type.ToString(),
                    category = rule.Category,
                    amountLimit = rule.AmountLimit,
                    period = rule.Period.ToString(),
                    isActive = rule.IsActive,
                    createdAt = rule.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserRules(string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var rules = await _ruleRepository.GetActiveRulesByUserIdAsync(userGuid);
                return Ok(rules);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRule(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);
                
                if (rule == null)
                    return NotFound(new { error = "Rule not found" });

                return Ok(rule);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateRule(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);

                if (rule == null)
                    return NotFound(new { error = "Rule not found" });

                rule.Deactivate();
                await _ruleRepository.UpdateAsync(rule);

                return Ok(new { message = "Rule deactivated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRule(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);

                if (rule == null)
                    return NotFound(new { error = "Rule not found" });

                await _ruleRepository.DeleteAsync(ruleGuid);

                return Ok(new { message = "Rule deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
