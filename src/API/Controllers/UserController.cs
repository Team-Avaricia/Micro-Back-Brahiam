using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Application.Interfaces;
using Core.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ITelegramService _telegramService;

        public UserController(
            IUserRepository userRepository,
            ITransactionRepository transactionRepository,
            ITelegramService telegramService)
        {
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;
            _telegramService = telegramService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = new User(request.Name, request.Email, request.PhoneNumber, request.InitialBalance);
                await _userRepository.AddAsync(user);

                return Ok(new 
                { 
                    message = "User created successfully", 
                    userId = user.Id,
                    name = user.Name,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    currentBalance = user.CurrentBalance
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var userGuid = Guid.Parse(id);
                var user = await _userRepository.GetByIdAsync(userGuid);

                if (user == null)
                    return NotFound(new { error = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                    return NotFound(new { error = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get telegramId for a user given their userId (needed for notifications)
        /// </summary>
        [HttpGet("{userId}/telegram")]
        public async Task<IActionResult> GetUserTelegramId(string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var user = await _userRepository.GetByIdAsync(userGuid);

                if (user == null)
                    return NotFound(new { error = "User not found" });

                if (user.TelegramId == null)
                    return NotFound(new { error = "User does not have Telegram linked" });

                return Ok(new
                {
                    telegramId = user.TelegramId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("telegram/{telegramId}")]
        public async Task<IActionResult> GetUserByTelegramId(long telegramId)
        {
            try
            {
                var user = await _userRepository.GetByTelegramIdAsync(telegramId);

                if (user == null)
                    return NotFound(new { error = "User not found with this TelegramId" });

                return Ok(new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    telegramId = user.TelegramId,
                    telegramUsername = user.TelegramUsername,
                    currentBalance = user.CurrentBalance
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("phone/{phoneNumber}")]
        public async Task<IActionResult> GetUserByPhone(string phoneNumber)
        {
            try
            {
                var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);

                if (user == null)
                    return NotFound(new { error = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{userId}/balance")]
        public async Task<IActionResult> GetBalance(string userId)
        {
            try
            {
                Guid userGuid;

                // Try to parse as GUID first, if fails, treat as email
                if (!Guid.TryParse(userId, out userGuid))
                {
                    // userId is an email, find the user
                    var userByEmail = await _userRepository.GetByEmailAsync(userId);
                    if (userByEmail == null)
                    {
                        return NotFound(new { error = $"User with email '{userId}' not found" });
                    }
                    userGuid = userByEmail.Id;
                }

                var user = await _userRepository.GetByIdAsync(userGuid);

                if (user == null)
                    return NotFound(new { error = "User not found" });

                var transactions = await _transactionRepository.GetByUserIdAsync(userGuid);
                var transactionList = transactions.ToList();

                var totalIncome = transactionList
                    .Where(t => t.Type == Core.Domain.Enums.TransactionType.Income)
                    .Sum(t => t.Amount);

                var totalExpenses = transactionList
                    .Where(t => t.Type == Core.Domain.Enums.TransactionType.Expense)
                    .Sum(t => t.Amount);

                // Calculate real balance based on transactions
                var calculatedBalance = totalIncome - totalExpenses;

                // Sync the database if balance is out of sync
                if (user.CurrentBalance != calculatedBalance)
                {
                    var difference = calculatedBalance - user.CurrentBalance;
                    user.UpdateBalance(difference);
                    await _userRepository.UpdateAsync(user);
                }

                var lastTransaction = transactionList
                    .OrderByDescending(t => t.Date)
                    .FirstOrDefault();

                return Ok(new
                {
                    totalIncome,
                    totalExpenses,
                    currentBalance = calculatedBalance,
                    lastTransactionDate = lastTransaction?.Date
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{userId}/telegram-link")]
        public async Task<IActionResult> GenerateTelegramLink(string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var response = await _telegramService.GenerateLinkAsync(userGuid);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("already has Telegram linked"))
                {
                    return BadRequest(new { error = ex.Message });
                }
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("link-telegram")]
        public async Task<IActionResult> LinkTelegram([FromBody] LinkTelegramRequest request)
        {
            try
            {
                var user = await _telegramService.LinkTelegramAsync(request);
                return Ok(new
                {
                    success = true,
                    message = "Telegram account linked successfully",
                    userId = user.Id,
                    userName = user.Name
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("{userId}/telegram-status")]
        public async Task<IActionResult> GetTelegramStatus(string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var status = await _telegramService.GetStatusAsync(userGuid);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{userId}/telegram")]
        public async Task<IActionResult> UnlinkTelegram(string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                await _telegramService.UnlinkTelegramAsync(userGuid);
                return Ok(new { success = true, message = "Telegram account unlinked" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}