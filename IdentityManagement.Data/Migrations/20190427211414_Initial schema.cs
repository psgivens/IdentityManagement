using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IdentityManagement.Data.Migrations
{
    public partial class Initialschema : Migration
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
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
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
                    StreamId = table.Column<long>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    TransactionId = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    Version = table.Column<long>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    Event = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupEvents_EventStream_StreamId",
                        column: x => x.StreamId,
                        principalTable: "EventStream",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    StreamId = table.Column<long>(nullable: false),
                    UserId = table.Column<string>(nullable: true),
                    TransactionId = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    Version = table.Column<long>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    Event = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleEvents_EventStream_StreamId",
                        column: x => x.StreamId,
                        principalTable: "EventStream",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserEvents",
                columns: table => new
                {
                    StreamId = table.Column<long>(nullable: false),
                    UserId = table.Column<string>(nullable: false),
                    Id = table.Column<Guid>(nullable: false),
                    TransactionId = table.Column<Guid>(nullable: false),
                    DeviceId = table.Column<string>(nullable: true),
                    Version = table.Column<long>(nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(nullable: false),
                    Event = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserEvents", x => new { x.StreamId, x.UserId, x.Id });
                    table.ForeignKey(
                        name: "FK_UserEvents_EventStream_StreamId",
                        column: x => x.StreamId,
                        principalTable: "EventStream",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Principals",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Discriminator = table.Column<string>(nullable: false),
                    RoleId = table.Column<long>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Principals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Principals_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupPrincipalMap",
                columns: table => new
                {
                    PrincipalId = table.Column<long>(nullable: false),
                    GroupId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPrincipalMap", x => new { x.GroupId, x.PrincipalId });
                    table.ForeignKey(
                        name: "FK_GroupPrincipalMap_Principals_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Principals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPrincipalMap_Principals_PrincipalId",
                        column: x => x.PrincipalId,
                        principalTable: "Principals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GroupEvents_StreamId",
                table: "GroupEvents",
                column: "StreamId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPrincipalMap_PrincipalId",
                table: "GroupPrincipalMap",
                column: "PrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_Principals_RoleId",
                table: "Principals",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleEvents_StreamId",
                table: "RoleEvents",
                column: "StreamId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GroupEvents");

            migrationBuilder.DropTable(
                name: "GroupPrincipalMap");

            migrationBuilder.DropTable(
                name: "RoleEvents");

            migrationBuilder.DropTable(
                name: "UserEvents");

            migrationBuilder.DropTable(
                name: "Principals");

            migrationBuilder.DropTable(
                name: "EventStream");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
