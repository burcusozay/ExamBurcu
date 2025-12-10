using ExamBurcu.Dtos;

namespace ExamBurcu.Interfaces
{
    public interface IChildService
    {
        Task<List<ChildDto>> GetAllAsync();
        Task<List<ChildDto>> GetListAsync(ChildRequestDto request);
        Task<List<MissingVaccineSummaryDto>> GetReportAsync(ChildRequestDto request);
        Task<ChildDto?> GetByIdAsync(int id);
        Task<ChildDto> AddAsync(ChildDto stock);
        Task<ChildDto?> UpdateAsync(int id, ChildDto updated);
        Task<bool> DeleteAsync(int id);
    }
}
