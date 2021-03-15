// <copyright file="TimesheetContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Teams.Apps.Timesheet.Common.Repositories
{
    using System;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Teams.Apps.Timesheet.Common.Models;

    /// <summary>
    /// The Db context which holds the entities.
    /// </summary>
    public partial class TimesheetContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimesheetContext"/> class.
        /// </summary>
        public TimesheetContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimesheetContext"/> class.
        /// </summary>
        /// <param name="options">The Db context options.</param>
        public TimesheetContext(DbContextOptions<TimesheetContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Db set for conversation entity.
        /// </summary>
        public virtual DbSet<Conversation> Conversations { get; set; }

        /// <summary>
        /// Gets or sets the Db set for members entity.
        /// </summary>
        public virtual DbSet<Member> Members { get; set; }

        /// <summary>
        /// Gets or sets the Db set for project entity.
        /// </summary>
        public virtual DbSet<Project> Projects { get; set; }

        /// <summary>
        /// Gets or sets the Db set for task entity.
        /// </summary>
        public virtual DbSet<TaskEntity> Tasks { get; set; }

        /// <summary>
        /// Gets or sets the Db set for timesheet entity.
        /// </summary>
        public virtual DbSet<TimesheetEntity> Timesheets { get; set; }

        /// <summary>
        /// The event called when model is being prepared.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder = modelBuilder ?? throw new ArgumentNullException(nameof(modelBuilder), "Model builder cannot be null");

            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK__Conversa__1788CC4C7C76A5E6");

                entity.ToTable("Conversation");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.BotInstalledOn).HasColumnType("datetime");

                entity.Property(e => e.ConversationId)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.ServiceUrl)
                    .IsRequired()
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.ToTable("Member");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Members)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Member__Project");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("Project");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.ClientName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.EndDate).HasColumnType("date");

                entity.Property(e => e.StartDate).HasColumnType("date");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TaskEntity>(entity =>
            {
                entity.ToTable("Task");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Task__Project");
            });

            modelBuilder.Entity<TimesheetEntity>(entity =>
            {
                entity.ToTable("TimesheetEntity");

                entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

                entity.Property(e => e.LastModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.ManagerComments)
                    .HasMaxLength(100);

                entity.Property(e => e.SubmittedOn).HasColumnType("datetime");

                entity.Property(e => e.TaskTitle)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.TimesheetDate).HasColumnType("date");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.Timesheets)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Timesheet__Task");
            });

            this.OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}