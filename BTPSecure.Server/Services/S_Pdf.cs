using BTPSecure.Shared.Entites;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BTPSecure.Server.Services;

public class S_Pdf
{
    // Logo officiel KEYDO (mot-symbole). fill explicite : currentColor n'existe pas dans un PDF.
    private const string _logoKeydo = """
<svg viewBox="322.09 362.01 650.16 96.51" xmlns="http://www.w3.org/2000/svg" fill="#00C9B7" fill-rule="evenodd">
<path d="M322.091302,458.501414 L322.091302,362.262806 L351.171577,362.325152 L351.171577,402.586954 L396.820066,362.236044 L435.264676,362.236044 L381.729481,408.760145 L439.237181,458.227564 L397.794518,458.227564 L351.356369,418.325748 L351.356369,458.520145 Z"/>
<path d="M448.272721,458.351546 L448.272721,362.260321 L553.406725,362.260321 L553.406725,385.444912 L478.318632,385.444912 L478.318632,401.432090 L547.423793,401.432090 L547.423793,420.457502 L478.331166,420.457502 L478.331166,436.496380 L558.459171,436.496380 L558.424549,458.351546 Z"/>
<path d="M563.394995,362.247835 L602.415323,362.247835 L633.733670,393.731772 L664.488747,362.362528 L702.785907,362.362528 L647.379518,418.051182 L647.379518,458.406426 L619.294186,458.406426 L619.294186,419.413596 Z"/>
<path d="M712.303166,385.239033 L712.303166,362.013533 L810.128242,362.013533 A25,25 0 0,1 835.128242,387.013533 L835.128242,432.860772 A25,25 0 0,1 810.128242,457.860772 L712.303166,457.860772 L712.303166,401.164965 L746.021320,401.164965 L746.021320,437.675660 L796.615814,437.675660 A10,10 0 0,0 806.615814,427.675660 L806.615814,395.239033 A10,10 0 0,0 796.615814,385.239033 Z"/>
<path d="M849.125282,387.380607 C849.112332,380.747870 851.712026,374.381797 856.352088,369.683766 C860.992151,364.985735 867.292215,362.340865 873.865435,362.331377 L947.432320,362.225184 C954.013617,362.215683 960.328227,364.848808 964.982934,369.543602 C969.637642,374.238395 972.249913,380.609029 972.243393,387.249927 L972.198402,433.072320 C972.184855,446.869760 961.096396,458.047552 947.422771,458.047552 L873.990141,458.047552 C860.325991,458.047552 849.241469,446.884668 849.214547,433.096808 Z M878.278709,395.040152 C878.278709,389.517304 882.633261,385.040152 888.004874,385.040152 L932.308004,385.040152 C937.679616,385.040152 942.034169,389.517304 942.034169,395.040152 L942.034169,427.765000 C942.034169,433.287847 937.679616,437.765000 932.308004,437.765000 L888.004874,437.765000 C882.633261,437.765000 878.278709,433.287847 878.278709,427.765000 Z"/>
</svg>
""";

    public byte[] GenererConfirmation(E_Code p_code)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var _document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Column(col =>
                {
                    col.Item().Width(150).Svg(_logoKeydo);
                    col.Item().Text("Confirmation de Transaction").FontSize(16).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor("#00C9B7");
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    var _dateValidation = p_code.DateValidation?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "N/A";

                    col.Item().PaddingBottom(15).Text($"Date et heure de validation : {_dateValidation}").FontSize(13);

                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Text("Code utilisé :").Bold();
                        row.RelativeItem().Text(p_code.Valeur).FontSize(16).Bold().FontColor("#00C9B7");
                    });

                    string _destinataire;
                    if (p_code.Collaborateur != null)
                    {
                        _destinataire = $"{p_code.Collaborateur.Prenom} {p_code.Collaborateur.Nom}";
                    }
                    else if (!string.IsNullOrWhiteSpace(p_code.EmailTiers))
                    {
                        _destinataire = $"{p_code.EmailTiers} (externe)";
                    }
                    else
                    {
                        _destinataire = "N/A";
                    }

                    col.Item().PaddingBottom(5).Row(row =>
                    {
                        row.RelativeItem().Text("Destinataire :").Bold();
                        row.RelativeItem().Text(_destinataire);
                    });

                    col.Item().PaddingBottom(5).Row(row =>
                    {
                        row.RelativeItem().Text("Numéro de commande :").Bold();
                        row.RelativeItem().Text(p_code.NumeroCommande);
                    });

                    col.Item().PaddingBottom(5).Row(row =>
                    {
                        row.RelativeItem().Text("Entreprise :").Bold();
                        row.RelativeItem().Text(p_code.NomEntreprise);
                    });

                    if (p_code.AchatsSupplementaires > 0)
                    {
                        col.Item().PaddingBottom(5).Row(row =>
                        {
                            row.RelativeItem().Text("Achats supplémentaires autorisés :").Bold();
                            row.RelativeItem().Text($"{p_code.AchatsSupplementaires} € HT");
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(p_code.ListeMateriaux))
                    {
                        col.Item().PaddingTop(15).Text("Liste des matériaux :").Bold();
                        col.Item().PaddingTop(5).Background(Colors.Grey.Lighten4).Padding(10).Text(p_code.ListeMateriaux);
                    }

                    if (!string.IsNullOrWhiteSpace(p_code.Info))
                    {
                        col.Item().PaddingTop(15).Text("Informations complémentaires :").Bold();
                        col.Item().PaddingTop(5).Background(Colors.Grey.Lighten4).Padding(10).Text(p_code.Info);
                    }
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    col.Item().PaddingTop(5).Text("Document généré automatiquement par KEYDO")
                        .FontSize(9).FontColor(Colors.Grey.Medium).Italic();
                });
            });
        });

        return _document.GeneratePdf();
    }
}
