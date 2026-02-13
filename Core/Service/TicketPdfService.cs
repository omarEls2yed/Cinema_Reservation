using DomainLayer.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
namespace Service
{
    public class TicketPdfService
    {
        public TicketPdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GeneratePdf(Ticket ticket)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A6.Landscape());
                    page.Margin(1, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Arial));

                    page.Header()
                        .Text("CINEMA TICKET")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium).AlignCenter();

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Item().Text(text =>
                            {
                                text.Span("Event: ").Bold();
                                text.Span(ticket.Event.Name).FontSize(14);
                            });

                            column.Item().Text(text =>
                            {
                                text.Span("Date: ").Bold();
                                text.Span($"{ticket.Event.EventDate:yyyy-MM-dd}");
                            });

                            column.Item().Text(text =>
                            {
                                text.Span("Seat Number: ").Bold();
                                text.Span(ticket.Seat.Number.ToString());
                            });

                            column.Item().PaddingTop(10).Text(text =>
                            {
                                text.Span("Price: ").Bold();
                                text.Span($"${ticket.Price}").FontColor(Colors.Green.Medium).FontSize(16).Bold();
                            });

                            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Darken3);

                            column.Item().AlignCenter().Text($"Ticket ID: {ticket.Id}").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
}
