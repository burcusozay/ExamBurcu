using VaccineExam.Core;
using VaccineExam.Core.Core;

namespace ExamBurcu.Dtos
{
    public class DoctorRequestDto : BaseRequestDto
    {
        public long? tckn { get; set; }

        public string? namesurname { get; set; } 
    }
}
