﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace StargateNetwork.Migrations
{
    [DbContext(typeof(StargateContext))]
    [Migration("20250218183906_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.1");

            modelBuilder.Entity("Stargate", b =>
                {
                    b.Property<string>("id")
                        .HasColumnType("TEXT");

                    b.Property<int>("active_users")
                        .HasColumnType("INTEGER");

                    b.Property<int>("creation_date")
                        .HasColumnType("INTEGER");

                    b.Property<string>("dialed_gate_id")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("gate_address")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("gate_code")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("gate_status")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("iris_state")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("is_headless")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("is_persistent")
                        .HasColumnType("INTEGER");

                    b.Property<int>("max_users")
                        .HasColumnType("INTEGER");

                    b.Property<string>("owner_name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("public_gate")
                        .HasColumnType("INTEGER");

                    b.Property<string>("session_name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("session_url")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("update_date")
                        .HasColumnType("INTEGER");

                    b.Property<string>("world_record")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.ToTable("Stargates");
                });
#pragma warning restore 612, 618
        }
    }
}
