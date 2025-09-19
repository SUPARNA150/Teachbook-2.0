using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Teachbook.Web.Data
{
    public class AuthDbContext : IdentityDbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed Roles (User, Admin, SuperAdmin)

            var adminRoleId = "5dfa745b-6159-472b-8ded-605e5fb135d1";
            var superAdminRoleId = "cda6b228-fb2a-4b89-9447-556c6c168ecf";
            var userRoleId = "2924c0ae-5a23-4232-a7cd-1735107eaba3";

            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Name= "Admin",
                    NormalizedName = "Admin",
                    Id = adminRoleId,
                    ConcurrencyStamp = adminRoleId
                },
                new IdentityRole
                {
                    Name = "SuperAdmin",
                    NormalizedName = "SuperAdmin",
                    Id = superAdminRoleId,
                    ConcurrencyStamp = superAdminRoleId
                },
                new IdentityRole
                {
                    Name = "User",
                    NormalizedName = "User",
                    Id = userRoleId,
                    ConcurrencyStamp = userRoleId
                }
            };

            builder.Entity<IdentityRole>().HasData(roles);

            // Seed SuperAdminUser

            var superAdminId = "9642c83b-1381-4025-8b7a-a5cfd7a19502";

            var superAdminUser = new IdentityUser
            {
                UserName = "superadmin@teachbook.com",
                Email = "superadmin@teachbook.com",
                NormalizedEmail = "superadmin@teachbook.com".ToUpper(),
                NormalizedUserName = "superadmin@teachbook.com".ToUpper(),
                Id = superAdminId,
                // Use a fixed, pre-generated hash instead of regenerating it
                PasswordHash = "AQAAAAIAAYagAAAAEEiDmtkhaAgnmZhmswODWlY2wIKxHShqIR5cxUXia2ZfPtw0N7eBgkE+DJgGb1qsHw==",
                SecurityStamp = "f92a7c13-09bb-43c0-9303-8dfd9ab3d27f",
                ConcurrencyStamp = "8e4d3d59-5d6b-4adf-98cf-bc1b7a1a63de"
            };

            //superAdminUser.PasswordHash = new PasswordHasher<IdentityUser>()
               //.HashPassword(superAdminUser, "Superadmin@123");

           

            builder.Entity<IdentityUser>().HasData(superAdminUser);

            // Add All roles to SuperAdminUser

            var superAdminRoles = new List<IdentityUserRole<string>>
            {
                new IdentityUserRole<string>
                {
                    RoleId = adminRoleId,
                    UserId = superAdminId
                },
                new IdentityUserRole<string>
                {
                    RoleId = superAdminRoleId,
                    UserId = superAdminId
                },
                new IdentityUserRole<string>
                {
                    RoleId = userRoleId,
                    UserId = superAdminId
                }
            };

            builder.Entity<IdentityUserRole<string>>().HasData(superAdminRoles);
        }
    }
}
