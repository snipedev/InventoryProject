using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.ValueObjects
{
    public class Sku
    {

        public string Value { get; }

        public Sku(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("SKU cannot be empty.", nameof(value));
            if (value.Length > 100)
                throw new ArgumentException("SKU length must be <= 100 characters.", nameof(value));
            // forbid whitespace inside if you prefer strictly token-like SKUs
            if (value.Any(char.IsWhiteSpace))
                throw new ArgumentException("SKU must not contain whitespace.", nameof(value));

            Value = value;
        }

        public override string ToString() => Value;

        public static implicit operator string(Sku sku) => sku.Value;
        public static implicit operator Sku(string value) => new(value);

    }
}
