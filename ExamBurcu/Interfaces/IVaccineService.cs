using ExamBurcu.Dtos;

namespace ExamBurcu.Interfaces
{
    public interface IVaccineService
    {
        Task<List<VaccineDto>> GetAllAsync();
        Task<List<VaccineDto>> GetListAsync(VaccineRequestDto request);
        Task<VaccineDto?> GetByIdAsync(int id);
        Task<VaccineDto> AddAsync(VaccineDto stock);
        Task<VaccineDto?> UpdateAsync(int id, VaccineDto updated);
        Task<bool> DeleteAsync(int id);
    }
}
