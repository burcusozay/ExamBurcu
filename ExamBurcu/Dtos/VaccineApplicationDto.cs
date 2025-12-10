using VaccineExam.Core;

namespace ExamBurcu.Dtos
{
    public class VaccineApplicationDto : BaseEntityDto<long>
    {
        public DateTime? applicationtime { get; set; }

        public long? vaccineid { get; set; }

        public long? doctorid { get; set; }

        public long? childid { get; set; }

        public string? description { get; set; }
    }
}
