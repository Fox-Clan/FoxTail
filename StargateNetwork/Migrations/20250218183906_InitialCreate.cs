using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StargateNetwork.Types;

#nullable disable

namespace StargateNetwork.Migrations
{
    [DbContext(typeof(StargateContext))]
    [Migration("20250218183906_InitialCreate")]
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stargates",
                columns: table => new
                {
                    id = table.Column<string>(type: "TEXT", nullable: false),
                    active_users = table.Column<int>(type: "INTEGER", nullable: false),
                    gate_address = table.Column<string>(type: "TEXT", nullable: false),
                    gate_code = table.Column<string>(type: "TEXT", nullable: false),
                    gate_status = table.Column<string>(type: "TEXT", nullable: false),
                    iris_state = table.Column<string>(type: "TEXT", nullable: false),
                    is_headless = table.Column<bool>(type: "INTEGER", nullable: false),
                    max_users = table.Column<int>(type: "INTEGER", nullable: false),
                    owner_name = table.Column<string>(type: "TEXT", nullable: false),
                    public_gate = table.Column<bool>(type: "INTEGER", nullable: false),
                    session_name = table.Column<string>(type: "TEXT", nullable: false),
                    session_url = table.Column<string>(type: "TEXT", nullable: false),
                    update_date = table.Column<int>(type: "INTEGER", nullable: false),
                    creation_date = table.Column<int>(type: "INTEGER", nullable: false),
                    dialed_gate_id = table.Column<string>(type: "TEXT", nullable: false),
                    is_persistent = table.Column<bool>(type: "INTEGER", nullable: false),
                    world_record = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stargates", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Stargates");
        }
    }
}
