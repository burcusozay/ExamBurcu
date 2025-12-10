namespace VaccineExam.Core.Core
{
    [Serializable]
    public abstract class BaseRequestDto
    {  
        public int? pagesize { get; set; }
        public int? page { get; set; }
        public bool? isdeleted { get; set; } = false;
        public bool? isactive { get; set; } = true; 
    }
}
