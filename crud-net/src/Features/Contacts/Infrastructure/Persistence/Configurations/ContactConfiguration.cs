using crud_net.Features.Contacts.Domain.Entities;
using crud_net.Features.Contacts.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace crud_net.Features.Contacts.Infrastructure.Persistence.Configurations;

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