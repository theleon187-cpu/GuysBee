using GuysBee.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GuysBee.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<SalePoint> SalePoints => Set<SalePoint>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
    public DbSet<ShippingRate> ShippingRates => Set<ShippingRate>();
    public DbSet<ContentPage> ContentPages => Set<ContentPage>();
    public DbSet<HomeSection> HomeSections => Set<HomeSection>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<HiveCalendarEntry> HiveCalendarEntries => Set<HiveCalendarEntry>();
    public DbSet<GlossaryTerm> GlossaryTerms => Set<GlossaryTerm>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Category>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Product>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<Post>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<MediaAsset>().HasIndex(x => x.SourceFileName).IsUnique();
        b.Entity<SiteSetting>().HasIndex(x => x.Key).IsUnique();
        b.Entity<Order>().HasIndex(x => x.Number).IsUnique();
        b.Entity<ContentPage>().HasIndex(x => x.Key).IsUnique();
        b.Entity<ContentPage>().HasIndex(x => x.Slug).IsUnique();
        b.Entity<EmailTemplate>().HasIndex(x => x.Key).IsUnique();
        b.Entity<GlossaryTerm>().HasIndex(x => x.Term).IsUnique();
        b.Entity<ContentPage>().HasOne(x => x.HeroMedia).WithMany().HasForeignKey(x => x.HeroMediaId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<HomeSection>().HasOne(x => x.Media).WithMany().HasForeignKey(x => x.MediaId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<HiveCalendarEntry>().HasOne(x => x.Media).WithMany().HasForeignKey(x => x.MediaId).OnDelete(DeleteBehavior.SetNull);

        b.Entity<ProductVariant>().Property(x => x.Price).HasPrecision(18, 2);
        b.Entity<OrderLine>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Entity<Order>().Property(x => x.ItemsTotal).HasPrecision(18, 2);
        b.Entity<Order>().Property(x => x.ShippingCost).HasPrecision(18, 2);
        b.Entity<Order>().Property(x => x.Total).HasPrecision(18, 2);
        b.Entity<ShippingMethod>().Property(x => x.FreeAboveAmount).HasPrecision(18, 2);
        b.Entity<ShippingZone>().Property(x => x.FlatPrice).HasPrecision(18, 2);
        b.Entity<ShippingRate>().Property(x => x.Price).HasPrecision(18, 2);

        b.Entity<Product>().HasOne(x => x.MainMedia).WithMany().HasForeignKey(x => x.MainMediaId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<Product>().HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<Post>().HasOne(x => x.CoverMedia).WithMany().HasForeignKey(x => x.CoverMediaId).OnDelete(DeleteBehavior.SetNull);
        b.Entity<Order>().HasOne(x => x.ShippingMethod).WithMany().HasForeignKey(x => x.ShippingMethodId).OnDelete(DeleteBehavior.SetNull);
    }
}
