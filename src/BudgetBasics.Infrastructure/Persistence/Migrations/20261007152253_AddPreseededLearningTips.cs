using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BudgetBasics.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPreseededLearningTips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LearningTips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HabitKey = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTips", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LearningTips",
                columns: new[] { "Id", "HabitKey", "IsActive", "Message", "SortOrder", "Title" },
                values: new object[,]
                {
                    { 1, "all", true, "Choose one fixed day to review what came in, what went out, and what needs attention next week.", 10, "Use a weekly money check-in" },
                    { 2, "all", true, "Move even a small planned amount to savings as soon as your income arrives instead of waiting for leftovers.", 20, "Save before spending" },
                    { 3, "food", true, "Give snacks and takeaway a weekly amount. When it is used, choose a home-prepared option until the next week.", 30, "Set a snack limit" },
                    { 4, "food", true, "Keep ingredients for two low-cost meals ready so an unplanned takeaway is less tempting on busy days.", 40, "Plan two easy meals" },
                    { 5, "transport", true, "Compare passes, shared rides, fuel, and parking over a whole week rather than judging one trip at a time.", 50, "Compare the full weekly cost" },
                    { 6, "transport", true, "Group errands in the same area and check safe public transport options to reduce repeated travel costs.", 60, "Combine nearby trips" },
                    { 7, "impulse", true, "Save the item instead of buying immediately. Recheck it tomorrow and confirm it still fits your wants budget.", 70, "Try the 24-hour pause" },
                    { 8, "impulse", true, "Delete saved card details and promotional alerts so every purchase becomes a deliberate decision.", 80, "Remove one-click temptation" },
                    { 9, "subscriptions", true, "List every recurring charge, its next billing date, and the last time you used it. Cancel what no longer helps.", 90, "Run a monthly subscription audit" },
                    { 10, "subscriptions", true, "Keep one entertainment service at a time and rotate only after the current billing period ends.", 100, "Avoid overlapping services" },
                    { 11, "saving", true, "A specific goal and date make progress easier to track than a general intention to save more.", 110, "Name your savings goal" },
                    { 12, "saving", true, "Build a small first target for unexpected transport, medicine, or study costs, then increase it gradually.", 120, "Start an emergency buffer" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningTips_HabitKey_IsActive",
                table: "LearningTips",
                columns: new[] { "HabitKey", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningTips");
        }
    }
}
