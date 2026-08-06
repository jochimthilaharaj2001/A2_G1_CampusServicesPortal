using CampusServicePortal.DTOs.Students;

namespace CampusServicePortal.Services.Interfaces
{
    public interface IStudentMasterListService
    {
        Task<MasterListRecordDto> VerifyIndexNumberAsync(string indexNumber);
        Task<IEnumerable<MasterListRecordDto>> SearchMasterListAsync(string? search);
        Task<int> ImportFromCsvAsync(Stream csvStream);
    }
}
