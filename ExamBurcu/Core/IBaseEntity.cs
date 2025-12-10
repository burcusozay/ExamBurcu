namespace VaccineExam.Core
{
    public interface IBaseEntity<TKey>
    {
        TKey id { get; set; }
    }
}
