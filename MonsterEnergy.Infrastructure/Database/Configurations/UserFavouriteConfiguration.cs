using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MonsterEnergy.Domain.Entities;

namespace MonsterEnergy.Infrastructure.Database.Configurations
{
    public class UserFavouriteConfiguration : IEntityTypeConfiguration<UserFavourite>
    {
        public void Configure(EntityTypeBuilder<UserFavourite> builder)
        {
            builder.ToTable("UserFavourites");

            builder.HasKey(x => new { x.Username, x.StoreGuid });

            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.HasOne(x => x.Store)
                .WithMany()
                .HasForeignKey(x => x.StoreGuid)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
