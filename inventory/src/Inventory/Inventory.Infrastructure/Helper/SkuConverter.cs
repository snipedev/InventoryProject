using System.Linq.Expressions;
using Inventory.Core.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Inventory.Infrastructure.Helper
{

    public sealed class SkuConverter : ValueConverter<Sku, string>
    {
        public SkuConverter()
            : base(ToProvider(), FromProvider())
        { }

        private static Expression<Func<Sku, string>> ToProvider()
            => sku => sku.Value;

        private static Expression<Func<string, Sku>> FromProvider()
            => value => new Sku(value);
    }

}
