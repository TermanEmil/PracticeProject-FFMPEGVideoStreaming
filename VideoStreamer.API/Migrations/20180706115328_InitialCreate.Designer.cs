﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VideoStreamer.Db;

namespace VideoStreamer.Migrations
{
    [DbContext(typeof(StreamerContext))]
    [Migration("20180706115328_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("VideoStreamer.Db.StreamingSession", b =>
                {
                    b.Property<string>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Channel");

                    b.HasKey("ID");

                    b.ToTable("StreamingSessions");
                });
#pragma warning restore 612, 618
        }
    }
}