using CampusServicePortal.DTOs.Students;
using CampusServicePortal.Models;
using CampusServicePortal.Repositories.Interfaces;
using CampusServicePortal.Services.Interfaces;

namespace CampusServicePortal.Services.Implementation
{
    public class StudentMasterListService : IStudentMasterListService
    {
        private readonly IStudentMasterListRepository _masterListRepository;

        public StudentMasterListService(IStudentMasterListRepository masterListRepository)
        {
            _masterListRepository = masterListRepository;
        }

        public async Task<MasterListRecordDto> VerifyIndexNumberAsync(string indexNumber)
        {
            var record = await _masterListRepository.GetByIndexNumberAsync(indexNumber)
                ?? throw new KeyNotFoundException(
                    $"Index number '{indexNumber}' was not found in the university master list. " +
                    "Please contact the Registrar's office if you believe this is an error.");

            return MapToDto(record);
        }

        public async Task<IEnumerable<MasterListRecordDto>> SearchMasterListAsync(string? search)
        {
            var records = await _masterListRepository.GetAllAsync(search);
            return records.Select(MapToDto);
        }

        public async Task<int> ImportFromCsvAsync(Stream csvStream)
        {
            var imported = new List<StudentMasterList>();

            using var reader = new StreamReader(csvStream);
            // Skip header line
            var header = await reader.ReadLineAsync();
            if (header == null) return 0;

            int lineNumber = 1;
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = line.Split(',');
                if (cols.Length < 5)
                    throw new FormatException(
                        $"CSV line {lineNumber} has fewer than 5 columns. " +
                        "Expected: IndexNumber,FullName,Faculty,DegreeProgram,EnrollmentYear");

                if (!int.TryParse(cols[4].Trim(), out int year))
                    throw new FormatException(
                        $"CSV line {lineNumber}: EnrollmentYear '{cols[4].Trim()}' is not a valid integer.");

                // Check if already in master list — skip duplicates silently
                var existing = await _masterListRepository.GetByIndexNumberAsync(cols[0].Trim());
                if (existing != null) continue;

                imported.Add(new StudentMasterList
                {
                    IndexNumber = cols[0].Trim(),
                    FullName = cols[1].Trim(),
                    Faculty = cols[2].Trim(),
                    DegreeProgram = cols[3].Trim(),
                    EnrollmentYear = year,
                    IsRegistered = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (imported.Any())
            {
                await _masterListRepository.AddRangeAsync(imported);
                await _masterListRepository.SaveChangesAsync();
            }

            return imported.Count;
        }

        private static MasterListRecordDto MapToDto(StudentMasterList record)
        {
            return new MasterListRecordDto
            {
                IndexNumber = record.IndexNumber,
                FullName = record.FullName,
                Faculty = record.Faculty,
                DegreeProgram = record.DegreeProgram,
                EnrollmentYear = record.EnrollmentYear,
                IsRegistered = record.IsRegistered
            };
        }
    }
}
