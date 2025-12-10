using ExamBurcu.Dtos;

namespace ExamBurcu.Interfaces
{
    public interface IVaccineApplicationService
    {
        Task<List<VaccineApplicationDto>> GetAllAsync();
        Task<List<VaccineApplicationDto>> GetListAsync(VaccineApplicationRequestDto request);
        Task<VaccineApplicationDto?> GetByIdAsync(int id);
        Task<VaccineApplicationDto> AddAsync(VaccineApplicationDto stock);
        Task<VaccineApplicationDto?> UpdateAsync(int id, VaccineApplicationDto updated);
        Task<bool> DeleteAsync(int id);
    }
}
