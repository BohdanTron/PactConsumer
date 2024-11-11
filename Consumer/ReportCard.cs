namespace Consumer;

public class ReportCard
{
    public required int Id { get; set; }
    public required Student Student { get; set; }
    public required double Score { get; set; }
    public bool IsPassed => Score > 5d;
}