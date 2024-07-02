﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NameGameCS;

#nullable disable

namespace NameGameCS.Migrations
{
    [DbContext(typeof(NameGameDbContext))]
    [Migration("20240629133820_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.6");

            modelBuilder.Entity("NameGameCS.Models.Answer", b =>
                {
                    b.Property<int>("answer_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("game_id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<int>("name_id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("round")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("success")
                        .HasColumnType("INTEGER");

                    b.Property<int>("team_id")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("time_finish")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("time_start")
                        .HasColumnType("TEXT");

                    b.Property<int>("user_inst_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("answer_id");

                    b.ToTable("Answers");
                });

            modelBuilder.Entity("NameGameCS.Models.DefaultName", b =>
                {
                    b.Property<int>("default_name_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.HasKey("default_name_id");

                    b.ToTable("DefaultNames");
                });

            modelBuilder.Entity("NameGameCS.Models.Game", b =>
                {
                    b.Property<int>("game_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("date_created")
                        .HasColumnType("TEXT");

                    b.Property<string>("game_name")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<bool>("is_active")
                        .HasColumnType("INTEGER");

                    b.Property<int>("number_names")
                        .HasColumnType("INTEGER");

                    b.Property<int>("number_teams")
                        .HasColumnType("INTEGER");

                    b.Property<int>("round")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("round1")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("round2")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("round3")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("round4")
                        .HasColumnType("INTEGER");

                    b.Property<int>("stage")
                        .HasColumnType("INTEGER");

                    b.Property<int>("time_limit_sec")
                        .HasColumnType("INTEGER");

                    b.HasKey("game_id");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("NameGameCS.Models.Mp3Order", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("current_start")
                        .HasColumnType("INTEGER");

                    b.Property<int>("current_stop")
                        .HasColumnType("INTEGER");

                    b.Property<int>("number_starts")
                        .HasColumnType("INTEGER");

                    b.Property<int>("number_stops")
                        .HasColumnType("INTEGER");

                    b.HasKey("id");

                    b.ToTable("Mp3Order");
                });

            modelBuilder.Entity("NameGameCS.Models.Name", b =>
                {
                    b.Property<int>("name_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("game_id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<int>("user_inst_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("name_id");

                    b.ToTable("Names");
                });

            modelBuilder.Entity("NameGameCS.Models.Team", b =>
                {
                    b.Property<int>("team_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("game_id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("order")
                        .HasColumnType("INTEGER");

                    b.Property<string>("team_name")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.HasKey("team_id");

                    b.ToTable("Teams");
                });

            modelBuilder.Entity("NameGameCS.Models.Turn", b =>
                {
                    b.Property<int>("turn_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("game_id")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("isActive")
                        .HasColumnType("INTEGER");

                    b.Property<int>("round")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("time_finish")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("time_start")
                        .HasColumnType("TEXT");

                    b.Property<int>("user_inst_id")
                        .HasColumnType("INTEGER");

                    b.HasKey("turn_id");

                    b.ToTable("Turns");
                });

            modelBuilder.Entity("NameGameCS.Models.User", b =>
                {
                    b.Property<int>("user_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("created")
                        .HasColumnType("TEXT");

                    b.Property<string>("last_ip")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("last_login")
                        .HasColumnType("TEXT");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.HasKey("user_id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("NameGameCS.Models.UserInstance", b =>
                {
                    b.Property<int>("user_inst_id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("game_id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("order")
                        .HasColumnType("INTEGER");

                    b.Property<int>("team_id")
                        .HasColumnType("INTEGER");

                    b.Property<int>("user_id")
                        .HasColumnType("INTEGER");

                    b.Property<string>("username")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.HasKey("user_inst_id");

                    b.ToTable("UserInstances");
                });
#pragma warning restore 612, 618
        }
    }
}
