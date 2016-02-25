using Microsoft.Data.Entity;
using SimpleIdentityServer.DataAccess.SqlServer.Models;

namespace SimpleIdentityServer.DataAccess.SqlServer.Mappings
{
    public static class ResourceOwnerMapping
    {
        public static void AddResourceOwnerMapping(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ResourceOwner>()
                .ToTable("resourceOwners")
                .HasKey(j => j.Id);
            modelBuilder.Entity<ResourceOwner>()
                .HasOne(r => r.Address)
                .WithOne(a => a.ResourceOwner);
            /*
            ToTable("resourceOwners");
            HasKey(r => r.Id);
            Property(r => r.Name);
            Property(r => r.GivenName);
            Property(r => r.FamilyName);
            Property(r => r.MiddleName);
            Property(r => r.NickName);
            Property(r => r.PreferredUserName);
            Property(r => r.Profile);
            Property(r => r.Picture);
            Property(r => r.WebSite);
            Property(r => r.Email);
            Property(r => r.EmailVerified);
            Property(r => r.Gender);
            Property(r => r.BirthDate);
            Property(r => r.ZoneInfo);
            Property(r => r.Locale);
            Property(r => r.PhoneNumber);
            Property(r => r.PhoneNumberVerified);
            Property(r => r.UpdatedAt);
            Property(r => r.Password);
            HasOptional(r => r.Address)
                .WithRequired(a => a.ResourceOwner);
            */
        }
    }
}
