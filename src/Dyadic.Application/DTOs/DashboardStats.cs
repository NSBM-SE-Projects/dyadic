namespace Dyadic.Application.DTOs;

public class DashboardStats
{
    public int TotalProposals { get; set; }
    public int DraftCount { get; set; }
    public int SubmittedCount { get; set; }
    public int AcceptedCount { get; set; }
    public int FinalizedCount { get; set; }
    public int TotalStudents { get; set; }
    public int TotalSupervisors { get; set; }
}
