using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;
using BTPSecure.Shared.Enums;

namespace BTPSecure.Server.Services;

public class S_Ticket
{
    private readonly DAO_Ticket _daoTicket;
    private readonly DAO_Utilisateur _daoUtilisateur;
    private readonly DAO_Entreprise _daoEntreprise;
    private readonly DAO_FournisseurContact _daoFournisseurContact;
    private readonly S_Email _sEmail;
    private readonly ILogger<S_Ticket> _logger;

    private const int _tailleMaxPieceJointe = 5 * 1024 * 1024; // 5 Mo

    public S_Ticket(DAO_Ticket p_daoTicket, DAO_Utilisateur p_daoUtilisateur, DAO_Entreprise p_daoEntreprise,
        DAO_FournisseurContact p_daoFournisseurContact, S_Email p_sEmail, ILogger<S_Ticket> p_logger)
    {
        _daoTicket = p_daoTicket;
        _daoUtilisateur = p_daoUtilisateur;
        _daoEntreprise = p_daoEntreprise;
        _daoFournisseurContact = p_daoFournisseurContact;
        _sEmail = p_sEmail;
        _logger = p_logger;
    }

    // Annuaire : liste des destinataires internes autorisés selon l'écosystème de l'utilisateur
    // Annuaire = qui chacun a le DROIT de contacter. L'envoi s'appuie dessus (voir Envoyer),
    // donc elargir l'annuaire elargit les permissions : rien d'autre a modifier.
    //
    // Regles :
    //  - Meme structure : tous les membres d'une entreprise se parlent entre eux
    //    (dirigeant et collaborateurs, dans les deux sens).
    //  - Relation commerciale : Dirigeant, Responsable et Responsable Admin dialoguent
    //    avec les fournisseurs de leur entreprise, et reciproquement.
    //
    // RGPD - minimisation : on ne renvoie que le strict necessaire a l'affichage
    // (identite + lien). Aucune adresse email ne transite (voir ResoudreDestinataire),
    // et les comptes desactives sont exclus.
    public async Task<List<DTO_ContactAnnuaire>> ObtenirAnnuaire(int p_userId)
    {
        var _contacts = new List<DTO_ContactAnnuaire>();
        var _utilisateur = await _daoUtilisateur.ObtenirParId(p_userId);
        if (_utilisateur == null)
            return _contacts;

        if (_utilisateur.Role == Enum_Role.Fournisseur)
        {
            await AjouterInterlocuteursDuFournisseur(_contacts, _utilisateur);
            return _contacts;
        }

        // Entreprises dont la personne fait partie, et si elle y voit les fournisseurs
        var _appartenances = new List<Appartenance>();

        var _sienne = await _daoEntreprise.ObtenirParDirigeantId(p_userId);
        if (_sienne != null)
        {
            _appartenances.Add(new Appartenance { Entreprise = _sienne, VoitLesFournisseurs = true });
        }

        var _liens = await _daoEntreprise.ObtenirInvitationsParCollaborateur(p_userId);
        foreach (var _lien in _liens)
        {
            if (_lien.StatutInvitation != Enum_StatutInvitation.Acceptee)
                continue;
            if (_lien.Entreprise == null)
                continue;

            // Seuls les Responsables et Responsables Admin traitent avec les fournisseurs
            bool _voitFournisseurs = _lien.RoleEntreprise == Enum_RoleEntreprise.Responsable
                || _lien.RoleEntreprise == Enum_RoleEntreprise.ResponsableAdmin;

            _appartenances.Add(new Appartenance { Entreprise = _lien.Entreprise, VoitLesFournisseurs = _voitFournisseurs });
        }

        foreach (var _appartenance in _appartenances)
        {
            var _entreprise = _appartenance.Entreprise;

            // Le dirigeant de la structure
            if (_entreprise.DirigeantId != p_userId)
            {
                var _dirigeant = _entreprise.Dirigeant;
                if (_dirigeant == null)
                {
                    _dirigeant = await _daoUtilisateur.ObtenirParId(_entreprise.DirigeantId);
                }
                if (_dirigeant != null)
                {
                    AjouterContact(_contacts, _dirigeant, "Dirigeant");
                }
            }

            // Tous les collegues de la structure, quel que soit leur role
            var _membres = await _daoEntreprise.ObtenirCollaborateurs(_entreprise.Id);
            foreach (var _membre in _membres)
            {
                if (_membre.StatutInvitation != Enum_StatutInvitation.Acceptee)
                    continue;
                if (_membre.Collaborateur == null)
                    continue;
                if (_membre.CollaborateurId == p_userId)
                    continue;

                AjouterContact(_contacts, _membre.Collaborateur, LibelleRole(_membre.RoleEntreprise));
            }

            // Les fournisseurs de l'entreprise (carnet tenu par le dirigeant)
            if (_appartenance.VoitLesFournisseurs)
            {
                var _fournisseurs = await _daoFournisseurContact.ObtenirParDirigeant(_entreprise.DirigeantId);
                foreach (var _fc in _fournisseurs)
                {
                    var _compte = await _daoUtilisateur.ObtenirParEmail(_fc.Email);
                    if (_compte != null && _compte.Role == Enum_Role.Fournisseur && !_compte.ParentFournisseurId.HasValue)
                    {
                        AjouterContact(_contacts, _compte, "Fournisseur");
                    }
                }
            }
        }

        return _contacts;
    }

