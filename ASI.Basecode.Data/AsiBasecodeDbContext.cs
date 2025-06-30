using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDBContext : DbContext
    {
        public AsiBasecodeDBContext()
        {
        }

        public AsiBasecodeDBContext(DbContextOptions<AsiBasecodeDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Duration> Durations { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<SkillLevel> SkillLevels { get; set; }
        public virtual DbSet<Topic> Topics { get; set; }
        public virtual DbSet<TopicMedium> TopicMedia { get; set; }
        public virtual DbSet<Training> Trainings { get; set; }
        public virtual DbSet<TrainingCategory> TrainingCategories { get; set; }
        public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.EmailId, "UQ__Accounts__1788CC4D5F4A160F")
                    .IsUnique();

                entity.Property(e => e.EmailId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Contact)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Birthdate)
                    .HasColumnType("date");

                entity.Property(e => e.ProfilePicture)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime)
                    .HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime)
                    .HasColumnType("datetime");

                entity.HasOne(d => d.RoleNavigation).WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.Role)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Accounts_Roles");
            });

            modelBuilder.Entity<Duration>(entity =>
            {
                entity.ToTable("Duration");

                entity.Property(e => e.Duration1)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("Duration");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SkillLevel>(entity =>
            {
                entity.ToTable("SkillLevel");

                entity.Property(e => e.SkillLevel1)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("SkillLevel");
            });

            modelBuilder.Entity<Training>(entity =>
            {
                entity.Property(e => e.CourseCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CoverPicture)
                    .HasMaxLength(2083)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.TrainingName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Account).WithMany(p => p.Training)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Trainings_Accounts");

                entity.HasOne(d => d.DurationNavigation).WithMany(p => p.Training)
                    .HasForeignKey(d => d.Duration)
                    .HasConstraintName("FK_Trainings_Duration");

                entity.HasOne(d => d.SkillLevelNavigation).WithMany(p => p.Training)
                    .HasForeignKey(d => d.SkillLevel)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Trainings_SkillLevel");

                entity.HasOne(d => d.TrainingCategory).WithMany(p => p.Training)
                    .HasForeignKey(d => d.TrainingCategoryId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Trainings_TrainingCategories");
            });

            modelBuilder.Entity<TrainingCategory>(entity =>
            {
                entity.ToTable("TrainingCategories");

                entity.HasKey(e => e.Id).HasName("PK__Training__3214EC0770533F47");

                entity.Property(e => e.CategoryName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CoverPicture)
                    .HasMaxLength(2083)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Account).WithMany(p => p.TrainingCategories)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("Training Category Owner");
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Table_1");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsUnicode(false);

                entity.Property(e => e.TopicName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Account).WithMany(p => p.Topics)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Table_1_Accounts");

                entity.HasOne(d => d.Training).WithMany(p => p.Topics)
                    .HasForeignKey(d => d.TrainingId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Table_1_Trainings");
            });

            modelBuilder.Entity<TopicMedium>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Table_2");

                entity.Property(e => e.Name)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.MediaType)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.MediaUrl)
                    .HasMaxLength(2083)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.HasOne(d => d.Account).WithMany(p => p.TopicMedia)
                    .HasForeignKey(d => d.AccountId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Table_2_Accounts");

                entity.HasOne(d => d.Topic).WithMany(p => p.TopicMedia)
                    .HasForeignKey(d => d.TopicId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_Table_2_Table_1");
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.ToTable("PasswordResetTokens");

                entity.HasKey(e => e.TokenId);

                entity.Property(e => e.Token)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(e => e.ExpirationTime)
                    .IsRequired();

                entity.Property(e => e.IsUsed)
                    .IsRequired();

                entity.HasOne(e => e.Account)
                    .WithMany(p => p.PasswordResetTokens)
                    .HasForeignKey(e => e.AccountId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_PasswordResetTokens_Accounts");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
