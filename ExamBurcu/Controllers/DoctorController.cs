using ExamBurcu.Dtos;
using ExamBurcu.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ExamBurcu.Controllers
{ 
        [Route("api/[controller]")]
        [ApiController]
        public class DoctorController : ControllerBase
        {
            private readonly IDoctorService _doctorService;
            public DoctorController(IDoctorService doctorService)
            {
                _doctorService = doctorService;
            }

            [HttpGet("{id}")]
            public async Task<IActionResult> Get(int id)
            {
                var result = await _doctorService.GetByIdAsync(id);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }

            [HttpGet("GetList")]
            public async Task<IActionResult> GetList([FromQuery] DoctorRequestDto model)
            {
                var result = await _doctorService.GetListAsync(model);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }

            [HttpPost("Add")]
            public async Task<IActionResult> Add([FromBody] DoctorDto model)
            {
                var created = await _doctorService.AddAsync(model);
                return CreatedAtAction(nameof(Get), new { id = created.id }, created);
            }

            [HttpPut("{id}")]
            public async Task<IActionResult> Update(int id, [FromBody] DoctorDto model)
            {
                // İsteğe bağlı ama önerilen: URL'deki id ile body'deki id'nin aynı olduğunu kontrol et.
                if (id != model.id)
                {
                    return BadRequest("URL ID ile gövde (body) ID'si uyuşmuyor.");
                }

                var updatedDto = await _doctorService.UpdateAsync(id, model);

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
