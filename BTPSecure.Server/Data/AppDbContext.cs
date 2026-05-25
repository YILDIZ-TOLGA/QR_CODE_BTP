using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> p_options) : base(p_options) { }

    public DbSet<E_Utilisateur> Utilisateurs => Set<E_Utilisateur>();
    public DbSet<E_Entreprise> Entreprises => Set<E_Entreprise>();
    public DbSet<E_SalarieEntreprise> SalariesEntreprises => Set<E_SalarieEntreprise>();
    public DbSet<E_Code> Codes => Set<E_Code>();
    public DbSet<E_FournisseurContact> FournisseursContacts => Set<E_FournisseurContact>();
    public DbSet<E_ResetMotDePasse> ResetsMotDePasse => Set<E_ResetMotDePasse>();

    protected override void OnModelCreating(ModelBuilder p_modelBuilder)
    {
        // E_Utilisateur
        p_modelBuilder.Entity<E_Utilisateur>(e =>
        {
            e.ToTable("utilisateurs");
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.MotDePasseHash).IsRequired();
            e.Property(u => u.Sel).IsRequired();
            e.Property(u => u.Nom).IsRequired().HasMaxLength(100);
            e.Property(u => u.Prenom).IsRequired().HasMaxLength(100);
            e.Property(u => u.Telephone).HasMaxLength(20);
            e.Property(u => u.Role).HasConversion<int>();
            e.Property(u => u.EstActif).HasDefaultValue(true);
        });

        // E_Entreprise
        p_modelBuilder.Entity<E_Entreprise>(e =>
        {
            e.ToTable("entreprises");
            e.HasKey(ent => ent.Id);
            e.Property(ent => ent.Nom).IsRequired().HasMaxLength(200);
            e.Property(ent => ent.Adresse).HasMaxLength(500);
            e.Property(ent => ent.Siret).HasMaxLength(20);
            e.Property(ent => ent.EstAutorisee).HasDefaultValue(false);
            e.HasOne(ent => ent.Patron)
                .WithMany()
                .HasForeignKey(ent => ent.PatronId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // E_SalarieEntreprise
        p_modelBuilder.Entity<E_SalarieEntreprise>(e =>
        {
            e.ToTable("salaries_entreprises");
            e.HasKey(se => se.Id);
            e.Property(se => se.EstActif).HasDefaultValue(true);
            e.Property(se => se.StatutInvitation).HasConversion<int>().HasDefaultValue(Enum_StatutInvitation.EnAttente);
            e.HasOne(se => se.Salarie)
                .WithMany()
                .HasForeignKey(se => se.SalarieId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(se => se.Entreprise)
                .WithMany()
                .HasForeignKey(se => se.EntrepriseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // E_Code
        p_modelBuilder.Entity<E_Code>(e =>
        {
            e.ToTable("codes");
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Valeur).IsUnique();
            e.Property(c => c.Valeur).IsRequired().HasMaxLength(9);
            e.Property(c => c.NumeroCommande).IsRequired().HasMaxLength(100);
            e.Property(c => c.NomEntreprise).IsRequired().HasMaxLength(200);
            e.Property(c => c.TypeCode).HasConversion<int>();
            e.Property(c => c.Statut).HasConversion<int>();
            e.HasOne(c => c.Patron)
                .WithMany()
                .HasForeignKey(c => c.PatronId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Salarie)
                .WithMany()
                .HasForeignKey(c => c.SalarieId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Fournisseur)
                .WithMany()
                .HasForeignKey(c => c.FournisseurId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Entreprise)
                .WithMany()
                .HasForeignKey(c => c.EntrepriseId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(c => c.Reference).HasMaxLength(300);
            e.HasOne(c => c.FournisseurContact)
                .WithMany()
                .HasForeignKey(c => c.FournisseurContactId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // E_FournisseurContact
        p_modelBuilder.Entity<E_FournisseurContact>(e =>
        {
            e.ToTable("fournisseurs_contacts");
            e.HasKey(f => f.Id);
            e.Property(f => f.NomEntreprise).IsRequired().HasMaxLength(200);
            e.Property(f => f.Email).IsRequired().HasMaxLength(256);
            e.Property(f => f.Siret).IsRequired().HasMaxLength(14);
            e.Property(f => f.Siren).HasMaxLength(9);
            e.HasOne(f => f.Patron)
                .WithMany()
                .HasForeignKey(f => f.PatronId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(f => new { f.PatronId, f.Siret });
        });

        // E_ResetMotDePasse
        p_modelBuilder.Entity<E_ResetMotDePasse>(e =>
        {
            e.ToTable("resets_motdepasse");
            e.HasKey(r => r.Id);
            e.Property(r => r.Token).IsRequired().HasMaxLength(128);
            e.HasIndex(r => r.Token).IsUnique();
            e.HasOne(r => r.Utilisateur)
                .WithMany()
                .HasForeignKey(r => r.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
