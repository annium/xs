using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NodaTime;

namespace Xs.Registry.Db.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dotnet");

            migrationBuilder.EnsureSchema(
                name: "node");

            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "shared",
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
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    LowerName = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Published = table.Column<Instant>(nullable: false),
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
                        principalSchema: "shared",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                schema: "shared",
                columns: table => new
                {
                    Token = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false),
                    Expires = table.Column<Instant>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Token);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "shared",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Package",
                schema: "dotnet",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MetaPackageId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    LowerName = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Published = table.Column<Instant>(nullable: false),
                    Downloads = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package", x => x.Id);
                    table.UniqueConstraint("AK_Package_LowerName_Version", x => new { x.LowerName, x.Version });
                    table.ForeignKey(
                        name: "FK_Package_MetaPackages_MetaPackageId",
                        column: x => x.MetaPackageId,
                        principalSchema: "shared",
                        principalTable: "MetaPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Package",
                schema: "node",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MetaPackageId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    LowerName = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Published = table.Column<Instant>(nullable: false),
                    Downloads = table.Column<int>(nullable: false),
                    Main = table.Column<string>(nullable: false),
                    Shasum = table.Column<string>(nullable: false),
                    Integrity = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Package", x => x.Id);
                    table.UniqueConstraint("AK_Package_LowerName_Version", x => new { x.LowerName, x.Version });
                    table.ForeignKey(
                        name: "FK_Package_MetaPackages_MetaPackageId",
                        column: x => x.MetaPackageId,
                        principalSchema: "shared",
                        principalTable: "MetaPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetaPackagePermissions",
                schema: "shared",
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
                        principalSchema: "shared",
                        principalTable: "MetaPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageDependency",
                schema: "dotnet",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(nullable: false),
                    Framework = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Version = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageDependency", x => new { x.PackageId, x.Framework, x.Name });
                    table.ForeignKey(
                        name: "FK_PackageDependency_Package_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "dotnet",
                        principalTable: "Package",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageDependency",
                schema: "node",
                columns: table => new
                {
                    PackageId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Version = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageDependency", x => new { x.PackageId, x.Name });
                    table.ForeignKey(
                        name: "FK_PackageDependency_Package_PackageId",
                        column: x => x.PackageId,
                        principalSchema: "node",
                        principalTable: "Package",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Package_MetaPackageId",
                schema: "dotnet",
                table: "Package",
                column: "MetaPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Package_MetaPackageId",
                schema: "node",
                table: "Package",
                column: "MetaPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_MetaPackages_OwnerId",
                schema: "shared",
                table: "MetaPackages",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                schema: "shared",
                table: "UserSessions",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageDependency",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "PackageDependency",
                schema: "node");

            migrationBuilder.DropTable(
                name: "MetaPackagePermissions",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "UserSessions",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "Package",
                schema: "dotnet");

            migrationBuilder.DropTable(
                name: "Package",
                schema: "node");

            migrationBuilder.DropTable(
                name: "MetaPackages",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "shared");
        }
    }
}
