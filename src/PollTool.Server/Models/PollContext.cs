using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace PollTool.Server.Models;

public partial class PollContext : DbContext
{
    public PollContext()
    {
    }

    public PollContext(DbContextOptions<PollContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Poll> Polls { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-HP;Database=Umfrage;Trusted_Connection=True;User ID=sa;Password=123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.ToTable("Answer");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.Text)
                .IsUnicode(false)
                .HasColumnName("Text");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
        });

        modelBuilder.Entity<Poll>(entity =>
        {
            entity.ToTable("Poll");

            entity.Property(e => e.PollId).HasColumnName("PollID");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Description).IsUnicode(false);
            entity.Property(e => e.Title).IsUnicode(false);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.PollId).HasColumnName("PollID");
            entity.Property(e => e.Title).IsUnicode(false);
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.ToTable("UserAnswer");

            entity.Property(e => e.UserAnswerId).HasColumnName("UserAnswerID");
            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.AnsweredBy)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
