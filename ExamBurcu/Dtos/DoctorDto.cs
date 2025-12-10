using VaccineExam.Core;

namespace ExamBurcu.Dtos
{
    public class DoctorDto : BaseEntityDto<long>
    {
        public long tckn { get; set; }

        public string? namesurname { get; set; }
         
    }
}
