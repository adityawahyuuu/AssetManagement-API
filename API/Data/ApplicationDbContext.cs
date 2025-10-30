using API.Configuration;
using API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace API.Data;

public partial class ApplicationDbContext : DbContext
{
    private readonly string _schemaName;

    public ApplicationDbContext()
    {
        _schemaName = "kosan"; // Default fallback
    }

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IOptions<DatabaseOptions> databaseOptions)
        : base(options)
    {
        _schemaName = databaseOptions.Value.SchemaName;
    }

    public virtual DbSet<user_login> user_logins { get; set; }
    public virtual DbSet<pending_users> pending_users { get; set; }
    public virtual DbSet<otp_codes> otp_codes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<user_login>(entity =>
        {
            entity.HasKey(e => e.userid).HasName("users_pkey");

            entity.ToTable("user_login", _schemaName);

            entity.HasIndex(e => e.email, "idx_user_email").IsUnique();

            entity.HasIndex(e => e.email, "users_email_key").IsUnique();

            entity.Property(e => e.userid)
                .HasDefaultValueSql($"nextval('{_schemaName}.users_userid_seq'::regclass)");
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.is_confirmed).HasColumnType("bit(1)");
            entity.Property(e => e.password_hash).HasMaxLength(255);
            entity.Property(e => e.updated_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.username).HasMaxLength(255);
        });

        modelBuilder.Entity<pending_users>(entity =>
        {
            entity.HasKey(e => e.id).HasName("pending_users_pkey");

            entity.ToTable("pending_users", _schemaName);

            entity.HasIndex(e => e.email, "pending_users_email_key").IsUnique();

            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.username).HasMaxLength(255);
            entity.Property(e => e.password_hash).HasMaxLength(255);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.expires_at)
                .HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<otp_codes>(entity =>
        {
            entity.HasKey(e => e.id).HasName("otp_codes_pkey");

            entity.ToTable("otp_codes", _schemaName);

            entity.HasIndex(e => e.email, "idx_otp_codes_email");
            entity.HasIndex(e => e.otp_code, "idx_otp_codes_otp");

            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.otp_code).HasMaxLength(6);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.expires_at)
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_verified).HasDefaultValue(false);
            entity.Property(e => e.attempts).HasDefaultValue(0);
            entity.Property(e => e.max_attempts).HasDefaultValue(5);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
