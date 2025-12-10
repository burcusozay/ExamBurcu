using VaccineExam.Core;
using VaccineExam.Core.Core;

namespace ExamBurcu.Dtos
{
    public class ChildRequestDto : BaseRequestDto
    {
        public long? tckn { get; set; }

        public string? namesurname { get; set; }

        public DateTime? birthdate { get; set; }
    }
}
