using System.Security.Cryptography;
using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

public class S_Code
{
    private readonly DAO_Code _daoCode;
    private readonly DAO_Entreprise _daoEntreprise;
    private readonly DAO_Utilisateur _daoUtilisateur;
    private readonly DAO_FournisseurContact _daoFournisseurContact;
    private readonly DAO_Blacklist _daoBlacklist;
    private readonly S_Pdf _sPdf;
    private readonly S_Email _sEmail;
    private readonly ILogger<S_Code> _logger;

    private const string CARACTERES_AUTORISES = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    public S_Code(DAO_Code p_daoCode, DAO_Entreprise p_daoEntreprise, DAO_Utilisateur p_daoUtilisateur,
        DAO_FournisseurContact p_daoFournisseurContact, DAO_Blacklist p_daoBlacklist, S_Pdf p_sPdf, S_Email p_sEmail, ILogger<S_Code> p_logger)
    {
        _daoCode = p_daoCode;
        _daoEntreprise = p_daoEntreprise;
        _daoUtilisateur = p_daoUtilisateur;
        _daoFournisseurContact = p_daoFournisseurContact;
        _daoBlacklist = p_daoBlacklist;
        _sPdf = p_sPdf;
        _sEmail = p_sEmail;
        _logger = p_logger;
    }

    // Racine fournisseur (compte principal) pour la blacklist
    private async Task<int> ObtenirRacineFournisseur(int p_userId)
    {
        var _u = await _daoUtilisateur.ObtenirParId(p_userId);
        if (_u == null)
            return p_userId;
        if (_u.ParentFournisseurId.HasValue)
            return _u.ParentFournisseurId.Value;
        return p_userId;
    }

    public async Task<(bool Succes, string Message, DTO_CodeAffichage? Code)> Creer(DTO_CreerCode p_dto, int p_dirigeantId)
    {
        if (string.IsNullOrWhiteSpace(p_dto.NumeroCommande))
            return (false, "Le numéro de commande est obligatoire.", null);

        var _entreprise = await _daoEntreprise.ObtenirParId(p_dto.EntrepriseId);
        if (_entreprise == null)
            return (false, "Entreprise non trouvée.", null);

        // Autorisation : le Dirigeant propriétaire OU un Responsable Admin de l'entreprise
        bool _estDirigeant = _entreprise.DirigeantId == p_dirigeantId;
        bool _estResponsableAdmin = false;
        if (!_estDirigeant)
        {
            var _lienAuteur = await _daoEntreprise.ObtenirLienCollaborateur(p_dirigeantId, p_dto.EntrepriseId);
            if (_lienAuteur != null
                && _lienAuteur.StatutInvitation == Enum_StatutInvitation.Acceptee
                && _lienAuteur.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin)
            {
                _estResponsableAdmin = true;
            }
        }
        if (!_estDirigeant && !_estResponsableAdmin)
            return (false, "Vous n'êtes pas autorisé à créer des codes pour cette entreprise.", null);

        // Propriétaire de l'entreprise = titulaire des ressources (code, carnet fournisseur)
        var _proprietaireId = _entreprise.DirigeantId;

        if (!_entreprise.EstAutorisee)
            return (false, "Votre entreprise n'est pas encore autorisée par l'administrateur à créer des codes.", null);

        // Destinataire : un collaborateur de l'entreprise OU un tiers externe (email)
        E_Utilisateur? _collaborateur = null;
        int? _collaborateurId = null;
        string? _emailTiers = null;
        if (p_dto.PourTiers)
        {
            if (string.IsNullOrWhiteSpace(p_dto.EmailTiers))
                return (false, "L'email du destinataire externe est obligatoire.", null);
            _emailTiers = p_dto.EmailTiers.Trim().ToLower();
        }
        else
        {
            if (!await _daoEntreprise.CollaborateurEstDansEntreprise(p_dto.CollaborateurId, p_dto.EntrepriseId))
                return (false, "Ce collaborateur n'appartient pas à votre entreprise.", null);

            _collaborateur = await _daoUtilisateur.ObtenirParId(p_dto.CollaborateurId);
            if (_collaborateur == null)
                return (false, "Collaborateur non trouvé.", null);
            _collaborateurId = p_dto.CollaborateurId;
        }

        if (p_dto.TypeCode == Enum_TypeCode.Liste && string.IsNullOrWhiteSpace(p_dto.ListeMateriaux))
            return (false, "La liste des matériaux est obligatoire pour un code Liste.", null);

        // Achats supplémentaires HT (uniquement pour le type Liste, valeurs autorisées 0/50/100/200)
        int _achats = 0;
        if (p_dto.TypeCode == Enum_TypeCode.Liste)
        {
            if (p_dto.AchatsSupplementaires == 50 || p_dto.AchatsSupplementaires == 100 || p_dto.AchatsSupplementaires == 200)
                _achats = p_dto.AchatsSupplementaires;
        }

        int? _fournisseurContactId = null;
        string? _reference = null;
        if (p_dto.UtiliserFournisseur)
        {
            E_FournisseurContact? _contact = null;

            if (p_dto.FournisseurContactId.HasValue)
            {
                _contact = await _daoFournisseurContact.ObtenirParId(p_dto.FournisseurContactId.Value);
                if (_contact == null || _contact.DirigeantId != _proprietaireId)
                    return (false, "Fournisseur sélectionné introuvable.", null);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(p_dto.NouveauFournisseurNomEntreprise)
                    || string.IsNullOrWhiteSpace(p_dto.NouveauFournisseurEmail)
                    || string.IsNullOrWhiteSpace(p_dto.NouveauFournisseurSiret))
                    return (false, "Nom, email et SIRET du nouveau fournisseur sont obligatoires.", null);

                var _siret = new string(p_dto.NouveauFournisseurSiret.Where(char.IsDigit).ToArray());
                if (_siret.Length != 14)
                    return (false, "Le SIRET du nouveau fournisseur doit contenir 14 chiffres.", null);

                string? _siren = null;
                if (!string.IsNullOrWhiteSpace(p_dto.NouveauFournisseurSiren))
                {
                    _siren = new string(p_dto.NouveauFournisseurSiren.Where(char.IsDigit).ToArray());
                    if (_siren.Length != 9)
                        return (false, "Le SIREN du nouveau fournisseur doit contenir 9 chiffres.", null);
                }

                _contact = await _daoFournisseurContact.ObtenirParDirigeantEtSiret(_proprietaireId, _siret);
                if (_contact == null)
                {
                    _contact = new E_FournisseurContact
                    {
                        DirigeantId = _proprietaireId,
                        NomEntreprise = p_dto.NouveauFournisseurNomEntreprise.Trim(),
                        Email = p_dto.NouveauFournisseurEmail.Trim().ToLower(),
                        Siret = _siret,
                        Siren = _siren
                    };
                    await _daoFournisseurContact.Creer(_contact);
                }
            }

            _fournisseurContactId = _contact.Id;
            var _nomNettoye = _entreprise.Nom.Replace(" ", "-");
            _reference = $"{_nomNettoye}-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
        }

