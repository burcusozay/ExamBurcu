using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using VaccineExam.Core;

namespace ExamBurcu.Data;

[Serializable]
public partial class vaccineapplication : BaseEntity<long>
{ 

    public DateTime? applicationtime { get; set; }

    public long? vaccineid { get; set; }

    public long? doctorid { get; set; }

    public long? childid { get; set; }

    [MaxLength(100)]
    public string? description { get; set; }

    public virtual child? child { get; set; }

    public virtual doctor? doctor { get; set; }

    public virtual vaccine? vaccine { get; set; }
}
