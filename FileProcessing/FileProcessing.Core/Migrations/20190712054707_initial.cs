using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CSS.Connector.FileProcessing.Core.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Direction = table.Column<string>(nullable: true),
                    RegexNameExpression = table.Column<string>(nullable: true),
                    UseHashCodeDuplicateDetection = table.Column<bool>(nullable: false),
                    UseFileParsing = table.Column<bool>(nullable: false),
                    Processor = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileEventLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    InstanceId = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    TimeStamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileEventLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileProcessorConfigs",
                columns: table => new
                {
                    Processor = table.Column<string>(nullable: false),
                    Config = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileProcessorConfigs", x => x.Processor);
                });

            migrationBuilder.CreateTable(
                name: "TypeMappings",
                columns: table => new
                {
                    FileId = table.Column<string>(nullable: false),
                    KeyType = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeMappings", x => new { x.FileId, x.KeyType });
                });

            migrationBuilder.CreateTable(
                name: "FileWatcherFolders",
                columns: table => new
                {
                    WatchingPath = table.Column<string>(nullable: false),
                    InProcessPath = table.Column<string>(nullable: true),
                    ProcessedPath = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileWatcherFolders", x => x.WatchingPath);
                });

            migrationBuilder.CreateTable(
                name: "FileInstances",
                columns: table => new
                {
                    InstanceId = table.Column<string>(nullable: false),
                    BeginTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: true),
                    HashCode = table.Column<string>(nullable: true),
                    FileId = table.Column<int>(nullable: false),
                    Successful = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    SequenceNumber = table.Column<int>(nullable: true),
                    FileDate = table.Column<DateTime>(nullable: true),
                    FileDefinitionId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileInstances", x => x.InstanceId);
                    table.ForeignKey(
                        name: "FK_FileInstances_FileDefinitions_FileDefinitionId",
                        column: x => x.FileDefinitionId,
                        principalTable: "FileDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileInstances_FileDefinitionId",
                table: "FileInstances",
                column: "FileDefinitionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileEventLogs");

            migrationBuilder.DropTable(
                name: "FileInstances");

            migrationBuilder.DropTable(
                name: "FileProcessorConfigs");

            migrationBuilder.DropTable(
                name: "TypeMappings");

            migrationBuilder.DropTable(
                name: "FileWatcherFolders");

            migrationBuilder.DropTable(
                name: "FileDefinitions");
        }
    }
}
