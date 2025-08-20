using KbApi.Models;

namespace KbApi.Services;

public class PlanService
{
    public record PlanCaps(int MaxDocs, long MaxFileBytes, int MonthlyQueries);

    public PlanCaps GetCaps(PlanType plan) => plan switch
    {
        PlanType.Starter => new PlanCaps(50, 5 * 1024L * 1024L, 300),
        PlanType.Pro => new PlanCaps(200, 20 * 1024L * 1024L, 2000),
        PlanType.Team => new PlanCaps(1000, 50 * 1024L * 1024L, 10000),
        _ => new PlanCaps(50, 5 * 1024L * 1024L, 300)
    };
}

