using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Core.Abstraction
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }

    public sealed class SystemDateTiemProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
