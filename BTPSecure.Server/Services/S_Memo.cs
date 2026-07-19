using BTPSecure.Server.DAO;
using BTPSecure.Shared.DTOs;
using BTPSecure.Shared.Entites;

namespace BTPSecure.Server.Services;

public class S_Memo
{
    private readonly DAO_Memo _daoMemo;

    public S_Memo(DAO_Memo p_daoMemo)
    {
        _daoMemo = p_daoMemo;
    }

    public async Task<List<DTO_Memo>> Lister(int p_userId)
    {
        var _memos = await _daoMemo.Lister(p_userId);
        return _memos.Select(VersDTO).ToList();
    }

    public async Task<(bool Succes, string Message, DTO_Memo? Memo)> Enregistrer(DTO_EnregistrerMemo p_dto, int p_userId)
    {
        if (string.IsNullOrWhiteSpace(p_dto.Titre))
            return (false, "Le titre est obligatoire.", null);
        if (p_dto.Titre.Length > 200)
            return (false, "Le titre est trop long (200 caractères maximum).", null);
        if (p_dto.Contenu != null && p_dto.Contenu.Length > 10000)
            return (false, "La note est trop longue (10 000 caractères maximum).", null);

        var _contenu = "";
        if (p_dto.Contenu != null)
            _contenu = p_dto.Contenu.Trim();

        // Création
        if (!p_dto.Id.HasValue)
        {
            var _nouveau = new E_Memo
            {
                UtilisateurId = p_userId,
                Titre = p_dto.Titre.Trim(),
                Contenu = _contenu
            };
            await _daoMemo.Creer(_nouveau);
            return (true, "Note créée.", VersDTO(_nouveau));
        }

        // Modification : uniquement ses propres notes
        var _memo = await _daoMemo.ObtenirParId(p_dto.Id.Value);
        if (_memo == null)
            return (false, "Note introuvable.", null);
        if (_memo.UtilisateurId != p_userId)
            return (false, "Cette note ne vous appartient pas.", null);

        _memo.Titre = p_dto.Titre.Trim();
        _memo.Contenu = _contenu;
        _memo.DateModification = DateTime.UtcNow;
        await _daoMemo.Sauvegarder();

        return (true, "Note enregistrée.", VersDTO(_memo));
    }

    public async Task<(bool Succes, string Message)> Supprimer(int p_id, int p_userId)
    {
        var _memo = await _daoMemo.ObtenirParId(p_id);
        if (_memo == null)
            return (false, "Note introuvable.");
        if (_memo.UtilisateurId != p_userId)
            return (false, "Cette note ne vous appartient pas.");

        await _daoMemo.Supprimer(_memo);
        return (true, "Note supprimée.");
    }

    private static DTO_Memo VersDTO(E_Memo p_m)
    {
        return new DTO_Memo
        {
            Id = p_m.Id,
            Titre = p_m.Titre,
            Contenu = p_m.Contenu,
            DateCreation = p_m.DateCreation,
            DateModification = p_m.DateModification
        };
    }
}
