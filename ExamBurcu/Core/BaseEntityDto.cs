using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VaccineExam.Core
{
    [Serializable]
    public abstract class BaseEntityDto<TKey> : IBaseEntityDto<TKey>
    { 
        public TKey id { get; set; }
        public bool isdeleted { get; set; } = false;
        public bool isactive { get; set; } = true;
        public DateTime? createddate { get; set; } 
    }
}
