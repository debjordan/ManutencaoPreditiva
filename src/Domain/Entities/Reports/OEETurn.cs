namespace ManutencaoPreditiva.Domain.Entities;
public class OEETurn
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string? Turn { get; set; }
    public Guid TurnId { get; set; }
    public decimal OeeTurn { get; set; }
}
