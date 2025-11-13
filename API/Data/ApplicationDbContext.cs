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
    public virtual DbSet<password_reset_tokens> password_reset_tokens { get; set; }
    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<Asset> Assets { get; set; }
    public virtual DbSet<AssetCategory> AssetCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Check if we're using in-memory database
        var isInMemory = Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

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

            // For in-memory database, use bool converter for is_confirmed
            if (isInMemory)
            {
                entity.Property(e => e.is_confirmed)
                    .HasConversion(
                        v => v != null && v.Length > 0 && v[0], // BitArray to bool
                        v => v ? new System.Collections.BitArray(new[] { true }) : new System.Collections.BitArray(new[] { false }) // bool to BitArray
                    );
            }
            else
            {
                entity.Property(e => e.is_confirmed).HasColumnType("bit(1)");
            }

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

        // OTP Codes Configuration
        // Foreign key constraint to pending_users with CASCADE delete
        // OTP codes are linked to pending user registrations
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

            // Foreign key relationship to pending_users
            entity.HasOne<pending_users>()
                .WithMany()
                .HasForeignKey(e => e.email)
                .HasPrincipalKey(p => p.email)
                .HasConstraintName("fk_otp_email")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Password Reset Tokens Configuration
        // Used for password reset functionality for confirmed users
        modelBuilder.Entity<password_reset_tokens>(entity =>
        {
            entity.HasKey(e => e.id).HasName("password_reset_tokens_pkey");

            entity.ToTable("password_reset_tokens", _schemaName);

            entity.HasIndex(e => e.email, "idx_password_reset_email");
            entity.HasIndex(e => e.token, "idx_password_reset_token");

            entity.Property(e => e.email).HasMaxLength(255);
            entity.Property(e => e.token).HasMaxLength(255);
            entity.Property(e => e.created_at)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.expires_at)
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.is_used).HasDefaultValue(false);

            // Foreign key relationship to user_login
            entity.HasOne<user_login>()
                .WithMany()
                .HasForeignKey(e => e.email)
                .HasPrincipalKey(u => u.email)
                .HasConstraintName("fk_password_reset_email")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Rooms Configuration
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("rooms_pkey");

            entity.ToTable("rooms", _schemaName);

            entity.HasIndex(e => e.UserId, "idx_rooms_user_id");
            entity.HasIndex(e => new { e.UserId, e.CreatedAt }, "idx_rooms_user_created");

            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.LengthM).HasColumnType("decimal(5,2)");
            entity.Property(e => e.WidthM).HasColumnType("decimal(5,2)");
            entity.Property(e => e.DoorPosition).HasMaxLength(20);
            entity.Property(e => e.WindowPosition).HasMaxLength(20);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            // Foreign key relationship to user_login
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("fk_rooms_user")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Assets Configuration
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("assets_pkey");

            entity.ToTable("assets", _schemaName);

            entity.HasIndex(e => e.RoomId, "idx_assets_room_id");
            entity.HasIndex(e => e.UserId, "idx_assets_user_id");
            entity.HasIndex(e => new { e.RoomId, e.Category }, "idx_assets_room_category");
            entity.HasIndex(e => new { e.RoomId, e.CreatedAt }, "idx_assets_room_created");

            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.FunctionZone).HasMaxLength(50);
            entity.Property(e => e.Condition).HasMaxLength(50);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(12,2)");
            entity.Property(e => e.PurchaseDate).HasColumnType("date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");

            // Foreign key relationship to rooms
            entity.HasOne(e => e.Room)
                .WithMany(r => r.Assets)
                .HasForeignKey(e => e.RoomId)
                .HasConstraintName("fk_assets_room")
                .OnDelete(DeleteBehavior.Cascade);

            // Foreign key relationship to user_login
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .HasConstraintName("fk_assets_user")
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Asset Categories Configuration
        modelBuilder.Entity<AssetCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("asset_categories_pkey");

            entity.ToTable("asset_categories", _schemaName);

            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
