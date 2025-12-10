using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using ExamBurcu.Data;
using ExamBurcu.Dtos;
using ExamBurcu.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using VaccineExam.Core;
using VaccineExam.Repository;
using VaccineExam.UnitOfWork;

namespace ExamBurcu.Services
{
    public class ChildService : BaseService<child, ChildDto>, IChildService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<child, long> _childRepository;
        private readonly IRepository<vaccineapplication, long> _vaccineapplicationRepository;
        private readonly IRepository<vaccineschedule, long> _vaccinescheduleRepository;
        private readonly IRepository<vaccine, long> _vaccineRepository;

        public ChildService(IUnitOfWork unitOfWork, IMapper mapper)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _childRepository = _unitOfWork.GetRepository<child, long>();
            _vaccineRepository = _unitOfWork.GetRepository<vaccine, long>();
            _vaccineapplicationRepository = _unitOfWork.GetRepository<vaccineapplication, long>();
            _vaccinescheduleRepository = _unitOfWork.GetRepository<vaccineschedule, long>();
        }

        public async Task<List<ChildDto>> GetAllAsync()
        {
            var childList = await _childRepository.GetAllAsync();
            return MapToDtoList(childList);
        }

        public async Task<ChildDto?> GetByIdAsync(int id)
        {
            var childDto = await _childRepository.GetByIdAsync(id);
            return MapToDto(childDto);
        }

        public async Task<ChildDto> AddAsync(ChildDto model)
        {
            var childEntity = MapToEntity(model);
            childEntity = await _childRepository.InsertAsync(childEntity);
            return MapToDto(childEntity); ;
        }

        public async Task<ChildDto?> UpdateAsync(int id, ChildDto model)
        {
            var existing = await _childRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.namesurname = model.namesurname;
            existing.isactive = model.isactive;
            existing.isdeleted = model.isdeleted;
            existing.tckn = model.tckn;
            // diğer alanlar güncellenebilir...

            await _childRepository.UpdateAsync(existing);

            var childDto = MapToDto(existing);

            return childDto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _childRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _childRepository.DeleteAsync(existing);
            return true;
        }

        public async Task<List<ChildDto>> GetListAsync(ChildRequestDto request)
        {

            if(request.tckn.HasValue && (request.tckn > 99999999999 || request.tckn < 10000000000))
            {
                throw new Exception("TCKN 11 hane olmalıdır");
            }

            var entityList = _childRepository.AsQueryable().AsNoTracking();
            if (!string.IsNullOrEmpty(request.namesurname))
            {
                entityList = entityList.Where(x => x.namesurname.Contains(request.namesurname));
            }

             

            if (request.tckn < 100000000000 && request.tckn > 10000000000)
            {
                entityList = entityList.Where(x => x.tckn == request.tckn);
            }

            var list = await entityList
                                .Skip(request.page > 0 ? (request.page.Value - 1) : 0)
                                .Take(request.pagesize ?? 10)
                                .Select(v => new ChildDto
                                {
                                    id = v.id,
                                    //isdeleted = v.isdeleted,
                                    //createddate = v.createddate,
                                    //isactive = v.isactive,
                                    namesurname = v.namesurname,
                                    tckn = v.tckn,
                                    birthdate = v.birthdate
                                })
                                .ToListAsync();
            //return MapToDtoList(list);
            return list;
        }

        public async Task<List<MissingVaccineSummaryDto>> GetReportAsync(ChildRequestDto request)
        {
            // TODO toplam eksik uygulanan aşı sayısı

            var vaccineScheduleQuery = _vaccinescheduleRepository.AsQueryable().AsNoTracking()
                            .Where(vs => vs.isactive && !vs.isdeleted);

            var vaccineApplicationQuery = _vaccineapplicationRepository.AsQueryable().AsNoTracking()
                            .Where(vs => vs.isactive && !vs.isdeleted);
             
            var vaccineQuery = _vaccineRepository.AsQueryable().AsNoTracking()
                            .Where(vs => vs.isactive && !vs.isdeleted);
             

            var childQuery = _childRepository.AsQueryable().AsNoTracking()
                            .Where(vs => vs.isactive && !vs.isdeleted);

            // 1. ADIM: Her Çocuk İçin Gerekli ve Uygulanan Aşı Bilgilerini Hazırlama
            // Not: Bu Query, RequiredVaccines koleksiyonunu belleğe çekeceği için daha sonraki adımlar LINQ-to-Objects olacaktır.
            var intermediateQuery = childQuery
                .Where(c => c.birthdate.HasValue) // Doğum tarihi olanları filtrele
                .Select(c => new
                {
                    ChildId = c.id,
                    ChildTckn = c.tckn,
                    ChildNameSurname = c.namesurname,
                    BirthDate = c.birthdate.Value,

                    // **HATA ÇÖZÜMÜ 1:** RequiredVaccines koleksiyonunu List<T> olarak çekmek için ToList() çağrısı.
                    // Bu, içteki sorguyu veritabanında çalıştırır ve sonucu belleğe alır.
                    RequiredVaccines = vaccineScheduleQuery
                        .Select(vs => new
                        {
                            VaccineId = vs.vaccineid,
                            RequiredApplicationDate = c.birthdate.Value.AddMonths(vs.month ?? 0)
                        })
                        .ToList(),

                    // AppliedVaccineIds zaten navigasyon property'si üzerinden çekildiği için ToList() ile belleğe alınmalı.
                    AppliedVaccineIds = c.vaccineapplications
                        .Where(va => va.vaccineid.HasValue)
                        .Select(va => va.vaccineid)
                        .ToList()
                })
                .AsEnumerable(); // Bundan sonraki sorgu adımları LINQ-to-Objects üzerinde çalışacaktır.

            // 2. ADIM: Eksik Aşı Sayısını Hesaplama (LINQ-to-Objects)
            var reportQuery = intermediateQuery
                .Select(c => new
                {
                    c.ChildTckn,
                    c.ChildNameSurname,
                    c.BirthDate,

                    // Eksik Aşıları Filtrele:
                    // 1. Programda var VE Applied listesinde yok.
                    // 2. VE zamanı gelmiş (RequiredApplicationDate <= today)
                    MissingVaccineCount = c.RequiredVaccines
                        .Count(rv =>
                            rv.VaccineId.HasValue &&
                            !c.AppliedVaccineIds.Contains(rv.VaccineId) &&
                            rv.RequiredApplicationDate <= DateTime.Today
                        )
                })
                // Nihai DTO'ya dönüştürme
                .Select(result => new MissingVaccineSummaryDto
                {
                    Tckn = result.ChildTckn,
                    NameSurname = result.ChildNameSurname,
                    TotalMissingVaccineCount = result.MissingVaccineCount,
                });

            // 3. ADIM: Filtreleme, Sıralama ve Sayfalama

            // TCKN Filtreleme (Opsiyonel)
            if (request.tckn.HasValue)
            {
                reportQuery = reportQuery.Where(r => r.Tckn == request.tckn.Value);
            }

            // Sıralama
            reportQuery = reportQuery.OrderByDescending(x => x.TotalMissingVaccineCount);

            // Sayfalama parametrelerini Request DTO'dan al
            int page = request.page ?? 1;
            int pageSize = request.pagesize ?? 10;

            // Sayfalama Uygulama
            var finalReportList = reportQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList(); // LINQ-to-Objects olduğu için ToList() güvenli.

            return finalReportList;
        }
    }
}
