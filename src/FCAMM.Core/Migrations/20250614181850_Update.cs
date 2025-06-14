using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FCAMM.Core.Migrations
{
    /// <inheritdoc />
    public partial class Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_AutorId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Categorias_CategoriaId",
                table: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Perfis",
                table: "Perfis");

            migrationBuilder.DropIndex(
                name: "IX_Perfis_UsuarioId",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "AprovadoPorId",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CategoriaModelId",
                table: "Posts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAprovacao",
                table: "Posts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacoesAprovacao",
                table: "Posts",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Perfis",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Perfis",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataNomeacao",
                table: "Perfis",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Especialidade",
                table: "Perfis",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalTextos",
                table: "Perfis",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalVisualizacoes",
                table: "Perfis",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Perfis",
                table: "Perfis",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_AprovadoPorId",
                table: "Posts",
                column: "AprovadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_CategoriaModelId",
                table: "Posts",
                column: "CategoriaModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_DataAprovacao",
                table: "Posts",
                column: "DataAprovacao");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_DataPublicacao",
                table: "Posts",
                column: "DataPublicacao");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Status",
                table: "Posts",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Perfis_Especialidade",
                table: "Perfis",
                column: "Especialidade");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Role",
                table: "AspNetUsers",
                column: "Role");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_AprovadoPorId",
                table: "Posts",
                column: "AprovadoPorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_AutorId",
                table: "Posts",
                column: "AutorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categorias_CategoriaId",
                table: "Posts",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categorias_CategoriaModelId",
                table: "Posts",
                column: "CategoriaModelId",
                principalTable: "Categorias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_AprovadoPorId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_AutorId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Categorias_CategoriaId",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Categorias_CategoriaModelId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_AprovadoPorId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_CategoriaModelId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_DataAprovacao",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_DataPublicacao",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Status",
                table: "Posts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Perfis",
                table: "Perfis");

            migrationBuilder.DropIndex(
                name: "IX_Perfis_Especialidade",
                table: "Perfis");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Role",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "AprovadoPorId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "CategoriaModelId",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "DataAprovacao",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "ObservacoesAprovacao",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "DataNomeacao",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "Especialidade",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "TotalTextos",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "TotalVisualizacoes",
                table: "Perfis");

            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Perfis",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Perfis",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Perfis",
                table: "Perfis",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Perfis_UsuarioId",
                table: "Perfis",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_AspNetUsers_AutorId",
                table: "Posts",
                column: "AutorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Categorias_CategoriaId",
                table: "Posts",
                column: "CategoriaId",
                principalTable: "Categorias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
