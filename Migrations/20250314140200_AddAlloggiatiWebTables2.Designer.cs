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
    [Migration("20250314140200_AddAlloggiatiWebTables2")]
    partial class AddAlloggiatiWebTables2
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MAppBnB.Accommodation", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CIN")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CIR")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("CleaningFee")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Floor")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhonePrefix")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PostCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Province")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("TownFee")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("UnitApartment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Website")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("id");

                    b.ToTable("Accommodation");
                });

            modelBuilder.Entity("MAppBnB.BookChannel", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<decimal>("Fee")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("id");

                    b.ToTable("BookChannel");
                });

            modelBuilder.Entity("MAppBnB.Booking", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<int>("AccommodationID")
                        .HasColumnType("int");

                    b.Property<DateOnly>("BookingDate")
                        .HasColumnType("date");

                    b.Property<int>("ChannelID")
                        .HasColumnType("int");

                    b.Property<DateTime>("CheckOutDateTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("CheckinDateTime")
                        .HasColumnType("datetime2");

                    b.Property<bool>("ContractPrinted")
                        .HasColumnType("bit");

                    b.Property<decimal>("Discount")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<bool>("IsPaid")
                        .HasColumnType("bit");

                    b.Property<DateOnly>("PaymentDate")
                        .HasColumnType("date");

                    b.Property<decimal>("Price")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("RoomID")
                        .HasColumnType("int");

                    b.Property<bool>("Sent2Police")
                        .HasColumnType("bit");

                    b.Property<bool>("Sent2Region")
                        .HasColumnType("bit");

                    b.Property<bool>("Sent2Town")
                        .HasColumnType("bit");

                    b.HasKey("id");

                    b.ToTable("Booking");
                });

            modelBuilder.Entity("MAppBnB.BookingPerson", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<int>("BookingID")
                        .HasColumnType("int");

                    b.Property<int>("PersonID")
                        .HasColumnType("int");

                    b.HasKey("id");

                    b.ToTable("BookingPerson");
                });

            modelBuilder.Entity("MAppBnB.Document", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<int?>("DocumentType")
                        .HasColumnType("int");

                    b.Property<string>("IssuedBy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly?>("IssuedDate")
                        .HasColumnType("date");

                    b.Property<string>("IssuingCountry")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("PdfCopy")
                        .HasColumnType("varbinary(max)");

                    b.Property<int?>("PersonID")
                        .HasColumnType("int");

                    b.Property<string>("SerialNumber")
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("id");

                    b.ToTable("Document");
                });

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

            modelBuilder.Entity("MAppBnB.Person", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<string>("BirthCountry")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateOnly>("BirthDate")
                        .HasColumnType("date");

                    b.Property<string>("BirthPlace")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BirthProvince")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("DocumentID")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhonePrefix")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RoleRelation")
                        .HasColumnType("int");

                    b.Property<int?>("Sex")
                        .HasColumnType("int");

                    b.Property<string>("Surname")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("id");

                    b.ToTable("Person");
                });

            modelBuilder.Entity("MAppBnB.Room", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("id"));

                    b.Property<int>("AccommodationId")
                        .HasColumnType("int");

                    b.Property<decimal>("BasicPrice")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("Capacity")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.HasKey("id");

                    b.ToTable("Room");
                });
#pragma warning restore 612, 618
        }
    }
}
