using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExamBurcu.Migrations
{
    /// <inheritdoc />
    public partial class InitialCodeFirstMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "child",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    tckn = table.Column<long>(type: "bigint", nullable: false),
                    namesurname = table.Column<string>(type: "character varying", maxLength: 80, nullable: true),
                    birthdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("child_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "doctor",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    tckn = table.Column<long>(type: "bigint", nullable: false),
                    namesurname = table.Column<string>(type: "character varying", maxLength: 80, nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("doctor_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vaccine",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    code = table.Column<string>(type: "character varying", maxLength: 70, nullable: false),
                    name = table.Column<string>(type: "character varying", maxLength: 50, nullable: true),
                    stockcount = table.Column<int>(type: "integer", nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("vaccine_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vaccineapplication",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    applicationtime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vaccineid = table.Column<long>(type: "bigint", nullable: true),
                    doctorid = table.Column<long>(type: "bigint", nullable: true),
                    childid = table.Column<long>(type: "bigint", nullable: true),
                    description = table.Column<string>(type: "character varying", maxLength: 100, nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("vaccineapplication_pk", x => x.id);
                    table.ForeignKey(
                        name: "vaccineapplication_child_fk",
                        column: x => x.childid,
                        principalTable: "child",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "vaccineapplication_doctor_fk",
                        column: x => x.doctorid,
                        principalTable: "doctor",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "vaccineapplication_vaccine_fk",
                        column: x => x.vaccineid,
                        principalTable: "vaccine",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vaccineschedule",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    vaccineid = table.Column<long>(type: "bigint", nullable: true),
                    month = table.Column<short>(type: "smallint", nullable: true),
                    isdeleted = table.Column<bool>(type: "boolean", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: false),
                    createddate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("vaccineschedule_pk", x => x.id);
                    table.ForeignKey(
                        name: "vaccineschedule_vaccine_fk",
                        column: x => x.vaccineid,
                        principalTable: "vaccine",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "child_unique",
                table: "child",
                column: "tckn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_child_tckn",
                table: "child",
                column: "tckn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "doctor_unique",
                table: "doctor",
                column: "tckn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_doctor_tckn",
                table: "doctor",
                column: "tckn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vaccine_code",
                table: "vaccine",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "vaccine_unique",
                table: "vaccine",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vaccineapplication_childid",
                table: "vaccineapplication",
                column: "childid");

            migrationBuilder.CreateIndex(
                name: "IX_vaccineapplication_doctorid",
                table: "vaccineapplication",
                column: "doctorid");

            migrationBuilder.CreateIndex(
                name: "IX_vaccineapplication_vaccineid",
                table: "vaccineapplication",
                column: "vaccineid");

            migrationBuilder.CreateIndex(
                name: "IX_vaccineschedule_month_vaccineid",
                table: "vaccineschedule",
                columns: new[] { "month", "vaccineid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "vaccineschedule_unique",
                table: "vaccineschedule",
                columns: new[] { "vaccineid", "month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vaccineapplication");

            migrationBuilder.DropTable(
                name: "vaccineschedule");

            migrationBuilder.DropTable(
                name: "child");

            migrationBuilder.DropTable(
                name: "doctor");

            migrationBuilder.DropTable(
                name: "vaccine");
        }
    }
}
