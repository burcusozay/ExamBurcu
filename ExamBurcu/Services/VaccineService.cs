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

    public class VaccineService : BaseService<vaccine, VaccineDto>, IVaccineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<vaccine, long> _vaccineRepository;

        public VaccineService(IUnitOfWork unitOfWork, IMapper mapper)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _vaccineRepository = _unitOfWork.GetRepository<vaccine, long>();
        }

        public async Task<List<VaccineDto>> GetAllAsync()
        {
            var vaccineList = await _vaccineRepository.GetAllAsync();
            return MapToDtoList(vaccineList);
        }

        public async Task<VaccineDto?> GetByIdAsync(int id)
        {
            var vaccineDto = await _vaccineRepository.GetByIdAsync(id);
            return MapToDto(vaccineDto);
        }

        public async Task<VaccineDto> AddAsync(VaccineDto model)
        {
            var vaccineEntity = MapToEntity(model);
            vaccineEntity = await _vaccineRepository.InsertAsync(vaccineEntity);
            return MapToDto(vaccineEntity); ;
        }

        public async Task<VaccineDto?> UpdateAsync(int id, VaccineDto model)
        {
            var existing = await _vaccineRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.isactive = model.isactive;
            existing.isdeleted = model.isdeleted;
            //existing.applicationtime = model.ap;
            // diğer alanlar güncellenebilir...

            await _vaccineRepository.UpdateAsync(existing);

            var vaccineDto = MapToDto(existing);

            return vaccineDto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _vaccineRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _vaccineRepository.DeleteAsync(existing);
            return true;
        }

        public async Task<List<VaccineDto>> GetListAsync(VaccineRequestDto request)
        {

            var entityList = _vaccineRepository.AsQueryable().AsNoTracking();


            var list = await entityList
                                .Skip(request.page > 0 ? (request.page.Value - 1) : 0)
                                .Take(request.pagesize ?? 10)
                                .Select(v => new VaccineDto
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
