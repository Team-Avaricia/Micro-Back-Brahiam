using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Domain.Entities;
using Core.Domain.Interfaces;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;

        public UserController(
            IUserRepository userRepository,
            ITransactionRepository transactionRepository)
        {
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                var user = new User(request.Name, request.Email, request.PhoneNumber, request.InitialBalance);
                await _userRepository.AddAsync(user);

                return Ok(new 
                { 
                    message = "Usuario creado exitosamente", 
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

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            try
            {
                var userGuid = Guid.Parse(id);
                var user = await _userRepository.GetByIdAsync(userGuid);

                if (user == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un usuario por email
        /// </summary>
        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);

                if (user == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene un usuario por número de teléfono
        /// </summary>
        [HttpGet("phone/{phoneNumber}")]
        public async Task<IActionResult> GetUserByPhone(string phoneNumber)
        {
            try
            {
                var user = await _userRepository.GetByPhoneNumberAsync(phoneNumber);

                if (user == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el balance actual del usuario
        /// </summary>
        [HttpGet("{userId}/balance")]
        public async Task<IActionResult> GetBalance(string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);
                var user = await _userRepository.GetByIdAsync(userGuid);

                if (user == null)
                    return NotFound(new { error = "Usuario no encontrado" });

                var transactions = await _transactionRepository.GetByUserIdAsync(userGuid);
                var transactionList = transactions.ToList();

                var totalIncome = transactionList
                    .Where(t => t.Type == Core.Domain.Enums.TransactionType.Income)
                    .Sum(t => t.Amount);

                var totalExpenses = transactionList
                    .Where(t => t.Type == Core.Domain.Enums.TransactionType.Expense)
                    .Sum(t => t.Amount);

                var lastTransaction = transactionList
                    .OrderByDescending(t => t.Date)
                    .FirstOrDefault();

                return Ok(new
                {
                    totalIncome,
                    totalExpenses,
                    currentBalance = user.CurrentBalance,
                    lastTransactionDate = lastTransaction?.Date
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class CreateUserRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public decimal InitialBalance { get; set; } = 0;
    }
}