    // Cote fournisseur : le dirigeant ET les responsables des entreprises qui l'ont reference.
    private async Task AjouterInterlocuteursDuFournisseur(List<DTO_ContactAnnuaire> p_contacts, E_Utilisateur p_fournisseur)
    {
        // Un sous-compte est reference sous l'email du compte principal
        var _emailReference = p_fournisseur.Email;
        if (p_fournisseur.ParentFournisseurId.HasValue)
        {
            var _principal = await _daoUtilisateur.ObtenirParId(p_fournisseur.ParentFournisseurId.Value);
            if (_principal != null)
                _emailReference = _principal.Email;
        }

        var _references = await _daoFournisseurContact.ObtenirParEmail(_emailReference);
        foreach (var _fc in _references)
        {
            if (_fc.Dirigeant != null)
            {
                AjouterContact(p_contacts, _fc.Dirigeant, "Dirigeant");
            }

            var _entreprise = await _daoEntreprise.ObtenirParDirigeantId(_fc.DirigeantId);
            if (_entreprise == null)
                continue;

            var _membres = await _daoEntreprise.ObtenirCollaborateurs(_entreprise.Id);
            foreach (var _membre in _membres)
            {
                if (_membre.StatutInvitation != Enum_StatutInvitation.Acceptee)
                    continue;
                if (_membre.Collaborateur == null)
                    continue;
                // Seuls les responsables traitent avec les fournisseurs : un collaborateur
                // simple n'a pas a apparaitre dans l'annuaire d'un tiers a l'entreprise.
                if (_membre.RoleEntreprise != Enum_RoleEntreprise.Responsable
                    && _membre.RoleEntreprise != Enum_RoleEntreprise.ResponsableAdmin)
                    continue;

                AjouterContact(p_contacts, _membre.Collaborateur, LibelleRole(_membre.RoleEntreprise));
            }
        }
    }

    // H_RoleEntreprise vit cote client : equivalent pour les libelles serveur
    private static string LibelleRole(Enum_RoleEntreprise p_role)
    {
        if (p_role == Enum_RoleEntreprise.Responsable)
        {
            return "Responsable";
        }
        if (p_role == Enum_RoleEntreprise.ResponsableAdmin)
        {
            return "Responsable Admin";
        }
        return "Collaborateur";
    }

    private class Appartenance
    {
        public E_Entreprise Entreprise { get; set; } = null!;
        public bool VoitLesFournisseurs { get; set; }
    }

