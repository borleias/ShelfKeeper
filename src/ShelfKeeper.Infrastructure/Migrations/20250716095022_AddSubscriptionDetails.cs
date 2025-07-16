using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShelfKeeper.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "plan",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "subscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "stripe_customer_id",
                table: "subscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "stripe_subscription_id",
                table: "subscriptions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "stripe_customer_id",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "stripe_subscription_id",
                table: "subscriptions");

            migrationBuilder.AlterColumn<string>(
                name: "plan",
                table: "subscriptions",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
