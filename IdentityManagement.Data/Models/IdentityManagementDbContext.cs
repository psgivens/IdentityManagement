using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace IdentityManagement.Models {
    public class IdentityManagementDbContext : DbContext {
        public IdentityManagementDbContext (DbContextOptions<IdentityManagementDbContext> options) : base (options) { }
        public IdentityManagementDbContext () { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        { 
            // FIXME: Pull this information from somewhere else
            var connectionString = "User ID=samplesam;Password=Password1;Host=localhost;Port=5432;Database=IdentityManagementDb;Pooling=true;";
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<UserEventEnvelopeEntity>()
                .HasKey(e => new { e.StreamId, e.UserId, e.Id });

            modelBuilder
                .Entity<GroupPrincipalMap>()
                .HasKey(e => new { e.GroupId, e.PrincipalId });

            // Some samples from the Toastmasters example. 
            
            // modelBuilder.Entity<RoleRequestEnvelopeEntity>().ToTable("RoleRequestEvents");
            // modelBuilder.Entity<RolePlacementEnvelopeEntity>().ToTable("RolePlacementEvents");

            // modelBuilder
            //     .Entity<RoleRequestMeeting>()
            //     .HasKey(e => new { e.RoleRequestId, e.MeetingId });

            base.OnModelCreating(modelBuilder);
        }


        public virtual DbSet<UserEventEnvelopeEntity> UserEvents { get; set; }
        public virtual DbSet<GroupEventEnvelopeEntity> GroupEvents { get; set; }
        public virtual DbSet<RoleEventEnvelopeEntity> RoleEvents { get; set; }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Principal> Principals { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
    }
}