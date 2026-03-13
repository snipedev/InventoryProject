using System;
using System.Collections.Generic;
using System.Text;
using Inventory.Core.Entities;
using Inventory.Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Inventory.Infrastructure.EntityTypeConfig
{

    public class InventorySkuConfiguration : IEntityTypeConfiguration<InventorySku>
    {
        [Obsolete]
        public void Configure(EntityTypeBuilder<InventorySku> sku)
        {
            sku.ToTable("inventory_sku");

            // Key: Sku (value object) -> text
            sku.HasKey(x => x.Sku);
            sku.Property(x => x.Sku)
               .HasConversion(new SkuConverter())
               .HasColumnName("sku")
               .HasMaxLength(100)
               .IsRequired();

            sku.Property(x => x.Available)
               .HasColumnName("available")
               .IsRequired();

            sku.Property(x => x.Reserved)
               .HasColumnName("reserved")
               .IsRequired();

            sku.Property(x => x.Version)
               .HasColumnName("version")
               .IsRequired()
               .HasDefaultValue(0)
               // mark as concurrency token to enforce optimistic concurrency
               .IsConcurrencyToken();

            sku.Property(x => x.UpdatedAt)
               .HasColumnName("updated_at")
               .HasDefaultValueSql("now()")
               .IsRequired();

            // Check constraints
            sku.HasCheckConstraint("ck_available_nonneg", "available >= 0");
            sku.HasCheckConstraint("ck_reserved_nonneg", "reserved >= 0");
        }
    }

}