        // Validité : 24 h par défaut, ajustable par le créateur (bornée 1 h → 7 jours)
        int _dureeHeures = 24;
        if (p_dto.DureeValiditeHeures.HasValue)
        {
            _dureeHeures = p_dto.DureeValiditeHeures.Value;
            if (_dureeHeures < 1)
                _dureeHeures = 1;
            if (_dureeHeures > 168)
                _dureeHeures = 168;
        }

        var _valeur = await GenererValeurUnique();

        var _code = new E_Code
        {
            Valeur = _valeur,
            TypeCode = p_dto.TypeCode,
            NumeroCommande = p_dto.NumeroCommande.Trim(),
            NomEntreprise = _entreprise.Nom,
            Info = p_dto.Info?.Trim(),
            ListeMateriaux = p_dto.ListeMateriaux?.Trim(),
            DureeValidite = _dureeHeures,
            DirigeantId = _proprietaireId,
            CollaborateurId = _collaborateurId,
            EmailTiers = _emailTiers,
            AchatsSupplementaires = _achats,
            EntrepriseId = p_dto.EntrepriseId,
            FournisseurContactId = _fournisseurContactId,
            Reference = _reference
        };

        _code.DateExpiration = DateTime.UtcNow.AddHours(_dureeHeures);

        await _daoCode.Creer(_code);
        _code.Collaborateur = _collaborateur;

        _logger.LogInformation("Code {Valeur} créé par utilisateur {UserId}", _valeur, p_dirigeantId);

        // Envoi du code au destinataire externe (tiers)
        if (p_dto.PourTiers && _emailTiers != null)
        {
            var _emailCopie = _emailTiers;
            var _valeurCopie = _valeur;
            var _nomEntrepriseCopie = _entreprise.Nom;
            var _numCmdCopie = _code.NumeroCommande;
            _ = Task.Run(async () =>
            {
                await _sEmail.EnvoyerCodeTiers(_emailCopie, _valeurCopie, _nomEntrepriseCopie, _numCmdCopie);
            });
        }

