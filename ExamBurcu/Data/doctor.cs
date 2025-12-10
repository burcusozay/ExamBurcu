using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VaccineExam.Core;

namespace ExamBurcu.Data;

[Serializable]

[Index(nameof(tckn), IsUnique = true)]
public partial class doctor : BaseEntity<long>
{ 
    public long tckn { get; set; }
    [MaxLength(80)]
    public string? namesurname { get; set; }

    public virtual ICollection<vaccineapplication> vaccineapplications { get; set; } = new List<vaccineapplication>();
}
