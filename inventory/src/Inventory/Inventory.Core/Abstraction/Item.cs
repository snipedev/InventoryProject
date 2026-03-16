using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.Abstraction
{
    public readonly struct Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }

        private Result(bool isSuccess, string? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Ok() => new(true, null);
        public static Result Fail(string error) => new(false, error);

        public override string ToString() => IsSuccess ? "Ok" : $"Fail ({Error})";
    }


    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public string? Error { get; }

        private Result(bool ok, T? value, string? error)
        {
            IsSuccess = ok;
            Value = value;
            Error = error;
        }

        public static Result<T> Ok(T value) => new(true, value, null);
        public static Result<T> Fail(string error) => new(false, default, error);

        public void Deconstruct(out bool ok, out T? value, out string? error)
        {
            ok = IsSuccess; value = Value; error = Error;
        }

        public override string ToString() => IsSuccess ? $"Ok({Value})" : $"Fail({Error})";
    }

    public sealed class PagedResult<T>
    {
        public required IReadOnlyList<T> Items { get; init; }
        public required int Page { get; init; }
        public required int PageSize { get; init; }
        public required long Total { get; init; }

    }

}
