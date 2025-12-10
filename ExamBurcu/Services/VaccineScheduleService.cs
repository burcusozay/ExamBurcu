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
    public class VaccineScheduleService : BaseService<vaccineschedule, VaccineScheduleDto>, IVaccineScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<vaccineschedule, long> _vaccinescheduleRepository;

        public VaccineScheduleService(IUnitOfWork unitOfWork, IMapper mapper)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _vaccinescheduleRepository = _unitOfWork.GetRepository<vaccineschedule, long>();
        }

        public async Task<List<VaccineScheduleDto>> GetAllAsync()
        {
            var vaccinescheduleList = await _vaccinescheduleRepository.GetAllAsync();
            return MapToDtoList(vaccinescheduleList);
        }

        public async Task<VaccineScheduleDto?> GetByIdAsync(int id)
        {
            var vaccinescheduleDto = await _vaccinescheduleRepository.GetByIdAsync(id);
            return MapToDto(vaccinescheduleDto);
        }

        public async Task<VaccineScheduleDto> AddAsync(VaccineScheduleDto model)
        {
            var vaccinescheduleEntity = MapToEntity(model);
            vaccinescheduleEntity = await _vaccinescheduleRepository.InsertAsync(vaccinescheduleEntity);
            return MapToDto(vaccinescheduleEntity); ;
        }

        public async Task<VaccineScheduleDto?> UpdateAsync(int id, VaccineScheduleDto model)
        {
            var existing = await _vaccinescheduleRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.isactive = model.isactive;
            existing.isdeleted = model.isdeleted;
            //existing.applicationtime = model.ap;
            // diğer alanlar güncellenebilir...

            await _vaccinescheduleRepository.UpdateAsync(existing);

            var vaccinescheduleDto = MapToDto(existing);

            return vaccinescheduleDto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _vaccinescheduleRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _vaccinescheduleRepository.DeleteAsync(existing);
            return true;
        }

        public async Task<List<VaccineScheduleDto>> GetListAsync(VaccineScheduleRequestDto request)
        {

            var entityList = _vaccinescheduleRepository.AsQueryable().AsNoTracking();


            var list = await entityList
                                .Skip(request.page > 0 ? (request.page.Value - 1) : 0)
                                .Take(request.pagesize ?? 10)
                                .Select(v => new VaccineScheduleDto
                                {
                                    id = v.id,
                                    isdeleted = v.isdeleted,
                                    createddate = v.createddate,
                                    isactive = v.isactive,
                                })
                                .ToListAsync();
            //return MapToDtoList(list);
            return list;
        }
    }
}
