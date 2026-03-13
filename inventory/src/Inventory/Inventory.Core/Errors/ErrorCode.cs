using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.Errors
{
    public static class ErrorCodes
    {
        public const string SkuAlreadyExists = "SKU_ALREADY_EXISTS";
        public const string SkuNotFound = "SKU_NOT_FOUND";
        public const string InvalidInput = "INVALID_INPUT";
        public const string InsufficientAvailable = "INSUFFICIENT_AVAILABLE";
        public const string ReservedNegative = "RESERVED_NEGATIVE";
        public const string AvailableNegative = "AVAILABLE_NEGATIV";
    }
    public class DomainException : Exception
    {
        public string Code { get; }

        public DomainException(string code, string message) : base(message)
        {
            Code = code;
        }
    }

}
