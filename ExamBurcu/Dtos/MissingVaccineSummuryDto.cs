namespace ExamBurcu.Dtos
{
    public class MissingVaccineSummaryDto
    {
        public long Tckn { get; set; }
        public string? NameSurname { get; set; }
        public int TotalMissingVaccineCount { get; set; }
        // public List<string>? MissingVaccineDetails { get; set; } // Opsiyonel
    }
}
