using Microsoft.EntityFrameworkCore.Migrations;

namespace VideoStreamer.Migrations
{
    public partial class AddedDisplayContentField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DisplayContent",
                table: "StreamingSessions",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayContent",
                table: "StreamingSessions");
        }
    }
}