    // Retrouve le compte interne correspondant a une adresse, UNIQUEMENT si cette personne
    // est joignable par l'appelant. Evite d'exposer l'email de tout l'annuaire au navigateur :
    // l'appelant interroge une adresse qu'il connait deja, il n'apprend rien de nouveau.
    public async Task<int?> ResoudreDestinataire(int p_userId, string p_email)
    {
        if (string.IsNullOrWhiteSpace(p_email))
            return null;

        var _compte = await _daoUtilisateur.ObtenirParEmail(p_email.Trim().ToLower());
        if (_compte == null)
            return null;

        var _annuaire = await ObtenirAnnuaire(p_userId);
        foreach (var _contact in _annuaire)
        {
            if (_contact.UtilisateurId == _compte.Id)
                return _compte.Id;
        }

        return null;
    }

    private static void AjouterContact(List<DTO_ContactAnnuaire> p_contacts, E_Utilisateur p_u, string p_relation)
    {
        // Un compte desactive ne doit plus circuler dans les annuaires
        if (!p_u.EstActif)
            return;

        foreach (var _c in p_contacts)
        {
            if (_c.UtilisateurId == p_u.Id)
                return;
        }
        p_contacts.Add(new DTO_ContactAnnuaire
        {
            UtilisateurId = p_u.Id,
            Nom = p_u.Nom,
            Prenom = p_u.Prenom,
            Relation = p_relation
        });
    }

    public async Task<(bool Succes, string Message)> Envoyer(DTO_EnvoyerTicket p_dto, int p_expediteurId)
    {
        if (string.IsNullOrWhiteSpace(p_dto.Sujet))
            return (false, "Le sujet est obligatoire.");
        if (string.IsNullOrWhiteSpace(p_dto.Message))
            return (false, "Le message est obligatoire.");

        var _expediteur = await _daoUtilisateur.ObtenirParId(p_expediteurId);
        if (_expediteur == null)
            return (false, "Expéditeur introuvable.");

        // Validation de la pièce jointe côté serveur (taille + type)
        if (p_dto.PieceJointe != null && p_dto.PieceJointe.Length > 0)
        {
            if (p_dto.PieceJointe.Length > _tailleMaxPieceJointe)
                return (false, "La pièce jointe dépasse la taille maximale de 5 Mo.");

            var _type = "";
            if (!string.IsNullOrEmpty(p_dto.TypePieceJointe))
                _type = p_dto.TypePieceJointe.ToLower();

            if (_type != "image/jpeg" && _type != "image/jpg" && _type != "image/png" && _type != "application/pdf")
                return (false, "Type de fichier non autorisé. Formats acceptés : JPG, PNG, PDF.");
        }

        int? _destinataireId = null;
        string? _emailDestinataire = null;
        bool _estExterne = false;

        if (p_dto.DestinataireId.HasValue)
        {
            // Destinataire interne : autorisé s'il est dans l'annuaire de l'expéditeur (sécurité serveur)
            var _annuaire = await ObtenirAnnuaire(p_expediteurId);
            var _autorise = false;
            foreach (var _contact in _annuaire)
            {
                if (_contact.UtilisateurId == p_dto.DestinataireId.Value)
                {
                    _autorise = true;
                    break;
                }
            }

            // Sinon, autorisé si une conversation existe déjà avec cette personne
            // (permet de répondre à un fil même si la relation a été retirée depuis)
            if (!_autorise)
                _autorise = await _daoTicket.ConversationExiste(p_expediteurId, p_dto.DestinataireId.Value);

            if (!_autorise)
                return (false, "Destinataire non autorisé.");

            _destinataireId = p_dto.DestinataireId.Value;
        }
        else if (!string.IsNullOrWhiteSpace(p_dto.EmailDestinataire))
        {
            var _email = p_dto.EmailDestinataire.Trim().ToLower();
            if (!_email.Contains("@") || !_email.Contains("."))
                return (false, "Email du destinataire invalide.");
            _emailDestinataire = _email;
            _estExterne = true;
        }
        else
        {
            return (false, "Aucun destinataire sélectionné.");
        }

        var _ticket = new E_Ticket
        {
            ExpediteurId = p_expediteurId,
            DestinataireId = _destinataireId,
            EmailDestinataire = _emailDestinataire,
            Sujet = p_dto.Sujet.Trim(),
            Message = p_dto.Message.Trim(),
            PieceJointe = p_dto.PieceJointe,
            NomPieceJointe = p_dto.NomPieceJointe,
            TypePieceJointe = p_dto.TypePieceJointe,
            EstLu = false
        };

        await _daoTicket.Creer(_ticket);
        _logger.LogInformation("Ticket {Id} envoyé par {ExpediteurId}", _ticket.Id, p_expediteurId);

        // Destinataire externe (pas de compte) → notification par email
        if (_estExterne)
        {
            var _nomExpediteur = $"{_expediteur.Prenom} {_expediteur.Nom}";
            var _emailCopie = _emailDestinataire!;
            var _sujetCopie = _ticket.Sujet;
            var _messageCopie = _ticket.Message;
            var _pjCopie = _ticket.PieceJointe;
            var _nomPjCopie = _ticket.NomPieceJointe;
            _ = Task.Run(async () =>
            {
                await _sEmail.EnvoyerTicketExterne(_emailCopie, _nomExpediteur, _sujetCopie, _messageCopie, _pjCopie, _nomPjCopie);
            });
            return (true, "Message envoyé par email au destinataire externe.");
        }

        return (true, "Message envoyé.");
    }

