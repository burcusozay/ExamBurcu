using VaccineExam.Core;
using VaccineExam.Core.Core;

namespace ExamBurcu.Dtos
{
    public class VaccineApplicationRequestDto : BaseRequestDto
    {
        public DateTime? applicationtime { get; set; }

        public long? vaccineid { get; set; }

        public long? doctorid { get; set; }

        public long? childid { get; set; }

        public string? description { get; set; }
    }
}
