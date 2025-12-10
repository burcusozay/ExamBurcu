using ExamBurcu.Dtos;

namespace ExamBurcu.Interfaces
{
    public interface IVaccineScheduleService
    {
        Task<List<VaccineScheduleDto>> GetAllAsync();
        Task<List<VaccineScheduleDto>> GetListAsync(VaccineScheduleRequestDto request);
        Task<VaccineScheduleDto?> GetByIdAsync(int id);
        Task<VaccineScheduleDto> AddAsync(VaccineScheduleDto stock);
        Task<VaccineScheduleDto?> UpdateAsync(int id, VaccineScheduleDto updated);
        Task<bool> DeleteAsync(int id);
    }
}
