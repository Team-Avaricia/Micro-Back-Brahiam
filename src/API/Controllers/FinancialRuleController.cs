using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Application.DTOs;
using Core.Domain.Entities;
using Core.Domain.Interfaces;

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

        /// <summary>
        /// Endpoint para Dashboard Vue.js: Crea una nueva regla financiera
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateRule([FromBody] CreateFinancialRuleRequest request)
        {
            try
            {
                var userId = Guid.Parse(request.UserId);
                var rule = new FinancialRule(userId, request.Type, request.Category, request.AmountLimit, request.Period);
                
                await _ruleRepository.AddAsync(rule);
                
                // Retornar el objeto completo de la regla como espera Johan
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

        /// <summary>
        /// Obtiene todas las reglas activas de un usuario
        /// </summary>
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

        /// <summary>
        /// Obtiene una regla específica por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRule(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);
                
                if (rule == null)
                    return NotFound(new { error = "Regla no encontrada" });

                return Ok(rule);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Desactiva una regla (soft delete)
        /// </summary>
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateRule(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);

                if (rule == null)
                    return NotFound(new { error = "Regla no encontrada" });

                rule.Deactivate();
                await _ruleRepository.UpdateAsync(rule);

                return Ok(new { message = "Regla desactivada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Elimina permanentemente una regla
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRule(string id)
        {
            try
            {
                var ruleGuid = Guid.Parse(id);
                var rule = await _ruleRepository.GetByIdAsync(ruleGuid);

                if (rule == null)
                    return NotFound(new { error = "Regla no encontrada" });

                await _ruleRepository.DeleteAsync(ruleGuid);

                return Ok(new { message = "Regla eliminada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
