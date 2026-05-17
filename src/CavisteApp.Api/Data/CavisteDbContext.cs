using CavisteApp.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CavisteApp.Api.Data;

public class CavisteDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public DbSet<Vin> Vins => Set<Vin>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();
    public DbSet<Commande> Commandes => Set<Commande>();
    public DbSet<LigneCommande> LignesCommande => Set<LigneCommande>();
    public DbSet<Vente> Ventes => Set<Vente>();
    public DbSet<LigneVente> LignesVente => Set<LigneVente>();

    public CavisteDbContext(DbContextOptions<CavisteDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Vin
        modelBuilder.Entity<Vin>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Nom).IsRequired().HasMaxLength(100);
            entity.Property(v => v.Prix).HasColumnType("decimal(18,2)");
        });

        // Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Nom).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Prenom).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Telephone).HasMaxLength(20);
            entity.Property(c => c.NumRue).HasMaxLength(100);
            entity.Property(c => c.NomRue).HasMaxLength(200);
            entity.Property(c => c.CodePostal).HasMaxLength(5);
            entity.Property(c => c.Ville).HasMaxLength(100);

            entity.HasIndex(c => c.Email).IsUnique();
        });

        // Vente
        modelBuilder.Entity<Vente>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Date).ValueGeneratedOnAdd();
            entity.Property(v => v.MontantTotal).HasColumnType("decimal(18,2)");

            // Statut + motif d'annulation
            entity.Property(v => v.Statut)
                .HasConversion<string>()      // stocké en string : "Brouillon", "Validee", "Annulee"
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(v => v.MotifAnnulation).HasMaxLength(500);

            entity.HasOne(v => v.Client)
                .WithMany(c => c.Ventes)
                .HasForeignKey(v => v.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.Utilisateur)
                .WithMany()
                .HasForeignKey(v => v.UtilisateurId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index utiles
            entity.HasIndex(v => v.Date);
            entity.HasIndex(v => v.Statut);
        });

        //  LigneVente
        modelBuilder.Entity<LigneVente>(entity =>
        {
            entity.HasKey(l => l.Id);

            // Snapshot
            entity.Property(l => l.VinNom).IsRequired().HasMaxLength(100);
            entity.Property(l => l.Type)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(l => l.Quantite).HasDefaultValue(1);
            entity.Property(l => l.PrixUnitaire)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Propriété calculée — pas persistée
            entity.Ignore(l => l.SousTotal);

            entity.HasOne(l => l.Vente)
                .WithMany(v => v.Lignes)
                .HasForeignKey(l => l.VenteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Vin)
                .WithMany(v => v.LignesVente)
                .HasForeignKey(l => l.VinId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Fournisseur
        modelBuilder.Entity<Fournisseur>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Nom).IsRequired().HasMaxLength(100);
            entity.Property(f => f.Email).HasMaxLength(150);
            entity.Property(f => f.Telephone).HasMaxLength(20);
            entity.Property(f => f.NumRue).HasMaxLength(100);
            entity.Property(f => f.NomRue).HasMaxLength(200);
            entity.Property(f => f.CodePostal).HasMaxLength(5);
            entity.Property(f => f.Ville).HasMaxLength(100);
        });

        //  Commande
        modelBuilder.Entity<Commande>(entity =>
        {
            entity.HasKey(c => c.Id);

            // Statut stocké en string pour lisibilité en base
            entity.Property(c => c.Statut)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(c => c.Fournisseur)
                .WithMany(f => f.Commandes)
                .HasForeignKey(c => c.FournisseurId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(c => c.DateCreation);
            entity.HasIndex(c => c.Statut);
        });

        // LigneCommande
        modelBuilder.Entity<LigneCommande>(entity =>
        {
            entity.HasKey(l => l.Id);

            // Snapshot
            entity.Property(l => l.VinNom).IsRequired().HasMaxLength(100);
            entity.Property(l => l.VinType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.HasOne(l => l.Commande)
                .WithMany(c => c.Lignes)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Vin)
                .WithMany(v => v.LignesCommande)
                .HasForeignKey(l => l.VinId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}