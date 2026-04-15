using crud_net.Features.Contacts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace crud_net.Infrastructure.Persistence;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");

        builder.HasKey(contact => contact.Id);

        builder.Property(contact => contact.Name)
            .IsRequired()
            .HasMaxLength(ContactRules.MaxNameLength);

        builder.Property(contact => contact.DateOfBirth)
            .IsRequired()
            .HasColumnType("date");

        builder.Property(contact => contact.Gender)
            .IsRequired();

        builder.Property(contact => contact.IsActive)
            .IsRequired();

        builder.Property(contact => contact.IsDeleted)
            .IsRequired();

        builder.Property(contact => contact.CreatedAtUtc)
            .IsRequired();

        builder.Property(contact => contact.UpdatedAtUtc);
        builder.Property(contact => contact.DeactivatedAtUtc);
        builder.Property(contact => contact.DeletedAtUtc);

        builder.HasIndex(contact => contact.IsActive);
        builder.HasIndex(contact => contact.IsDeleted);
    }
}