using System.Collections.Generic;

namespace VendorManagementprojApplication.DTOs;

public class SpoilageAiNarrativeDto
{
    public int ProductID { get; set; }
    public string Why { get; set; } = string.Empty;
    public string RecommendedAction { get; set; } = string.Empty;
}

public class SpoilageAdvisorAiResultDto
{
    public string OverallSummary { get; set; } = string.Empty;
    public List<SpoilageAiNarrativeDto> ProductNarratives { get; set; } = new();
}
