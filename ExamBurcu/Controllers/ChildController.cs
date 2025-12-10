using ExamBurcu.Dtos;
using ExamBurcu.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamBurcu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildController : ControllerBase
    {
        private readonly IChildService _childService;
        private readonly IExcelService _excelService; // Excel servisini inject et
        public ChildController(IChildService childService, IExcelService excelService)
        {
            _childService = childService;
            _excelService = excelService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _childService.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpGet("GetList")]
        public async Task<IActionResult> GetList([FromQuery] ChildRequestDto model)
        {
            var result = await _childService.GetListAsync(model);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] ChildDto model)
        {
            var created = await _childService.AddAsync(model);
            return CreatedAtAction(nameof(Get), new { id = created.id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ChildDto model)
        {
            // İsteğe bağlı ama önerilen: URL'deki id ile body'deki id'nin aynı olduğunu kontrol et.
            if (id != model.id)
            {
                return BadRequest("URL ID ile gövde (body) ID'si uyuşmuyor.");
            }

            var updatedDto = await _childService.UpdateAsync(id, model);

            if (updatedDto == null)
            {
                // Güncellenmek istenen kaynak bulunamadıysa.
                return NotFound();
            }

            // DÜZELTME 2: Başarılı bir PUT isteği için 204 No Content veya 200 OK dönmek daha doğrudur.
            return NoContent(); // Başarılı, yanıt gövdesinde içerik yok.
                                // Alternatif olarak güncellenmiş nesneyi de dönebilirsiniz: return Ok(updatedDto);
        }

        [HttpPost("Excel")] // Filtreleri body'de almak için POST
        //[Authorize(Roles = "admin")]
        public async Task<IActionResult> ExportExcel([FromBody] ChildRequestDto model)
        {
            // 1. Sayfalama olmadan filtrelenmiş tüm veriyi al
            var dataToExport = await _childService.GetReportAsync(model);

            // 2. Excel servisi ile dosyayı byte dizisine çevir
            var fileBytes = await _excelService.ExportToExcelAsync(dataToExport);

            // 3. Dosyayı kullanıcıya gönder
            string fileName = $"Excel_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }

}
