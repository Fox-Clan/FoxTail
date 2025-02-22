using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using StargateNetwork.Types;

#nullable disable

namespace StargateNetwork.Migrations
{
    [DbContext(typeof(StargateContext))]
    [Migration("20250222054602_InitialCreate")]
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
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ActiveUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    GateAddress = table.Column<string>(type: "TEXT", nullable: true),
                    GateCode = table.Column<string>(type: "TEXT", nullable: true),
                    GateStatus = table.Column<string>(type: "TEXT", nullable: true),
                    IrisState = table.Column<string>(type: "TEXT", nullable: true),
                    IsHeadless = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxUsers = table.Column<int>(type: "INTEGER", nullable: false),
                    OwnerName = table.Column<string>(type: "TEXT", nullable: true),
                    PublicGate = table.Column<bool>(type: "INTEGER", nullable: false),
                    SessionName = table.Column<string>(type: "TEXT", nullable: true),
                    SessionUrl = table.Column<string>(type: "TEXT", nullable: true),
                    UpdateDate = table.Column<long>(type: "INTEGER", nullable: false),
                    CreationDate = table.Column<long>(type: "INTEGER", nullable: false),
                    DialedGateId = table.Column<string>(type: "TEXT", nullable: true),
                    IsPersistent = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorldRecord = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stargates", x => x.Id);
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
