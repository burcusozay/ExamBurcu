using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using VaccineExam.Core;

namespace ExamBurcu.Data;

[Serializable]
[Index(nameof(month), nameof(vaccineid), IsUnique = true)]
public partial class vaccineschedule : BaseEntity<long>
{ 

    public long? vaccineid { get; set; }

    public short? month { get; set; }

    public virtual vaccine? vaccine { get; set; }
}
