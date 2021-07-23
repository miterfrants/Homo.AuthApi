﻿// <auto-generated />
using System;
using Homo.AuthApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AuthApi.Migrations
{
    [DbContext(typeof(DBContext))]
    [Migration("20210723195852_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.7");

            modelBuilder.Entity("Homo.AuthApi.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("City")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("County")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<long?>("CreatedBy")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("EditedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<long?>("EditedBy")
                        .HasColumnType("bigint");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("EncryptAddress")
                        .HasColumnType("longtext");

                    b.Property<string>("EncryptHomePhone")
                        .HasColumnType("longtext");

                    b.Property<string>("EncryptPhone")
                        .HasColumnType("longtext");

                    b.Property<string>("FacebookSub")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("FbSubDeletionConfirmCode")
                        .HasMaxLength(32)
                        .HasColumnType("varchar(32)");

                    b.Property<string>("FirstName")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<DateTime?>("ForgotPasswordAt")
                        .HasColumnType("datetime(6)");

                    b.Property<byte?>("Gender")
                        .HasMaxLength(1)
                        .HasColumnType("tinyint unsigned");

                    b.Property<string>("GoogleSub")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Hash")
                        .HasMaxLength(512)
                        .HasColumnType("varchar(512)");

                    b.Property<bool?>("IsManager")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool?>("IsSubscription")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("LastName")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("LineSub")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Profile")
                        .HasMaxLength(512)
                        .HasColumnType("varchar(512)");

                    b.Property<string>("PseudonymousAddress")
                        .HasColumnType("longtext");

                    b.Property<string>("PseudonymousHomePhone")
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("PseudonymousPhone")
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("Salt")
                        .HasMaxLength(512)
                        .HasColumnType("varchar(512)");

                    b.Property<bool>("Status")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Username")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<string>("Zip")
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)");

                    b.HasKey("Id");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Homo.AuthApi.VerifyCode", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(12)
                        .HasColumnType("varchar(12)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasColumnType("longtext");

                    b.Property<DateTime>("Expiration")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Ip")
                        .HasMaxLength(128)
                        .HasColumnType("varchar(128)");

                    b.Property<bool?>("IsUsed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Msgid")
                        .HasColumnType("longtext");

                    b.Property<string>("Phone")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("VerifyCode");
                });
#pragma warning restore 612, 618
        }
    }
}