    public async Task<List<DTO_Ticket>> ObtenirRecus(int p_userId)
    {
        var _apercus = await _daoTicket.ObtenirRecus(p_userId);
        return _apercus.Select(a => VersDTO(a, p_userId)).ToList();
    }

    public async Task<List<DTO_Ticket>> ObtenirEnvoyes(int p_userId)
    {
        var _apercus = await _daoTicket.ObtenirEnvoyes(p_userId);
        return _apercus.Select(a => VersDTO(a, p_userId)).ToList();
    }

    public async Task<int> CompterNonLus(int p_userId)
    {
        return await _daoTicket.CompterNonLus(p_userId);
    }

    // Liste des conversations internes (regroupées par interlocuteur)
    public async Task<List<DTO_Conversation>> ObtenirConversations(int p_userId)
    {
        var _tous = new List<TicketApercu>();
        _tous.AddRange(await _daoTicket.ObtenirRecus(p_userId));
        _tous.AddRange(await _daoTicket.ObtenirEnvoyes(p_userId));

        // Regrouper par interlocuteur (on ignore les tickets externes sans compte)
        var _map = new Dictionary<int, List<TicketApercu>>();
        foreach (var _t in _tous)
        {
            int? _autreId = null;
            if (_t.ExpediteurId == p_userId && _t.DestinataireId.HasValue)
                _autreId = _t.DestinataireId.Value;
            else if (_t.DestinataireId.HasValue && _t.DestinataireId.Value == p_userId)
                _autreId = _t.ExpediteurId;

            if (!_autreId.HasValue)
                continue;

            if (!_map.ContainsKey(_autreId.Value))
                _map[_autreId.Value] = new List<TicketApercu>();
            _map[_autreId.Value].Add(_t);
        }

        var _resultat = new List<DTO_Conversation>();
        foreach (var _paire in _map)
        {
            var _liste = _paire.Value.OrderBy(t => t.DateCreation).ToList();
            var _dernier = _liste[_liste.Count - 1];

            var _nonLus = 0;
            foreach (var _t in _liste)
            {
                if (_t.DestinataireId.HasValue && _t.DestinataireId.Value == p_userId && !_t.EstLu)
                    _nonLus++;
            }

            // L'interlocuteur : selon qui a envoyé le dernier message
            var _nom = "";
            var _email = "";
            if (_dernier.ExpediteurId == _paire.Key)
            {
                _nom = $"{_dernier.ExpediteurPrenom} {_dernier.ExpediteurNom}";
                _email = _dernier.ExpediteurEmail;
            }
            else
            {
                _nom = $"{_dernier.DestinatairePrenom} {_dernier.DestinataireNom}";
                if (_dernier.DestinataireEmail != null)
                    _email = _dernier.DestinataireEmail;
            }

            _resultat.Add(new DTO_Conversation
            {
                AutreUtilisateurId = _paire.Key,
                NomAutre = _nom,
                EmailAutre = _email,
                DernierMessage = _dernier.Message,
                DateDernier = _dernier.DateCreation,
                NonLus = _nonLus
            });
        }

        return _resultat.OrderByDescending(c => c.DateDernier).ToList();
    }

