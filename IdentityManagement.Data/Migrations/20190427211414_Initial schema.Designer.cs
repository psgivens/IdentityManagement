﻿// <auto-generated />
using System;
using IdentityManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IdentityManagement.Data.Migrations
{
    [DbContext(typeof(IdentityManagementDbContext))]
    [Migration("20190427211414_Initial schema")]
    partial class Initialschema
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("IdentityManagement.Models.EventStream", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("Created");

                    b.HasKey("Id");

                    b.ToTable("EventStream");
                });

            modelBuilder.Entity("IdentityManagement.Models.GroupEventEnvelopeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DeviceId");

                    b.Property<string>("Event");

                    b.Property<long>("StreamId");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid>("TransactionId");

                    b.Property<string>("UserId");

                    b.Property<long>("Version");

                    b.HasKey("Id");

                    b.HasIndex("StreamId");

                    b.ToTable("GroupEvents");
                });

            modelBuilder.Entity("IdentityManagement.Models.GroupPrincipalMap", b =>
                {
                    b.Property<long>("GroupId");

                    b.Property<long>("PrincipalId");

                    b.HasKey("GroupId", "PrincipalId");

                    b.HasIndex("PrincipalId");

                    b.ToTable("GroupPrincipalMap");
                });

            modelBuilder.Entity("IdentityManagement.Models.Principal", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<long?>("RoleId");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("Principals");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Principal");
                });

            modelBuilder.Entity("IdentityManagement.Models.Role", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("IdentityManagement.Models.RoleEventEnvelopeEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("DeviceId");

                    b.Property<string>("Event");

                    b.Property<long>("StreamId");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid>("TransactionId");

                    b.Property<string>("UserId");

                    b.Property<long>("Version");

                    b.HasKey("Id");

                    b.HasIndex("StreamId");

                    b.ToTable("RoleEvents");
                });

            modelBuilder.Entity("IdentityManagement.Models.UserEventEnvelopeEntity", b =>
                {
                    b.Property<long>("StreamId");

                    b.Property<string>("UserId");

                    b.Property<Guid>("Id");

                    b.Property<string>("DeviceId");

                    b.Property<string>("Event");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<Guid>("TransactionId");

                    b.Property<long>("Version");

                    b.HasKey("StreamId", "UserId", "Id");

                    b.ToTable("UserEvents");
                });

            modelBuilder.Entity("IdentityManagement.Models.Group", b =>
                {
                    b.HasBaseType("IdentityManagement.Models.Principal");

                    b.Property<string>("Name");

                    b.HasDiscriminator().HasValue("Group");
                });

            modelBuilder.Entity("IdentityManagement.Models.User", b =>
                {
                    b.HasBaseType("IdentityManagement.Models.Principal");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.HasDiscriminator().HasValue("User");
                });

            modelBuilder.Entity("IdentityManagement.Models.GroupEventEnvelopeEntity", b =>
                {
                    b.HasOne("IdentityManagement.Models.EventStream", "Stream")
                        .WithMany()
                        .HasForeignKey("StreamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IdentityManagement.Models.GroupPrincipalMap", b =>
                {
                    b.HasOne("IdentityManagement.Models.Group", "Group")
                        .WithMany("MemberRelations")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("IdentityManagement.Models.Principal", "Principal")
                        .WithMany("EncapsulatingGroups")
                        .HasForeignKey("PrincipalId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IdentityManagement.Models.Principal", b =>
                {
                    b.HasOne("IdentityManagement.Models.Role")
                        .WithMany("Members")
                        .HasForeignKey("RoleId");
                });

            modelBuilder.Entity("IdentityManagement.Models.RoleEventEnvelopeEntity", b =>
                {
                    b.HasOne("IdentityManagement.Models.EventStream", "Stream")
                        .WithMany()
                        .HasForeignKey("StreamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("IdentityManagement.Models.UserEventEnvelopeEntity", b =>
                {
                    b.HasOne("IdentityManagement.Models.EventStream", "Stream")
                        .WithMany()
                        .HasForeignKey("StreamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
