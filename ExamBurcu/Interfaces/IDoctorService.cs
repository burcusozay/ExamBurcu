using ExamBurcu.Dtos;

namespace ExamBurcu.Interfaces
{
    public interface IDoctorService
    {
        Task<List<DoctorDto>> GetAllAsync();
        Task<List<DoctorDto>> GetListAsync(DoctorRequestDto request);
        Task<DoctorDto?> GetByIdAsync(int id);
        Task<DoctorDto> AddAsync(DoctorDto stock);
        Task<DoctorDto?> UpdateAsync(int id, DoctorDto updated);
        Task<bool> DeleteAsync(int id);
    }
}
