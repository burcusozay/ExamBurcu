using VaccineExam.Core;

namespace ExamBurcu.Dtos
{
    public class ChildDto : BaseEntityDto<long>
    {
        public long tckn { get; set; }

        public string? namesurname { get; set; }

        public DateTime? birthdate { get; set; }
    }
}
