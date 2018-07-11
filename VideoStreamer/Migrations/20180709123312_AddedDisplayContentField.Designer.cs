﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VideoStreamer.Db;

namespace VideoStreamer.Migrations
{
    [DbContext(typeof(StreamerContext))]
    [Migration("20180709123312_AddedDisplayContentField")]
    partial class AddedDisplayContentField
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846");

            modelBuilder.Entity("VideoStreamer.DB.StreamingSession", b =>
                {
                    b.Property<string>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Channel");

                    b.Property<bool>("DisplayContent");

                    b.Property<DateTime>("ExpireTime");

                    b.Property<int>("HlsListSize");

                    b.Property<string>("IP");

                    b.Property<int>("LastFileIndex");

                    b.Property<DateTime>("LastFileTimeSpan");

                    b.HasKey("ID");

                    b.ToTable("StreamingSessions");
                });
#pragma warning restore 612, 618
        }
    }
}
