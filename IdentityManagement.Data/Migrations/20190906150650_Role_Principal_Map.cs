using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IdentityManagement.Data.Migrations
{
    public partial class Role_Principal_Map : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "RolePrincipalMaps",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePrincipalMaps_GroupId",
                table: "RolePrincipalMaps",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePrincipalMaps_Principals_GroupId",
                table: "RolePrincipalMaps",
                column: "GroupId",
                principalTable: "Principals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePrincipalMaps_Principals_GroupId",
                table: "RolePrincipalMaps");

            migrationBuilder.DropIndex(
                name: "IX_RolePrincipalMaps_GroupId",
                table: "RolePrincipalMaps");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "RolePrincipalMaps");
        }
    }
}
