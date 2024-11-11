using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DigitalBlog.Models;

public partial class DigitalBlogContext : DbContext
{
    public DigitalBlogContext()
    {
    }

    public DigitalBlogContext(DbContextOptions<DigitalBlogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogSubscription> BlogSubscriptions { get; set; }

    public virtual DbSet<UserList> UserLists { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Bid).HasName("PK__Blog__C6DE0CC162E02702");

            entity.ToTable("Blog");

            entity.Property(e => e.Bid)
                .ValueGeneratedNever()
                .HasColumnName("BId");
            entity.Property(e => e.Amount).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Bdescription).HasColumnName("BDescription");
            entity.Property(e => e.Bstatus)
                .HasMaxLength(50)
                .HasColumnName("BStatus");

            entity.HasOne(d => d.User).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Blog__UserId__4CA06362");
        });

        modelBuilder.Entity<BlogSubscription>(entity =>
        {
            entity.HasKey(e => e.SubId).HasName("PK__BlogSubs__4D9BB84A77E59BF9");

            entity.ToTable("BlogSubscription");

            entity.Property(e => e.SubId).ValueGeneratedNever();
            entity.Property(e => e.Bid).HasColumnName("BId");
            entity.Property(e => e.SubAmount).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.BlogSubscriptions)
                .HasForeignKey(d => d.Bid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogSubscri__BId__52593CB8");

            entity.HasOne(d => d.User).WithMany(p => p.BlogSubscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogSubsc__UserI__5165187F");
        });

        modelBuilder.Entity<UserList>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserList__1788CC4C5DEF99EE");

            entity.ToTable("UserList");

            entity.HasIndex(e => e.EmailAddress, "UQ__UserList__49A14740ECC4B2DC").IsUnique();

            entity.HasIndex(e => e.Phone, "UQ__UserList__5C7E359E60A5E765").IsUnique();

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.EmailAddress).HasMaxLength(40);
            entity.Property(e => e.FullName).HasMaxLength(50);
            entity.Property(e => e.LoginName).HasMaxLength(30);
            entity.Property(e => e.LoginStatus).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.UserRole).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
