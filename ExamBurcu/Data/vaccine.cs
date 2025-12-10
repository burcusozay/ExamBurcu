using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VaccineExam.Core;

namespace ExamBurcu.Data;

[Serializable]
[Index(nameof(code), IsUnique = true)]
public partial class vaccine : BaseEntity<long>
{
    [MaxLength(70)]
    public string code { get; set; } = null!;

    [MaxLength(50)]
    public string? name { get; set; }

    public int? stockcount { get; set; }

    public virtual ICollection<vaccineapplication> vaccineapplications { get; set; } = new List<vaccineapplication>();

    public virtual ICollection<vaccineschedule> vaccineschedules { get; set; } = new List<vaccineschedule>();
}