    // Fil complet avec un interlocuteur ; marque au passage les messages reçus comme lus
    public async Task<List<DTO_Ticket>> ObtenirConversation(int p_userId, int p_autreId)
    {
        await _daoTicket.MarquerLusConversation(p_userId, p_autreId);
        var _apercus = await _daoTicket.ObtenirConversation(p_userId, p_autreId);
        return _apercus.Select(a => VersDTO(a, p_userId)).ToList();
    }

    public async Task<(bool Succes, string Message)> MarquerLu(int p_ticketId, int p_userId)
    {
        // Le filtre inclut DestinataireId == userId → seul le destinataire peut marquer lu
        await _daoTicket.MarquerLu(p_ticketId, p_userId);
        return (true, "Ticket marqué comme lu.");
    }

    // Pièce jointe : accessible seulement à l'expéditeur ou au destinataire interne
    public async Task<DTO_PieceJointe?> ObtenirPieceJointe(int p_ticketId, int p_userId)
    {
        var _ticket = await _daoTicket.ObtenirParId(p_ticketId);
        if (_ticket == null)
            return null;
        if (_ticket.PieceJointe == null || _ticket.PieceJointe.Length == 0)
            return null;

        var _autorise = false;
        if (_ticket.ExpediteurId == p_userId)
            _autorise = true;
        if (_ticket.DestinataireId.HasValue && _ticket.DestinataireId.Value == p_userId)
            _autorise = true;
        if (!_autorise)
            return null;

        var _nom = "piece-jointe";
        if (!string.IsNullOrEmpty(_ticket.NomPieceJointe))
            _nom = _ticket.NomPieceJointe;

        var _type = "application/octet-stream";
        if (!string.IsNullOrEmpty(_ticket.TypePieceJointe))
            _type = _ticket.TypePieceJointe;

        return new DTO_PieceJointe
        {
            Nom = _nom,
            Type = _type,
            Base64 = Convert.ToBase64String(_ticket.PieceJointe)
        };
    }

    private static DTO_Ticket VersDTO(TicketApercu p_a, int p_userId)
    {
        var _dto = new DTO_Ticket
        {
            Id = p_a.Id,
            ExpediteurId = p_a.ExpediteurId,
            NomExpediteur = $"{p_a.ExpediteurPrenom} {p_a.ExpediteurNom}",
            EmailExpediteur = p_a.ExpediteurEmail,
            DestinataireId = p_a.DestinataireId,
            Sujet = p_a.Sujet,
            Message = p_a.Message,
            ADocumentJoint = p_a.ADocumentJoint,
            NomPieceJointe = p_a.NomPieceJointe,
            TypePieceJointe = p_a.TypePieceJointe,
            DateCreation = p_a.DateCreation,
            EstLu = p_a.EstLu,
            EstEnvoyeParMoi = p_a.ExpediteurId == p_userId
        };

        if (p_a.DestinataireId.HasValue)
        {
            _dto.NomDestinataire = $"{p_a.DestinatairePrenom} {p_a.DestinataireNom}";
            if (p_a.DestinataireEmail != null)
                _dto.EmailDestinataire = p_a.DestinataireEmail;
        }
        else if (!string.IsNullOrEmpty(p_a.EmailDestinataireExterne))
        {
            _dto.EmailDestinataire = p_a.EmailDestinataireExterne;
        }

        return _dto;
    }
}
