using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VehiclePart.Infrastructure.Data;

#nullable disable

namespace VehiclePart.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260428135541_AddVendorIdToParts")]
    partial class AddVendorIdToParts
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("VehiclePart.Domain.Entities.Appointment", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("AppointmentDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<string>("Notes")
                        .HasColumnType("text");

                    b.Property<string>("ServiceType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("Appointments");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.Part", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PartNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("QuantityInStock")
                        .HasColumnType("integer");

                    b.Property<decimal>("UnitPrice")
                        .HasColumnType("numeric");

                    b.Property<Guid>("VendorId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("Parts");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.PartRequest", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("PartName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("PartRequests");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.PurchaseInvoice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("IssuedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("numeric");

                    b.Property<Guid>("VendorId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.ToTable("PurchaseInvoices");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.SalesInvoice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("DiscountAmount")
                        .HasColumnType("numeric");

                    b.Property<DateTime>("IssuedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("PaidAmount")
                        .HasColumnType("numeric");

                    b.Property<decimal>("PendingCredit")
                        .HasColumnType("numeric");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.ToTable("SalesInvoices");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.ServiceReview", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Comment")
                        .HasColumnType("text");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<int>("Rating")
                        .HasColumnType("integer");

                    b.Property<Guid>("ServiceId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CustomerId");

                    b.ToTable("ServiceReviews");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Role")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.Vehicle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uuid");

                    b.Property<string>("Make")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VehicleNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Year")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Vehicles");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.Vendor", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("ContactPerson")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Vendors");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.Appointment", b =>
                {
                    b.HasOne("VehiclePart.Domain.Entities.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.PartRequest", b =>
                {
                    b.HasOne("VehiclePart.Domain.Entities.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("VehiclePart.Domain.Entities.ServiceReview", b =>
                {
                    b.HasOne("VehiclePart.Domain.Entities.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });
#pragma warning restore 612, 618
        }
    }
}
