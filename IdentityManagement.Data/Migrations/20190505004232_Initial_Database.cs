using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IdentityManagement.Data.Migrations
{
    public partial class Initial_Database : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventStream",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Created = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStream", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Principals",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Discriminator = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Principals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ExternalId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GroupEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StreamId = table.Column<Guid>(nullable: false),
                    StreamId1 = table.Column<long>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false),
                    TransactionId = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    Version = table.Column<short>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    Event = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupEvents_EventStream_StreamId1",
                        column: x => x.StreamId1,
                        principalTable: "EventStream",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RoleEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StreamId = table.Column<Guid>(nullable: false),
                    StreamId1 = table.Column<long>(nullable: true),
                    UserId = table.Column<Guid>(nullable: false),
                    TransactionId = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    Version = table.Column<short>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    Event = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleEvents_EventStream_StreamId1",
                        column: x => x.StreamId1,
                        principalTable: "EventStream",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserEvents",
                columns: table => new
                {
                    StreamId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    StreamId1 = table.Column<long>(nullable: true),
                    TransactionId = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    Version = table.Column<short>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    Event = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEvents", x => new { x.StreamId, x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_UserEvents_EventStream_StreamId1",
                        column: x => x.StreamId1,
                        principalTable: "EventStream",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupPrincipalMaps",
                columns: table => new
                {
                    PrincipalId = table.Column<Guid>(nullable: false),
                    GroupId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPrincipalMaps", x => new { x.GroupId, x.PrincipalId });
                    table.ForeignKey(
                        name: "FK_GroupPrincipalMaps_Principals_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Principals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPrincipalMaps_Principals_PrincipalId",
                        column: x => x.PrincipalId,
                        principalTable: "Principals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePrincipalMaps",
                columns: table => new
                {
                    PrincipalId = table.Column<Guid>(nullable: false),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePrincipalMaps", x => new { x.RoleId, x.PrincipalId });
                    table.ForeignKey(
                        name: "FK_RolePrincipalMaps_Principals_PrincipalId",
                        column: x => x.PrincipalId,
                        principalTable: "Principals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePrincipalMaps_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupEvents_StreamId1",
                table: "GroupEvents",
                column: "StreamId1");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPrincipalMaps_PrincipalId",
                table: "GroupPrincipalMaps",
                column: "PrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleEvents_StreamId1",
                table: "RoleEvents",
                column: "StreamId1");

            migrationBuilder.CreateIndex(
                name: "IX_RolePrincipalMaps_PrincipalId",
                table: "RolePrincipalMaps",
                column: "PrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_UserEvents_StreamId1",
                table: "UserEvents",
                column: "StreamId1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupEvents");

            migrationBuilder.DropTable(
                name: "GroupPrincipalMaps");

            migrationBuilder.DropTable(
                name: "RoleEvents");

            migrationBuilder.DropTable(
                name: "RolePrincipalMaps");

            migrationBuilder.DropTable(
                name: "UserEvents");

            migrationBuilder.DropTable(
                name: "Principals");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "EventStream");
        }
    }
}
