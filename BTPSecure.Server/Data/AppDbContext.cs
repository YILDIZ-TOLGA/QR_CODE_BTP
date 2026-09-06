using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace BTPSecure.Server.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> p_options) : base(p_options) { }

    public DbSet<E_Utilisateur> Utilisateurs => Set<E_Utilisateur>();
    public DbSet<E_Entreprise> Entreprises => Set<E_Entreprise>();
    public DbSet<E_CollaborateurEntreprise> CollaborateursEntreprises => Set<E_CollaborateurEntreprise>();
    public DbSet<E_Code> Codes => Set<E_Code>();
    public DbSet<E_FournisseurContact> FournisseursContacts => Set<E_FournisseurContact>();
    public DbSet<E_ResetMotDePasse> ResetsMotDePasse => Set<E_ResetMotDePasse>();
    public DbSet<E_Blacklist> Blacklists => Set<E_Blacklist>();
    public DbSet<E_Ticket> Tickets => Set<E_Ticket>();
    public DbSet<E_Memo> Memos => Set<E_Memo>();
    public DbSet<E_Notification> Notifications => Set<E_Notification>();
    public DbSet<E_ValidationCode> ValidationsCodes => Set<E_ValidationCode>();

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
            e.Property(u => u.NomSociete).HasMaxLength(200);
            e.Property(u => u.Telephone).HasMaxLength(20);
            e.Property(u => u.LimiteSousComptes).HasDefaultValue(3);
            e.Property(u => u.Role).HasConversion<int>();
            e.Property(u => u.EstActif).HasDefaultValue(true);
            e.Property(u => u.EstValide).HasDefaultValue(true);
            e.Property(u => u.EmailVerifie).HasDefaultValue(false);
            e.Property(u => u.TokenVerification).HasMaxLength(128);
            e.HasIndex(u => u.TokenVerification);
            e.HasIndex(u => u.ParentFournisseurId);
            e.HasOne<E_Utilisateur>()
                .WithMany()
                .HasForeignKey(u => u.ParentFournisseurId)
                .OnDelete(DeleteBehavior.Restrict);
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
            e.Property(ent => ent.LimiteResponsables).HasDefaultValue(2);
            // Colonne physique conservée : "PatronId"
            e.Property(ent => ent.DirigeantId).HasColumnName("PatronId");
            e.HasOne(ent => ent.Dirigeant)
                .WithMany()
                .HasForeignKey(ent => ent.DirigeantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // E_CollaborateurEntreprise (table physique conservée : "salaries_entreprises")
        p_modelBuilder.Entity<E_CollaborateurEntreprise>(e =>
        {
            e.ToTable("salaries_entreprises");
            e.HasKey(se => se.Id);
            e.Property(se => se.EstActif).HasDefaultValue(true);
            e.Property(se => se.StatutInvitation).HasConversion<int>().HasDefaultValue(Enum_StatutInvitation.EnAttente);
            e.Property(se => se.RoleEntreprise).HasConversion<int>().HasDefaultValue(Enum_RoleEntreprise.Collaborateur);
            // Colonne physique conservée : "SalarieId"
            e.Property(se => se.CollaborateurId).HasColumnName("SalarieId");
            e.HasOne(se => se.Collaborateur)
                .WithMany()
                .HasForeignKey(se => se.CollaborateurId)
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
            // Colonnes physiques conservées : "PatronId" et "SalarieId"
            e.Property(c => c.DirigeantId).HasColumnName("PatronId");
            e.Property(c => c.CollaborateurId).HasColumnName("SalarieId");
            e.HasOne(c => c.Dirigeant)
                .WithMany()
                .HasForeignKey(c => c.DirigeantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.Collaborateur)
                .WithMany()
                .HasForeignKey(c => c.CollaborateurId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(c => c.CreateurId);
            e.HasOne(c => c.Createur)
                .WithMany()
                .HasForeignKey(c => c.CreateurId)
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
            e.Property(c => c.EstPermanent).HasDefaultValue(false);
            e.Property(c => c.EmailTiers).HasMaxLength(256);
            e.Property(c => c.AchatsSupplementaires).HasDefaultValue(0);
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
            // Colonne physique conservée : "PatronId"
            e.Property(f => f.DirigeantId).HasColumnName("PatronId");
            e.HasOne(f => f.Dirigeant)
                .WithMany()
                .HasForeignKey(f => f.DirigeantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(f => new { f.DirigeantId, f.Siret });
        });

        // E_Blacklist
        p_modelBuilder.Entity<E_Blacklist>(e =>
        {
            e.ToTable("blacklists");
            e.HasKey(b => b.Id);
            e.Property(b => b.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(b => new { b.FournisseurId, b.Email }).IsUnique();
            e.HasOne(b => b.Fournisseur)
                .WithMany()
                .HasForeignKey(b => b.FournisseurId)
                .OnDelete(DeleteBehavior.Cascade);
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

        // E_Memo (pense-bête personnel)
        p_modelBuilder.Entity<E_Memo>(e =>
        {
            e.ToTable("memos");
            e.HasKey(m => m.Id);
            e.Property(m => m.Titre).IsRequired().HasMaxLength(200);
            e.Property(m => m.Contenu).IsRequired().HasMaxLength(10000);
            e.HasIndex(m => m.UtilisateurId);
            e.HasOne(m => m.Utilisateur)
                .WithMany()
                .HasForeignKey(m => m.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // E_Notification (message personnel affiché à la prochaine connexion)
        p_modelBuilder.Entity<E_Notification>(e =>
        {
            e.ToTable("notifications");
            e.HasKey(n => n.Id);
            e.Property(n => n.Titre).IsRequired().HasMaxLength(150);
            e.Property(n => n.Message).IsRequired().HasMaxLength(1000);
            e.Property(n => n.Severite).HasConversion<int>();
            e.Property(n => n.EstLue).HasDefaultValue(false);
            e.HasIndex(n => new { n.UtilisateurId, n.EstLue });
            e.HasOne(n => n.Utilisateur)
                .WithMany()
                .HasForeignKey(n => n.UtilisateurId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // E_ValidationCode (trace d'une utilisation ; voir l'entité pour le pourquoi)
        p_modelBuilder.Entity<E_ValidationCode>(e =>
        {
            e.ToTable("validations_codes");
            e.HasKey(v => v.Id);
            e.Property(v => v.ValeurUtilisee).IsRequired().HasMaxLength(9);
            e.Property(v => v.NumeroCommande).IsRequired().HasMaxLength(100);
            e.Property(v => v.EmailTiers).HasMaxLength(256);
            e.Property(v => v.AchatsSupplementaires).HasDefaultValue(0);
            e.Property(v => v.EstPermanent).HasDefaultValue(false);
            e.HasIndex(v => new { v.EntrepriseId, v.DateValidation });
            e.HasIndex(v => v.PorteurId);
            // Restrict partout : l'historique ne doit jamais disparaître avec un code ou un compte
            e.HasOne(v => v.Code)
                .WithMany()
                .HasForeignKey(v => v.CodeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Porteur)
                .WithMany()
                .HasForeignKey(v => v.PorteurId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(v => v.Validateur)
                .WithMany()
                .HasForeignKey(v => v.ValidateurId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // E_Ticket (messagerie / tickets avec pièce jointe, TTL 24 h)
        p_modelBuilder.Entity<E_Ticket>(e =>
        {
            e.ToTable("tickets");
            e.HasKey(t => t.Id);
            e.Property(t => t.Sujet).IsRequired().HasMaxLength(200);
            e.Property(t => t.Message).IsRequired().HasMaxLength(5000);
            e.Property(t => t.EmailDestinataire).HasMaxLength(256);
            e.Property(t => t.NomPieceJointe).HasMaxLength(256);
            e.Property(t => t.TypePieceJointe).HasMaxLength(100);
            e.Property(t => t.EstLu).HasDefaultValue(false);
            e.HasIndex(t => t.DestinataireId);
            e.HasIndex(t => t.ExpediteurId);
            e.HasIndex(t => t.DateCreation);
            e.HasOne(t => t.Expediteur)
                .WithMany()
                .HasForeignKey(t => t.ExpediteurId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Destinataire)
                .WithMany()
                .HasForeignKey(t => t.DestinataireId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
