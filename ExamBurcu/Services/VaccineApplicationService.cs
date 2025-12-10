using AutoMapper;
using ExamBurcu.Data;
using ExamBurcu.Dtos;
using ExamBurcu.Interfaces;
using Microsoft.EntityFrameworkCore;
using VaccineExam.Core;
using VaccineExam.Repository;
using VaccineExam.UnitOfWork;

namespace ExamBurcu.Services
{

    public class VaccineApplicationService : BaseService<vaccineapplication, VaccineApplicationDto>, IVaccineApplicationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<vaccineapplication, long> _vaccineapplicationRepository;
        private readonly IRepository<vaccine, long> _vaccineRepository;

        public VaccineApplicationService(IUnitOfWork unitOfWork, IMapper mapper)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _vaccineapplicationRepository = _unitOfWork.GetRepository<vaccineapplication, long>();
            _vaccineRepository = _unitOfWork.GetRepository<vaccine, long>();
        }

        public async Task<List<VaccineApplicationDto>> GetAllAsync()
        {
            var vaccineapplicationList = await _vaccineapplicationRepository.GetAllAsync();
            return MapToDtoList(vaccineapplicationList);
        }

        public async Task<VaccineApplicationDto?> GetByIdAsync(int id)
        {
            var vaccineapplicationDto = await _vaccineapplicationRepository.GetByIdAsync(id);
            return MapToDto(vaccineapplicationDto);
        }

        public async Task<VaccineApplicationDto> AddAsync(VaccineApplicationDto model)
        {
            if (model.childid.HasValue && model.vaccineid.HasValue && model.applicationtime.HasValue)
            { 
                var applicationDate = model.applicationtime.Value.Date; 
                var applicationStartOfDay = applicationDate; 
                var applicationEndOfDay = applicationDate.AddDays(1);

                var isDuplicate = await _vaccineapplicationRepository.AsQueryable()
                    .AnyAsync(va =>
                        va.childid == model.childid.Value &&
                        va.vaccineid == model.vaccineid.Value &&
                        va.applicationtime.HasValue && 
                        va.applicationtime.Value >= applicationStartOfDay &&
                        va.applicationtime.Value < applicationEndOfDay
                    );

                if (isDuplicate)
                {
                    throw new InvalidOperationException($"Hata: {model.childid.Value} ID'li çocuğa, {applicationDate:dd.MM.yyyy} tarihinde, aynı aşı zaten uygulanmıştır. Aynı gün ikinci kez uygulanamaz.");
                }
            }

            // 2. Stok Güncelleme
            if (model.vaccineid.HasValue)
            {
                var vaccineEntity = await _vaccineRepository.GetByIdAsync(model.vaccineid.Value);

                if (vaccineEntity != null && vaccineEntity.stockcount > 0)
                {
                    vaccineEntity.stockcount = vaccineEntity.stockcount - 1; 
                    await _vaccineRepository.UpdateAsync(vaccineEntity);
                }
                else
                {
                    throw new Exception("Stok yetersiz");
                }
            }

            var vaccineapplicationEntity = MapToEntity(model);
            vaccineapplicationEntity = await _vaccineapplicationRepository.InsertAsync(vaccineapplicationEntity);

            return MapToDto(vaccineapplicationEntity); ;
        }

        public async Task<VaccineApplicationDto?> UpdateAsync(int id, VaccineApplicationDto model)
        {
            var existing = await _vaccineapplicationRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.isactive = model.isactive;
            existing.isdeleted = model.isdeleted;

            existing.applicationtime = model.applicationtime;
            existing.childid = model.childid;
            existing.vaccineid = model.vaccineid;
            existing.doctorid = model.doctorid;
            existing.description = model.description;
             
            // 1. Stoğu Geri Ekleme (Aşı ID'si varsa)
            if (existing.vaccineid.HasValue && ((existing.isactive && !model.isactive) || (existing.isdeleted && !model.isdeleted))) 
            {
                var vaccineEntity = await _vaccineRepository.GetByIdAsync(existing.vaccineid.Value);
                if (vaccineEntity != null)
                {
                    // Stoğu 1 artır
                    vaccineEntity.stockcount = vaccineEntity.stockcount + 1;
                    await _vaccineRepository.UpdateAsync(vaccineEntity);
                }
            } 

            await _vaccineapplicationRepository.UpdateAsync(existing);
            var vaccineapplicationDto = MapToDto(existing);

            return vaccineapplicationDto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _vaccineapplicationRepository.GetByIdAsync(id);
            if (existing is null) return false;

            // 1. Stoğu Geri Ekleme (Aşı ID'si varsa)
            if (existing.vaccineid.HasValue)
            {
                var vaccineEntity = await _vaccineRepository.GetByIdAsync(existing.vaccineid.Value); 
                if (vaccineEntity != null)
                {
                    // Stoğu 1 artır
                    vaccineEntity.stockcount += 1;
                    await _vaccineRepository.UpdateAsync(vaccineEntity);
                }
            }

            // 2. Uygulama Kaydını Silme
            await _vaccineapplicationRepository.DeleteAsync(existing);
            return true;
             
        }

        public async Task<List<VaccineApplicationDto>> GetListAsync(VaccineApplicationRequestDto request)
        {

            var entityList = _vaccineapplicationRepository.AsQueryable().AsNoTracking();


            var list = await entityList
                                .Skip(request.page > 0 ? (request.page.Value - 1) : 0)
                                .Take(request.pagesize ?? 10)
                                .Select(v => new VaccineApplicationDto
                                {
                                    id = v.id,
                                    isdeleted = v.isdeleted,
                                    createddate = v.createddate,
                                    isactive = v.isactive,
                                    childid = v.childid,
                                    doctorid = v.doctorid,
                                    vaccineid = v.vaccineid,
                                    applicationtime = v.applicationtime,
                                    description = v.description
                                })
                                .ToListAsync();
            //return MapToDtoList(list);
            return list;
        }
    }
}
