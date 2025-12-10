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

    public class DoctorService : BaseService<doctor, DoctorDto>, IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<doctor, long> _doctorRepository;

        public DoctorService(IUnitOfWork unitOfWork, IMapper mapper)
            : base(mapper)
        {
            _unitOfWork = unitOfWork;
            _doctorRepository = _unitOfWork.GetRepository<doctor, long>();
        }

        public async Task<List<DoctorDto>> GetAllAsync()
        {
            var doctorList = await _doctorRepository.GetAllAsync();
            return MapToDtoList(doctorList);
        }

        public async Task<DoctorDto?> GetByIdAsync(int id)
        {
            var doctorDto = await _doctorRepository.GetByIdAsync(id);
            return MapToDto(doctorDto);
        }

        public async Task<DoctorDto> AddAsync(DoctorDto model)
        {
            var doctorEntity = MapToEntity(model);
            doctorEntity = await _doctorRepository.InsertAsync(doctorEntity);
            return MapToDto(doctorEntity); ;
        }

        public async Task<DoctorDto?> UpdateAsync(int id, DoctorDto model)
        {
            var existing = await _doctorRepository.GetByIdAsync(id);
            if (existing is null) return null;

            existing.namesurname = model.namesurname;
            existing.isactive = model.isactive;
            existing.isdeleted = model.isdeleted;
            existing.tckn = model.tckn;
            // diğer alanlar güncellenebilir...

            await _doctorRepository.UpdateAsync(existing);

            var doctorDto = MapToDto(existing);

            return doctorDto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _doctorRepository.GetByIdAsync(id);
            if (existing is null) return false;

            await _doctorRepository.DeleteAsync(existing);
            return true;
        }

        public async Task<List<DoctorDto>> GetListAsync(DoctorRequestDto request)
        {

            if (request.tckn.HasValue &&  (request.tckn > 99999999999 || request.tckn < 10000000000))
            {
                throw new Exception("TCKN 11 hane olmalıdır");
            }

            var entityList = _doctorRepository.AsQueryable().AsNoTracking();
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
                                .Select(v => new DoctorDto
                                {
                                    id = v.id,
                                    isdeleted = v.isdeleted,
                                    createddate = v.createddate,
                                    isactive = v.isactive,
                                    namesurname = v.namesurname,
                                    tckn = v.tckn
                                })
                                .ToListAsync();
            //return MapToDtoList(list);
            return list;
        }
    }
}
