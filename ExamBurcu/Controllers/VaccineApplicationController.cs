using ExamBurcu.Dtos;
using ExamBurcu.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExamBurcu.Controllers
{ 
        [Route("api/[controller]")]
        [ApiController]
        public class VaccineApplicationController : ControllerBase
        {
            private readonly IVaccineApplicationService _vaccineApplicationService;
            public VaccineApplicationController(IVaccineApplicationService vaccineApplicationService)
            {
                _vaccineApplicationService = vaccineApplicationService;
            }

            [HttpGet("{id}")]
            public async Task<IActionResult> Get(int id)
            {
                var result = await _vaccineApplicationService.GetByIdAsync(id);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }

            [HttpGet("GetList")]
            public async Task<IActionResult> GetList([FromQuery] VaccineApplicationRequestDto model)
            {
                var result = await _vaccineApplicationService.GetListAsync(model);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }

            [HttpPost("Add")]
            public async Task<IActionResult> Add([FromBody] VaccineApplicationDto model)
            {
                var created = await _vaccineApplicationService.AddAsync(model);
                return CreatedAtAction(nameof(Get), new { id = created.id }, created);
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> Update(int id, [FromBody] VaccineApplicationDto model)
            {
                // İsteğe bağlı ama önerilen: URL'deki id ile body'deki id'nin aynı olduğunu kontrol et.
                if (id != model.id)
                {
                    return BadRequest("URL ID ile gövde (body) ID'si uyuşmuyor.");
                }

                var updatedDto = await _vaccineApplicationService.UpdateAsync(id, model);

                if (updatedDto == null)
                {
                    // Güncellenmek istenen kaynak bulunamadıysa.
                    return NotFound();
                }

                // DÜZELTME 2: Başarılı bir PUT isteği için 204 No Content veya 200 OK dönmek daha doğrudur.
                return NoContent(); // Başarılı, yanıt gövdesinde içerik yok.
                                    // Alternatif olarak güncellenmiş nesneyi de dönebilirsiniz: return Ok(updatedDto);
            }
        }
    
}
