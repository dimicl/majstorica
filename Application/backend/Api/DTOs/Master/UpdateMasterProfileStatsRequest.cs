namespace backend.Api.DTOs.Master;

/// <summary>PATCH delova profila majstora (iskustvo, satnica). Bar jedno polje mora biti poslato.</summary>
public class UpdateMasterProfileStatsRequest
{
    public int? YearsOfExperience { get; set; }
    public decimal? HourlyRateAmount { get; set; }
    public string? HourlyRateCurrency { get; set; }
}
