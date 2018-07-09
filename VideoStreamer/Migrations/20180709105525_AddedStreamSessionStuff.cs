using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoStreamer.Migrations
{
    public partial class AddedStreamSessionStuff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpireTime",
                table: "StreamingSessions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "HlsListSize",
                table: "StreamingSessions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IP",
                table: "StreamingSessions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastFileIndex",
                table: "StreamingSessions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFileTimeSpan",
                table: "StreamingSessions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpireTime",
                table: "StreamingSessions");

            migrationBuilder.DropColumn(
                name: "HlsListSize",
                table: "StreamingSessions");

            migrationBuilder.DropColumn(
                name: "IP",
                table: "StreamingSessions");

            migrationBuilder.DropColumn(
                name: "LastFileIndex",
                table: "StreamingSessions");

            migrationBuilder.DropColumn(
                name: "LastFileTimeSpan",
                table: "StreamingSessions");
        }
    }
}
