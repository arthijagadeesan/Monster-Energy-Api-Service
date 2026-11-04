using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonsterEnergy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonsterEnergy.Infrastructure.Database.Configurations
{
    public class StoreConfiguration : IEntityTypeConfiguration<Store>
    {
        public void Configure(EntityTypeBuilder<Store> builder)
        {
            // Table and primary key
            builder.ToTable("Stores");

            builder.HasKey(x => x.StoreGuid);

            builder.Property(x => x.StoreGuid)
                .ValueGeneratedNever();

            // Store hierarchy
            builder.Property(x => x.Tdm)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Territory)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Region)
                .IsRequired()
                .HasMaxLength(100);

            // Store details
            builder.Property(x => x.StoreName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.PostCode)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.StoreChannel)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.StoreFascia)
                .IsRequired()
                .HasMaxLength(100);

            // Date and time
            builder.Property(x => x.Date)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(x => x.StoreStart)
                .IsRequired()
                .HasColumnType("time");

            // Indexes
            builder.HasIndex(x => x.Region);

            builder.HasIndex(x => x.StoreChannel);

            builder.HasIndex(x => x.Date);
        }
    }
}
