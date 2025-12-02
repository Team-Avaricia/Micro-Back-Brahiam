using System.ComponentModel.DataAnnotations;
using Core.Domain.Enums;

namespace Core.Application.DTOs
{
    /// <summary>
    /// Request from AI Worker microservice to validate a spending
    /// </summary>
    public class SpendingValidationRequest
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Category { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
    }

    /// <summary>
    /// Response from Core microservice to AI Worker with the verdict
    /// </summary>
    public class SpendingValidationResponse
    {
        public bool IsApproved { get; set; }
        public string Verdict { get; set; } // "Aprobado" o "Rechazado"
        public string Reason { get; set; } // Explicación del veredicto
        public decimal RemainingBudget { get; set; }
    }

    /// <summary>
    /// Request to create a new transaction
    /// </summary>
    public class CreateTransactionRequest
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        public TransactionType Type { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Category { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public TransactionSource Source { get; set; }
    }

    /// <summary>
    /// Request to create a financial rule
    /// </summary>
    public class CreateFinancialRuleRequest
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public RuleType Type { get; set; }
        
        [StringLength(100)]
        public string Category { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount limit must be greater than 0")]
        public decimal AmountLimit { get; set; }
        
        [Required]
        public RulePeriod Period { get; set; }
    }

    /// <summary>
    /// Response for transaction queries with totals
    /// </summary>
    public class TransactionQueryResponse
    {
        public IEnumerable<object> Data { get; set; }
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Response for user balance information
    /// </summary>
    public class BalanceResponse
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime? LastTransactionDate { get; set; }
    }

    /// <summary>
    /// Response for category summary
    /// </summary>
    public class CategorySummaryResponse
    {
        public IEnumerable<CategorySummaryItem> Data { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class CategorySummaryItem
    {
        public string Category { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Request to create a recurring transaction
    /// </summary>
    public class CreateRecurringTransactionRequest
    {
        [Required]
        public string UserId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        public TransactionType Type { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Category { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public RecurrenceFrequency Frequency { get; set; }
        
        [Required]
        public DateTime StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Range(1, 31)]
        public int? DayOfMonth { get; set; }
        
        [Range(0, 6)]
        public int? DayOfWeek { get; set; }
    }

    /// <summary>
    /// Response for monthly cashflow
    /// </summary>
    public class CashflowResponse
    {
        public decimal TotalMonthlyIncome { get; set; }
        public decimal TotalMonthlyExpenses { get; set; }
        public decimal NetMonthlyCashflow { get; set; }
        public IEnumerable<CashflowItem> IncomeBreakdown { get; set; }
        public IEnumerable<CashflowItem> ExpenseBreakdown { get; set; }
    }

    public class CashflowItem
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Request for user registration
    /// </summary>
    public class RegisterRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        [StringLength(50)]
        public string PhoneNumber { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal InitialBalance { get; set; } = 0;
    }

    /// <summary>
    /// Request for user login
    /// </summary>
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    /// <summary>
    /// Authentication response with tokens
    /// </summary>
    public class AuthResponse
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }

    /// <summary>
    /// Request to refresh authentication token
    /// </summary>
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Request to create a new user
    /// </summary>
    public class CreateUserRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }
        
        [Required]
        [Phone]
        [StringLength(50)]
        public string PhoneNumber { get; set; }
        
        [Range(0, double.MaxValue)]
        public decimal InitialBalance { get; set; } = 0;
    }

    /// <summary>
    /// Request to update a recurring transaction
    /// </summary>
    public class UpdateRecurringTransactionRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal? Amount { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Request to toggle recurring transaction status
    /// </summary>
    public class ToggleRecurringTransactionRequest
    {
        public bool IsActive { get; set; }
    }
}
