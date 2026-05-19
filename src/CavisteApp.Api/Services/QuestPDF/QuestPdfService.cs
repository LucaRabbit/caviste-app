using CavisteApp.Api.Configuration;
using CavisteApp.Api.Entities;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace CavisteApp.Api.Services.QuestPDF;

public class QuestPdfService
{
    private readonly ILogger<QuestPdfService> _logger;
    private readonly PdfSettings _pdfSettings;

    public QuestPdfService(ILogger<QuestPdfService> logger, IOptions<PdfSettings> pdfSettings)
    {
        _logger = logger;
        _pdfSettings = pdfSettings.Value;
    }

    /// <summary>
    /// Génère un ticket PDF pour une vente validée
    /// </summary>
    /// <param name="vente">La vente à traiter</param>
    /// <returns>Les données du PDF en bytes</returns>
    public byte[] GenererTicketPdf(Vente vente)
    {
        if (vente == null)
            throw new ArgumentNullException(nameof(vente));

        if (vente.Client == null)
            throw new InvalidOperationException("La vente doit avoir un client associé");

        try
        {
            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);

                    page.Content().Column(column =>
                    {
                        // En-tête du ticket
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Text("CAVISTE APP")
                                .FontSize(20)
                                .Bold();

                            row.RelativeItem().AlignRight().Text("Ticket de Vente")
                                .FontSize(12)
                                .Bold();
                        });

                        column.Item().PaddingVertical(10).BorderBottom(1);

                        // Informations de la vente
                        column.Item().PaddingVertical(10).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("N° Vente").Bold().FontSize(10);
                                col.Item().Text($"#{vente.Id}").FontSize(11);
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Date").Bold().FontSize(10);
                                col.Item().Text(vente.Date.ToString("dd/MM/yyyy HH:mm")).FontSize(11);
                            });

                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("Statut").Bold().FontSize(10);
                                col.Item().Text(vente.Statut.ToString()).FontSize(11);
                            });
                        });

                        // Informations client
                        column.Item().PaddingVertical(10).Column(col =>
                        {
                            col.Item().Text("CLIENT").Bold().FontSize(11);
                            col.Item().Text($"Id Client : {vente.Client.Id}").FontSize(10);
                            col.Item().Text($"{vente.Client.NomComplet}").FontSize(10);
                            col.Item().Text($"{vente.Client.NumRue} {vente.Client.NomRue}").FontSize(10);
                            col.Item().Text($"{vente.Client.CodePostal} {vente.Client.Ville}").FontSize(10);
                            col.Item().Text($"Email: {vente.Client.Email}").FontSize(10);
                            col.Item().Text($"Téléphone: {vente.Client.Telephone}").FontSize(10);
                        });

                        column.Item().PaddingVertical(10).BorderTop(1);

                        // Tableau des articles
                        column.Item().PaddingVertical(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2);
                            });

                            // En-têtes du tableau
                            table.Header(header =>
                            {
                                header.Cell().Padding(5).Text("Produit").Bold().FontSize(10);
                                header.Cell().Padding(5).AlignCenter().Text("Qté").Bold().FontSize(10);
                                header.Cell().Padding(5).AlignRight().Text("P.U.").Bold().FontSize(10);
                                header.Cell().Padding(5).AlignRight().Text("Total").Bold().FontSize(10);
                            });

                            // Lignes de vente
                            foreach (var ligne in vente.Lignes)
                            {
                                table.Cell().Padding(5).Text(ligne.VinNom).FontSize(9);
                                table.Cell().Padding(5).AlignCenter().Text(ligne.Quantite.ToString()).FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text($"{ligne.PrixUnitaire:F2}€").FontSize(9);
                                table.Cell().Padding(5).AlignRight().Text($"{ligne.SousTotal:F2}€").FontSize(9);
                            }
                        });

                        column.Item().PaddingVertical(10).BorderTop(1);

                        // Résumé financier
                        column.Item().PaddingVertical(10).AlignRight().Column(col =>
                        {
                            var sousTotal = vente.Lignes.Sum(l => l.SousTotal);

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Sous-total:").Bold().FontSize(10);
                                row.RelativeItem().AlignRight().Text($"{sousTotal:F2}€").FontSize(10);
                            });

                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL:").Bold().FontSize(12);
                                row.RelativeItem().AlignRight().Text($"{vente.MontantTotal:F2}€").Bold().FontSize(12);
                            });
                        });

                        column.Item().PaddingTop(20).BorderTop(1);

                        // Pied de page
                        column.Item().PaddingTop(10).AlignCenter().Column(col =>
                        {
                            col.Item().Text("Merci de votre visite !").Italic().FontSize(10);
                            col.Item().Text($"Généré le {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}").FontSize(8);
                        });
                    });
                });
            }).GeneratePdf();

            _logger.LogInformation("Ticket PDF généré pour la vente #{VenteId}", vente.Id);
            return pdf;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du ticket PDF pour la vente #{VenteId}", vente.Id);
            throw;
        }
    }

    /// <summary>
    /// Génère et sauvegarde un ticket PDF sur le disque en utilisant le chemin par défaut de la configuration
    /// </summary>
    /// <param name="vente">La vente à traiter</param>
    /// <returns>Le chemin complet du fichier PDF généré</returns>
    public string SauvegarderTicketPdf(Vente vente)
    {
        return SauvegarderTicketPdf(vente, _pdfSettings.TicketsFolder);
    }

    /// <summary>
    /// Génère et sauvegarde un ticket PDF sur le disque
    /// </summary>
    /// <param name="vente">La vente à traiter</param>
    /// <param name="cheminDossier">Le chemin du dossier de destination</param>
    /// <returns>Le chemin complet du fichier PDF généré</returns>
    public string SauvegarderTicketPdf(Vente vente, string cheminDossier)
    {
        if (string.IsNullOrWhiteSpace(cheminDossier))
            throw new ArgumentException("Le chemin du dossier ne peut pas être vide", nameof(cheminDossier));

        try
        {
            // Créer le dossier s'il n'existe pas
            if (!Directory.Exists(cheminDossier))
            {
                Directory.CreateDirectory(cheminDossier);
            }

            var pdfBytes = GenererTicketPdf(vente);
            var nomFichier = $"ticket_vente_{vente.Id}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
            var cheminFichier = Path.Combine(cheminDossier, nomFichier);

            File.WriteAllBytes(cheminFichier, pdfBytes);

            _logger.LogInformation("Ticket PDF sauvegardé à: {CheminFichier}", cheminFichier);
            return cheminFichier;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde du ticket PDF pour la vente #{VenteId}", vente.Id);
            throw;
        }
    }
}
