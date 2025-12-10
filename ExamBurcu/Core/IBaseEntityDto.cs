namespace VaccineExam.Core
{
    public interface IBaseEntityDto<TKey>
    {
        TKey id { get; set; }
    }
}