        // Envoi d'invitation au fournisseur s'il n'a pas encore de compte avec ce SIRET
        if (p_dto.UtiliserFournisseur && _fournisseurContactId.HasValue)
        {
            var _contactFinal = await _daoFournisseurContact.ObtenirParId(_fournisseurContactId.Value);
            if (_contactFinal != null)
            {
                bool _compteExiste = await _daoUtilisateur.FournisseurExisteAvecSiret(_contactFinal.Siret);
                if (!_compteExiste)
                {
                    _ = Task.Run(async () =>
                    {
                        await _sEmail.EnvoyerInvitationFournisseur(
                            _contactFinal.Email,
                            _entreprise.Nom,
                            _contactFinal.NomEntreprise,
                            _contactFinal.Siret,
                            _contactFinal.Siren,
                            _contactFinal.Email);
                    });
                }
            }
        }

        return (true, "Code créé avec succès.", VersDTO(_code));
    }

    public async Task<List<DTO_CodeAffichage>> ObtenirParDirigeant(int p_dirigeantId)
    {
        var _codes = await _daoCode.ObtenirParDirigeant(p_dirigeantId);
        return _codes.Select(VersDTO).ToList();
    }

    // Contexte du tableau de bord : accessible au Dirigeant propriétaire OU à un Responsable Admin
    public async Task<DTO_ContexteDashboard> ObtenirContexteDashboard(int p_userId, bool p_estDirigeantRole)
    {
        var _ctx = new DTO_ContexteDashboard();

        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_userId);
        if (_entreprise != null)
        {
            _ctx.EstProprietaire = true;
            _ctx.AAcces = true;
        }
        else
        {
            var _lienRA = await _daoEntreprise.ObtenirPremierLienResponsableAdmin(p_userId);
            if (_lienRA != null)
            {
                _entreprise = _lienRA.Entreprise;
                _ctx.AAcces = true;
            }
            else if (p_estDirigeantRole)
            {
                _ctx.EstDirigeantSansEntreprise = true;
                return _ctx;
            }
            else
            {
                return _ctx;
            }
        }

        if (_entreprise == null)
            return _ctx;

        _ctx.Entreprise = new DTO_EntrepriseAffichage
        {
            Id = _entreprise.Id,
            Nom = _entreprise.Nom,
            Adresse = _entreprise.Adresse,
            Siret = _entreprise.Siret,
            DateCreation = _entreprise.DateCreation,
            EstAutorisee = _entreprise.EstAutorisee
        };

        var _liens = await _daoEntreprise.ObtenirCollaborateurs(_entreprise.Id);
        _ctx.Collaborateurs = _liens.Select(l => new DTO_CollaborateurAffichage
        {
            Id = l.Id,
            CollaborateurId = l.CollaborateurId,
            Nom = l.Collaborateur.Nom,
            Prenom = l.Collaborateur.Prenom,
            Email = l.Collaborateur.Email,
            DateAjout = l.DateAjout,
            StatutInvitation = l.StatutInvitation,
            RoleEntreprise = l.RoleEntreprise
        }).ToList();

        var _codes = await _daoCode.ObtenirParDirigeant(_entreprise.DirigeantId);
        _ctx.Codes = _codes.Select(VersDTO).ToList();

        var _fournisseurs = await _daoFournisseurContact.ObtenirParDirigeant(_entreprise.DirigeantId);
        _ctx.Fournisseurs = _fournisseurs.Select(f => new DTO_FournisseurContact
        {
            Id = f.Id,
            NomEntreprise = f.NomEntreprise,
            Email = f.Email,
            Siret = f.Siret,
            Siren = f.Siren,
            DateCreation = f.DateCreation
        }).ToList();

        var _notifs = await _daoCode.ObtenirNotificationsPourDirigeant(_entreprise.DirigeantId);
        _ctx.NbNotifications = _notifs.Count;

        return _ctx;
    }

    // Contexte de création de code : résout l'entreprise pour un Dirigeant OU un Responsable Admin
    public async Task<DTO_ContexteCreationCode> ObtenirContexteCreation(int p_userId)
    {
        var _ctx = new DTO_ContexteCreationCode { PeutCreer = false };

        var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(p_userId);
        if (_entreprise == null)
        {
            var _lienRA = await _daoEntreprise.ObtenirPremierLienResponsableAdmin(p_userId);
            if (_lienRA != null)
                _entreprise = _lienRA.Entreprise;
        }

        if (_entreprise == null)
            return _ctx;

        _ctx.PeutCreer = true;
        _ctx.EntrepriseId = _entreprise.Id;
        _ctx.NomEntreprise = _entreprise.Nom;
        _ctx.EstAutorisee = _entreprise.EstAutorisee;

        var _liens = await _daoEntreprise.ObtenirCollaborateurs(_entreprise.Id);
        _ctx.Collaborateurs = _liens
            .Where(l => l.StatutInvitation == Enum_StatutInvitation.Acceptee
                && l.CollaborateurId != p_userId)
            .Select(l => new DTO_CollaborateurAffichage
            {
                Id = l.Id,
                CollaborateurId = l.CollaborateurId,
                Nom = l.Collaborateur.Nom,
                Prenom = l.Collaborateur.Prenom,
                Email = l.Collaborateur.Email,
                StatutInvitation = l.StatutInvitation,
                RoleEntreprise = l.RoleEntreprise
            }).ToList();

        var _fournisseurs = await _daoFournisseurContact.ObtenirParDirigeant(_entreprise.DirigeantId);
        _ctx.Fournisseurs = _fournisseurs.Select(f => new DTO_FournisseurContact
        {
            Id = f.Id,
            NomEntreprise = f.NomEntreprise,
            Email = f.Email,
            Siret = f.Siret,
            Siren = f.Siren,
            DateCreation = f.DateCreation
        }).ToList();

        return _ctx;
    }

    public async Task<List<DTO_CodeAffichage>> ObtenirParCollaborateur(int p_collaborateurId)
    {
        var _codes = await _daoCode.ObtenirParCollaborateur(p_collaborateurId);
        return _codes.Select(VersDTO).ToList();
    }

    public async Task<(bool Succes, string Message, DTO_ResultatValidation? Resultat)> Valider(string p_valeur, int p_fournisseurId)
    {
        var _code = await _daoCode.ObtenirParValeur(p_valeur.Trim().ToUpper());
        if (_code == null)
            return (false, "Code non trouvé.", new DTO_ResultatValidation { EstValide = false, Message = "Code non trouvé." });

        if (_code.Statut != Enum_StatutCode.Actif)
            return (false, "Ce code n'est plus actif.", new DTO_ResultatValidation { EstValide = false, Message = $"Ce code est {_code.Statut.ToString().ToLower()}." });

        // Blocage : le dirigeant émetteur est-il blacklisté par ce fournisseur ?
        if (_code.Dirigeant != null)
        {
            var _racineId = await ObtenirRacineFournisseur(p_fournisseurId);
            if (await _daoBlacklist.Existe(_racineId, _code.Dirigeant.Email))
                return (false, "Émetteur blacklisté.", new DTO_ResultatValidation { EstValide = false, Message = "Vous avez blacklisté l'émetteur de ce code." });
        }

        if (_code.DateExpiration.HasValue && _code.DateExpiration.Value < DateTime.UtcNow)
        {
            _code.Statut = Enum_StatutCode.Expire;
            await _daoCode.Sauvegarder();
            return (false, "Ce code est expiré.", new DTO_ResultatValidation { EstValide = false, Message = "Ce code est expiré." });
        }

        _code.FournisseurId = p_fournisseurId;
        _code.DateValidation = DateTime.UtcNow;

        // Code permanent (Responsable) → régénère sa valeur et reste actif.
        // Tout autre code → usage unique : consommé après une validation.
        if (_code.EstPermanent)
        {
            _code.Valeur = await GenererValeurUnique();
        }
        else
        {
            _code.Statut = Enum_StatutCode.Utilise;
            _code.DateExpiration = DateTime.UtcNow.AddMinutes(10);
        }

        await _daoCode.Sauvegarder();

        var _pdf = _sPdf.GenererConfirmation(_code);

        string _nomAff = "";
        string _prenomAff = "";
        if (_code.Collaborateur != null)
        {
            _nomAff = _code.Collaborateur.Nom;
            _prenomAff = _code.Collaborateur.Prenom;
        }
        else if (_code.EmailTiers != null)
        {
            _nomAff = _code.EmailTiers;
        }

        var _resultat = new DTO_ResultatValidation
        {
            EstValide = true,
            Message = "Code validé avec succès.",
            NomCollaborateur = _nomAff,
            PrenomCollaborateur = _prenomAff,
            NumeroCommande = _code.NumeroCommande,
            NomEntreprise = _code.NomEntreprise,
            ListeMateriaux = _code.ListeMateriaux,
            Info = _code.Info,
            AchatsSupplementaires = _code.AchatsSupplementaires,
            PdfBase64 = Convert.ToBase64String(_pdf)
        };

        _logger.LogInformation("Code {Valeur} validé par fournisseur {FournisseurId}", p_valeur, p_fournisseurId);
        return (true, "Code validé.", _resultat);
    }

    public async Task<(bool Succes, string Message)> Revoquer(int p_codeId, int p_userId)
    {
        var _code = await _daoCode.ObtenirParId(p_codeId);
        if (_code == null)
            return (false, "Code non trouvé.");

        // Autorisé : le Dirigeant propriétaire OU un Responsable Admin de l'entreprise du code
        bool _estProprietaire = _code.DirigeantId == p_userId;
        bool _estResponsableAdmin = false;
        if (!_estProprietaire)
        {
            var _lien = await _daoEntreprise.ObtenirLienCollaborateur(p_userId, _code.EntrepriseId);
            if (_lien != null
                && _lien.StatutInvitation == Enum_StatutInvitation.Acceptee
                && _lien.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin)
            {
                _estResponsableAdmin = true;
            }
        }
        if (!_estProprietaire && !_estResponsableAdmin)
            return (false, "Vous n'êtes pas autorisé à révoquer ce code.");

        // Un Responsable Admin ne peut pas révoquer son propre code ni celui d'un autre Responsable Admin
        if (_estResponsableAdmin && _code.CollaborateurId.HasValue)
        {
            if (_code.CollaborateurId.Value == p_userId)
                return (false, "Vous ne pouvez pas révoquer votre propre code.");

            var _lienCible = await _daoEntreprise.ObtenirLienCollaborateur(_code.CollaborateurId.Value, _code.EntrepriseId);
            if (_lienCible != null && _lienCible.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin)
                return (false, "Vous ne pouvez pas révoquer le code d'un autre Responsable Admin.");
        }

        if (_code.Statut != Enum_StatutCode.Actif)
            return (false, "Seuls les codes actifs peuvent être révoqués.");

        _code.Statut = Enum_StatutCode.Revoque;
        await _daoCode.Sauvegarder();

        _logger.LogInformation("Code {Id} révoqué par utilisateur {UserId}", p_codeId, p_userId);
        return (true, "Code révoqué avec succès.");
    }

    // Réattribue un code déjà généré à un autre destinataire et/ou un autre fournisseur.
    // Règle de sécurité : si le destinataire change, la valeur du code est RÉGÉNÉRÉE,
    // car l'ancien destinataire connaît l'ancienne valeur (le code est un porteur).
    public async Task<(bool Succes, string Message, DTO_ResultatModificationCode? Resultat)> Modifier(DTO_ModifierCode p_dto, int p_userId)
    {
        var _code = await _daoCode.ObtenirParId(p_dto.CodeId);
        if (_code == null)
            return (false, "Code non trouvé.", null);

        // Autorisé : le Dirigeant propriétaire OU un Responsable Admin de l'entreprise du code
        bool _estProprietaire = _code.DirigeantId == p_userId;
        bool _estResponsableAdmin = false;
        if (!_estProprietaire)
        {
            var _lien = await _daoEntreprise.ObtenirLienCollaborateur(p_userId, _code.EntrepriseId);
            if (_lien != null
                && _lien.StatutInvitation == Enum_StatutInvitation.Acceptee
                && _lien.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin)
            {
                _estResponsableAdmin = true;
            }
        }
        if (!_estProprietaire && !_estResponsableAdmin)
            return (false, "Vous n'êtes pas autorisé à modifier ce code.", null);

        // Un Responsable Admin ne peut pas modifier son propre code ni celui d'un autre Responsable Admin
        if (_estResponsableAdmin && _code.CollaborateurId.HasValue)
        {
            if (_code.CollaborateurId.Value == p_userId)
                return (false, "Vous ne pouvez pas modifier votre propre code.", null);

            var _lienCible = await _daoEntreprise.ObtenirLienCollaborateur(_code.CollaborateurId.Value, _code.EntrepriseId);
            if (_lienCible != null && _lienCible.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin)
                return (false, "Vous ne pouvez pas modifier le code d'un autre Responsable Admin.", null);
        }

        if (_code.Statut != Enum_StatutCode.Actif)
            return (false, "Seuls les codes actifs peuvent être modifiés.", null);
        if (_code.EstPermanent)
            return (false, "Un code permanent est lié à son responsable et ne peut pas être réattribué.", null);

        // ── Nouveau destinataire ────────────────────────────────────────────
        int? _nouveauCollaborateurId = null;
        string? _nouvelEmailTiers = null;
        if (p_dto.PourTiers)
        {
            if (string.IsNullOrWhiteSpace(p_dto.EmailTiers))
                return (false, "L'email du destinataire externe est obligatoire.", null);
            _nouvelEmailTiers = p_dto.EmailTiers.Trim().ToLower();
        }
        else
        {
            if (!await _daoEntreprise.CollaborateurEstDansEntreprise(p_dto.CollaborateurId, _code.EntrepriseId))
                return (false, "Ce collaborateur n'appartient pas à cette entreprise.", null);
            _nouveauCollaborateurId = p_dto.CollaborateurId;
        }

        // ── Nouveau fournisseur ─────────────────────────────────────────────
        int? _nouveauFournisseurContactId = null;
        if (p_dto.UtiliserFournisseur)
        {
            if (!p_dto.FournisseurContactId.HasValue)
                return (false, "Sélectionnez un fournisseur.", null);

            var _contact = await _daoFournisseurContact.ObtenirParId(p_dto.FournisseurContactId.Value);
            if (_contact == null || _contact.DirigeantId != _code.DirigeantId)
                return (false, "Fournisseur sélectionné introuvable.", null);
            _nouveauFournisseurContactId = _contact.Id;
        }

        var _destinataireChange = _code.CollaborateurId != _nouveauCollaborateurId
                                  || _code.EmailTiers != _nouvelEmailTiers;
        var _fournisseurChange = _code.FournisseurContactId != _nouveauFournisseurContactId;

        if (!_destinataireChange && !_fournisseurChange)
            return (false, "Aucune modification à appliquer.", null);

        _code.CollaborateurId = _nouveauCollaborateurId;
        _code.EmailTiers = _nouvelEmailTiers;
        _code.FournisseurContactId = _nouveauFournisseurContactId;

        // Le fournisseur change : la préparation de l'ancien fournisseur n'a plus lieu d'être
        if (_fournisseurChange)
        {
            _code.EstPrete = false;
            _code.DatePrete = null;

            // Un code sans fournisseur n'a pas de référence : on la génère à la volée
            if (_nouveauFournisseurContactId.HasValue && string.IsNullOrEmpty(_code.Reference))
            {
                var _entrepriseCode = await _daoEntreprise.ObtenirParId(_code.EntrepriseId);
                if (_entrepriseCode != null)
                {
                    var _nomNettoye = _entrepriseCode.Nom.Replace(" ", "-");
                    _code.Reference = $"{_nomNettoye}-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
                }
            }
        }

        // SÉCURITÉ : l'ancien destinataire connaît l'ancienne valeur → on la régénère
        var _valeurRegeneree = false;
        if (_destinataireChange)
        {
            _code.Valeur = await GenererValeurUnique();
            _valeurRegeneree = true;
        }

        await _daoCode.Sauvegarder();

        _logger.LogInformation(
            "Code {Id} modifié par {UserId} (destinataire changé : {DestChange}, fournisseur changé : {FourChange}, valeur régénérée : {Regen})",
            _code.Id, p_userId, _destinataireChange, _fournisseurChange, _valeurRegeneree);

        // Le nouveau destinataire externe doit recevoir la valeur courante du code
        if (_nouvelEmailTiers != null && (_destinataireChange || _valeurRegeneree))
        {
            var _emailCopie = _nouvelEmailTiers;
            var _valeurCopie = _code.Valeur;
            var _nomEntrepriseCopie = _code.NomEntreprise;
            var _numCmdCopie = _code.NumeroCommande;
            _ = Task.Run(async () =>
            {
                await _sEmail.EnvoyerCodeTiers(_emailCopie, _valeurCopie, _nomEntrepriseCopie, _numCmdCopie);
            });
        }

        // Invitation au nouveau fournisseur s'il n'a pas encore de compte
        if (_fournisseurChange && _nouveauFournisseurContactId.HasValue)
        {
            var _contactFinal = await _daoFournisseurContact.ObtenirParId(_nouveauFournisseurContactId.Value);
            if (_contactFinal != null)
            {
                bool _compteExiste = await _daoUtilisateur.FournisseurExisteAvecSiret(_contactFinal.Siret);
                if (!_compteExiste)
                {
                    var _nomEntrepriseCopie = _code.NomEntreprise;
                    _ = Task.Run(async () =>
                    {
                        await _sEmail.EnvoyerInvitationFournisseur(
                            _contactFinal.Email,
                            _nomEntrepriseCopie,
                            _contactFinal.NomEntreprise,
                            _contactFinal.Siret,
                            _contactFinal.Siren,
                            _contactFinal.Email);
                    });
                }
            }
        }

        var _message = "Code modifié.";
        if (_valeurRegeneree)
            _message = "Code modifié et régénéré : l'ancienne valeur n'est plus valable.";

        var _resultat = new DTO_ResultatModificationCode
        {
            Message = _message,
            CodeRegenere = _valeurRegeneree,
            NouvelleValeur = _code.Valeur
        };
        return (true, _message, _resultat);
    }

    // Crée le code permanent libre-service d'un Responsable / Responsable Admin (idempotent)
    public async Task CreerCodePermanent(int p_collaborateurId, E_Entreprise p_entreprise)
    {
        var _existant = await _daoCode.ObtenirCodePermanentActif(p_collaborateurId, p_entreprise.Id);
        if (_existant != null)
            return;

        var _valeur = await GenererValeurUnique();
        var _code = new E_Code
        {
            Valeur = _valeur,
            TypeCode = Enum_TypeCode.LibreService,
            NumeroCommande = "Accès permanent",
            NomEntreprise = p_entreprise.Nom,
            DirigeantId = p_entreprise.DirigeantId,
            CollaborateurId = p_collaborateurId,
            EntrepriseId = p_entreprise.Id,
            EstPermanent = true,
            DateExpiration = null
        };
        await _daoCode.Creer(_code);
        _logger.LogInformation("Code permanent créé pour collaborateur {CollaborateurId} entreprise {EntrepriseId}", p_collaborateurId, p_entreprise.Id);
    }

    // Révoque le code permanent d'un collaborateur rétrogradé
    public async Task RevoquerCodePermanent(int p_collaborateurId, int p_entrepriseId)
    {
        var _code = await _daoCode.ObtenirCodePermanentActif(p_collaborateurId, p_entrepriseId);
        if (_code == null)
            return;

        _code.Statut = Enum_StatutCode.Revoque;
        _code.EstPermanent = false;
        await _daoCode.Sauvegarder();
        _logger.LogInformation("Code permanent révoqué pour collaborateur {CollaborateurId}", p_collaborateurId);
    }

    private async Task<string> GenererValeurUnique()
    {
        string _valeur;
        do
        {
            _valeur = GenererCode();
        } while (await _daoCode.ValeurExiste(_valeur));

        return _valeur;
    }

    private static string GenererCode()
    {
        var _chars = new char[8];
        for (int i = 0; i < 8; i++)
        {
            _chars[i] = CARACTERES_AUTORISES[RandomNumberGenerator.GetInt32(CARACTERES_AUTORISES.Length)];
        }
        return $"{new string(_chars, 0, 4)}-{new string(_chars, 4, 4)}";
    }

    public async Task<(bool Succes, string Message, DTO_ResultatValidation? Resultat)> ValiderPourCommande(int p_codeId, string p_valeur, int p_fournisseurId)
    {
        var _code = await _daoCode.ObtenirParId(p_codeId);
        if (_code == null)
            return (false, "Commande introuvable.", new DTO_ResultatValidation { EstValide = false, Message = "Commande introuvable." });

        if (_code.Statut != Enum_StatutCode.Actif)
            return (false, "Cette commande n'est plus active.", new DTO_ResultatValidation { EstValide = false, Message = $"Cette commande est {_code.Statut.ToString().ToLower()}." });

        var _valeurSaisie = p_valeur.Trim().ToUpper();
        if (_code.Valeur != _valeurSaisie)
            return (false, "Le code ne correspond pas à la référence associée.",
                new DTO_ResultatValidation { EstValide = false, Message = "Le code ne correspond pas à la référence associée." });

        return await Valider(_valeurSaisie, p_fournisseurId);
    }

    public async Task<List<DTO_CommandeAVenir>> ObtenirCommandesAVenir(int p_fournisseurId)
    {
        var _utilisateur = await _daoUtilisateur.ObtenirParId(p_fournisseurId);
        if (_utilisateur == null || string.IsNullOrEmpty(_utilisateur.Siret))
            return new List<DTO_CommandeAVenir>();

        var _codes = await _daoCode.ObtenirCommandesPourFournisseur(_utilisateur.Siret, _utilisateur.Siren);

        // Exclure les commandes des dirigeants blacklistés
        var _racineId = await ObtenirRacineFournisseur(p_fournisseurId);
        var _blacklist = await _daoBlacklist.ListerEmails(_racineId);
        if (_blacklist.Count > 0)
        {
            _codes = _codes.Where(c => c.Dirigeant == null || !_blacklist.Contains(c.Dirigeant.Email)).ToList();
        }

        return _codes.Select(c => new DTO_CommandeAVenir
        {
            CodeId = c.Id,
            Reference = c.Reference ?? "",
            NomEntrepriseDirigeant = c.NomEntreprise,
            NumeroCommande = c.NumeroCommande,
            DateCreation = c.DateCreation,
            DateExpiration = c.DateExpiration,
            ListeMateriaux = c.ListeMateriaux,
            Info = c.Info,
            TypeCode = c.TypeCode,
            AchatsSupplementaires = c.AchatsSupplementaires,
            EstPrete = c.EstPrete,
            DatePrete = c.DatePrete,
            EstTiers = c.EmailTiers != null,
            Destinataire = ObtenirLibelleDestinataire(c)
        }).ToList();
    }

    // Libellé du destinataire d'un code, pour l'affichage côté fournisseur
    private static string ObtenirLibelleDestinataire(E_Code p_code)
    {
        if (p_code.EmailTiers != null)
            return p_code.EmailTiers;
        if (p_code.Collaborateur != null)
            return $"{p_code.Collaborateur.Prenom} {p_code.Collaborateur.Nom}";
        return "";
    }

    // Nombre de commandes restant à préparer (pour le badge de la sidebar)
    public async Task<int> CompterCommandesAPreparer(int p_fournisseurId)
    {
        var _commandes = await ObtenirCommandesAVenir(p_fournisseurId);
        return _commandes.Count(c => !c.EstPrete);
    }

    public async Task<(bool Succes, string Message)> MarquerPrete(int p_codeId, int p_fournisseurId)
    {
        var _code = await _daoCode.ObtenirParId(p_codeId);
        if (_code == null)
            return (false, "Commande introuvable.");

        if (_code.Statut != Enum_StatutCode.Actif)
            return (false, "Cette commande n'est plus active.");

        var _utilisateur = await _daoUtilisateur.ObtenirParId(p_fournisseurId);
        if (_utilisateur == null || string.IsNullOrEmpty(_utilisateur.Siret))
            return (false, "Fournisseur introuvable.");

        if (_code.FournisseurContactId == null)
            return (false, "Cette commande n'a pas de fournisseur associé.");

        var _contact = await _daoFournisseurContact.ObtenirParId(_code.FournisseurContactId.Value);
        if (_contact == null)
            return (false, "Fournisseur associé introuvable.");

        if (_contact.Siret != _utilisateur.Siret)
            return (false, "Vous n'êtes pas le fournisseur de cette commande.");

        // Le SIRET identifie déjà l'établissement : on ne compare le SIREN que s'il est
        // renseigné des deux côtés (il est optionnel), sinon on refuserait à tort.
        bool _sirenContactPresent = !string.IsNullOrEmpty(_contact.Siren);
        bool _sirenUserPresent = !string.IsNullOrEmpty(_utilisateur.Siren);
        if (_sirenContactPresent && _sirenUserPresent && _contact.Siren != _utilisateur.Siren)
            return (false, "Vous n'êtes pas le fournisseur de cette commande.");

        if (_code.EstPrete)
            return (false, "Cette commande est déjà marquée comme prête.");

        _code.EstPrete = true;
        _code.DatePrete = DateTime.UtcNow;
        await _daoCode.Sauvegarder();

        _logger.LogInformation("Commande {CodeId} marquée prête par fournisseur {FournisseurId}", p_codeId, p_fournisseurId);
        return (true, "Commande marquée comme prête.");
    }

    public async Task<List<DTO_NotificationDirigeant>> ObtenirNotificationsDirigeant(int p_dirigeantId)
    {
        var _codes = await _daoCode.ObtenirNotificationsPourDirigeant(p_dirigeantId);

        return _codes.Select(c => new DTO_NotificationDirigeant
        {
            CodeId = c.Id,
            Reference = c.Reference ?? "",
            NumeroCommande = c.NumeroCommande,
            NomFournisseur = c.FournisseurContact?.NomEntreprise ?? "",
            EmailFournisseur = c.FournisseurContact?.Email ?? "",
            NomCollaborateur = c.Collaborateur?.Nom ?? "",
            PrenomCollaborateur = c.Collaborateur?.Prenom ?? "",
            DatePrete = c.DatePrete ?? DateTime.UtcNow,
            DateExpiration = c.DateExpiration,
            ListeMateriaux = c.ListeMateriaux,
            Info = c.Info
        }).ToList();
    }

    private static DTO_CodeAffichage VersDTO(E_Code p_code)
    {
        return new DTO_CodeAffichage
        {
            Id = p_code.Id,
            Valeur = p_code.Valeur,
            TypeCode = p_code.TypeCode,
            Statut = p_code.Statut,
            NumeroCommande = p_code.NumeroCommande,
            NomEntreprise = p_code.NomEntreprise,
            Info = p_code.Info,
            ListeMateriaux = p_code.ListeMateriaux,
            DateCreation = p_code.DateCreation,
            DateExpiration = p_code.DateExpiration,
            NomCollaborateur = p_code.Collaborateur?.Nom ?? "",
            PrenomCollaborateur = p_code.Collaborateur?.Prenom ?? "",
            CollaborateurId = p_code.CollaborateurId ?? 0,
            Reference = p_code.Reference,
            NomFournisseurContact = p_code.FournisseurContact?.NomEntreprise,
            FournisseurContactId = p_code.FournisseurContactId,
            EmailTiers = p_code.EmailTiers,
            AchatsSupplementaires = p_code.AchatsSupplementaires,
            EstPermanent = p_code.EstPermanent
        };
    }
}
