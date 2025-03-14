﻿// <auto-generated />
using System;
using MAppBnB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MAppBnB.Migrations
{
    [DbContext(typeof(MappBnBContext))]
    [Migration("20250314185307_AlloggiatiWebTables")]
    partial class AlloggiatiWebTables
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MAppBnB.Models.Comuni", b =>
                {
                    b.Property<string>("Codice")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("DataFineVal")
                        .HasColumnType("datetime2");

                    b.Property<string>("Descrizione")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Provincia")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Codice");

                    b.ToTable("Comuni");
                });

            modelBuilder.Entity("MAppBnB.Models.Stati", b =>
                {
                    b.Property<string>("Codice")
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("DataFineVal")
                        .HasColumnType("datetime2");

                    b.Property<string>("Descrizione")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Provincia")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Codice");

                    b.ToTable("Stati");
                });

            modelBuilder.Entity("MAppBnB.Models.TipoAlloggiato", b =>
                {
                    b.Property<string>("Codice")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Descrizione")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Codice");

                    b.ToTable("TipoAlloggiato");
                });

            modelBuilder.Entity("MAppBnB.Models.TipoDocumento", b =>
                {
                    b.Property<string>("Codice")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Descrizione")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Codice");

                    b.ToTable("TipoDocumento");
                });


#pragma warning restore 612, 618
        }
    }
}
