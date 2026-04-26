using BTPSecure.Shared.Entites;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BTPSecure.Server.Services;

public class S_Pdf
{
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
                    col.Item().Text("BTPSecure").Bold().FontSize(24).FontColor(Colors.Blue.Darken2);
                    col.Item().Text("Confirmation de Transaction").FontSize(16).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    var _dateValidation = p_code.DateValidation?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? "N/A";

                    col.Item().PaddingBottom(15).Text($"Date et heure de validation : {_dateValidation}").FontSize(13);

                    col.Item().PaddingBottom(10).Row(row =>
                    {
                        row.RelativeItem().Text("Code utilisé :").Bold();
                        row.RelativeItem().Text(p_code.Valeur).FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                    });

                    col.Item().PaddingBottom(5).Row(row =>
                    {
                        row.RelativeItem().Text("Salarié :").Bold();
                        row.RelativeItem().Text($"{p_code.Salarie.Prenom} {p_code.Salarie.Nom}");
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
                    col.Item().PaddingTop(5).Text("Document généré automatiquement par BTPSecure")
                        .FontSize(9).FontColor(Colors.Grey.Medium).Italic();
                });
            });
        });

        return _document.GeneratePdf();
    }
}
