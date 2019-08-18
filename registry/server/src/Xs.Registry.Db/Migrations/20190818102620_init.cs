using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Xs.Registry.Db.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PasswordHash = table.Column<string>(nullable: false),
                    ApiToken = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetaPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    LowerName = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Published = table.Column<DateTime>(nullable: false),
                    Downloads = table.Column<int>(nullable: false),
                    OwnerId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaPackages", x => x.Id);
                    table.UniqueConstraint("AK_MetaPackages_Type_LowerName", x => new { x.Type, x.LowerName });
                    table.ForeignKey(
                        name: "FK_MetaPackages_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Token = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Expires = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Token);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DotnetPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MetaPackageId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    LowerName = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Published = table.Column<DateTime>(nullable: false),
                    Downloads = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DotnetPackages", x => x.Id);
                    table.UniqueConstraint("AK_DotnetPackages_LowerName_Version", x => new { x.LowerName, x.Version });
                    table.ForeignKey(
                        name: "FK_DotnetPackages_MetaPackages_MetaPackageId",
                        column: x => x.MetaPackageId,
                        principalTable: "MetaPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetaPackagePermissions",
                columns: table => new
                {
                    MetaPackageId = table.Column<Guid>(nullable: false),
                    Category = table.Column<int>(nullable: false),
                    Permission = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetaPackagePermissions", x => new { x.MetaPackageId, x.Category });
                    table.ForeignKey(
                        name: "FK_MetaPackagePermissions_MetaPackages_MetaPackageId",
                        column: x => x.MetaPackageId,
                        principalTable: "MetaPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MetaPackageId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    LowerName = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Published = table.Column<DateTime>(nullable: false),
                    Downloads = table.Column<int>(nullable: false),
                    Main = table.Column<string>(nullable: false),
                    Shasum = table.Column<string>(nullable: false),
                    Integrity = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodePackages", x => x.Id);
                    table.UniqueConstraint("AK_NodePackages_LowerName_Version", x => new { x.LowerName, x.Version });
                    table.ForeignKey(
                        name: "FK_NodePackages_MetaPackages_MetaPackageId",
                        column: x => x.MetaPackageId,
                        principalTable: "MetaPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DotnetPackageDependencies",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(nullable: false),
                    Framework = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DotnetPackageDependencies", x => new { x.PackageId, x.Framework, x.Name });
                    table.ForeignKey(
                        name: "FK_DotnetPackageDependencies_DotnetPackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "DotnetPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NodePackageDependencies",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Version = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodePackageDependencies", x => new { x.PackageId, x.Name });
                    table.ForeignKey(
                        name: "FK_NodePackageDependencies_NodePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "NodePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DotnetPackages_MetaPackageId",
                table: "DotnetPackages",
                column: "MetaPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaPackages_OwnerId",
                table: "MetaPackages",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_NodePackages_MetaPackageId",
                table: "NodePackages",
                column: "MetaPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DotnetPackageDependencies");

            migrationBuilder.DropTable(
                name: "MetaPackagePermissions");

            migrationBuilder.DropTable(
                name: "NodePackageDependencies");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "DotnetPackages");

            migrationBuilder.DropTable(
                name: "NodePackages");

            migrationBuilder.DropTable(
                name: "MetaPackages");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
