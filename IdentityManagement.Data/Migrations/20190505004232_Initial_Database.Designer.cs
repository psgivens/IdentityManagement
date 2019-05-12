﻿// <auto-generated />
using System;
using IdentityManagement.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IdentityManagement.Data.Migrations
{
    [DbContext(typeof(IdentityManagementDbContext))]
    [Migration("20190505004232_Initial_Database")]
    partial class Initial_Database
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("IdentityManagement.Data.Models.EventStream", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("Created");

                    b.HasKey("Id");

                    b.ToTable("EventStream");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.GroupEventEnvelopeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DeviceId");

                    b.Property<string>("Event");

                    b.Property<Guid>("StreamId");

                    b.Property<long?>("StreamId1");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid>("TransactionId");

                    b.Property<Guid>("UserId");

                    b.Property<short>("Version");

                    b.HasKey("Id");

                    b.HasIndex("StreamId1");

                    b.ToTable("GroupEvents");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.GroupPrincipalMap", b =>
                {
                    b.Property<Guid>("GroupId");

                    b.Property<Guid>("PrincipalId");

                    b.HasKey("GroupId", "PrincipalId");

                    b.HasIndex("PrincipalId");

                    b.ToTable("GroupPrincipalMaps");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.Principal", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("Principals");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Principal");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ExternalId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.RoleEventEnvelopeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DeviceId");

                    b.Property<string>("Event");

                    b.Property<Guid>("StreamId");

                    b.Property<long?>("StreamId1");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid>("TransactionId");

                    b.Property<Guid>("UserId");

                    b.Property<short>("Version");

                    b.HasKey("Id");

                    b.HasIndex("StreamId1");

                    b.ToTable("RoleEvents");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.RolePrincipalMap", b =>
                {
                    b.Property<Guid>("RoleId");

                    b.Property<Guid>("PrincipalId");

                    b.HasKey("RoleId", "PrincipalId");

                    b.HasIndex("PrincipalId");

                    b.ToTable("RolePrincipalMaps");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.UserEventEnvelopeEntity", b =>
                {
                    b.Property<Guid>("StreamId");

                    b.Property<Guid>("UserId");

                    b.Property<Guid>("Id");

                    b.Property<string>("DeviceId");

                    b.Property<string>("Event");

                    b.Property<long?>("StreamId1");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid>("TransactionId");

                    b.Property<short>("Version");

                    b.HasKey("StreamId", "UserId", "Id");

                    b.HasIndex("StreamId1");

                    b.ToTable("UserEvents");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.Group", b =>
                {
                    b.HasBaseType("IdentityManagement.Data.Models.Principal");

                    b.Property<string>("Name");

                    b.HasDiscriminator().HasValue("Group");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.User", b =>
                {
                    b.HasBaseType("IdentityManagement.Data.Models.Principal");

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.HasDiscriminator().HasValue("User");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.GroupEventEnvelopeEntity", b =>
                {
                    b.HasOne("IdentityManagement.Data.Models.EventStream", "Stream")
                        .WithMany()
                        .HasForeignKey("StreamId1");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.GroupPrincipalMap", b =>
                {
                    b.HasOne("IdentityManagement.Data.Models.Group", "Group")
                        .WithMany("MemberRelations")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IdentityManagement.Data.Models.Principal", "Principal")
                        .WithMany()
                        .HasForeignKey("PrincipalId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.RoleEventEnvelopeEntity", b =>
                {
                    b.HasOne("IdentityManagement.Data.Models.EventStream", "Stream")
                        .WithMany()
                        .HasForeignKey("StreamId1");
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.RolePrincipalMap", b =>
                {
                    b.HasOne("IdentityManagement.Data.Models.Principal", "Principal")
                        .WithMany()
                        .HasForeignKey("PrincipalId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IdentityManagement.Data.Models.Role", "Role")
                        .WithMany("Members")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IdentityManagement.Data.Models.UserEventEnvelopeEntity", b =>
                {
                    b.HasOne("IdentityManagement.Data.Models.EventStream", "Stream")
                        .WithMany()
                        .HasForeignKey("StreamId1");
                });
#pragma warning restore 612, 618
        }
    }
}