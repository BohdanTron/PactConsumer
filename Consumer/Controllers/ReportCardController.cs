using Microsoft.AspNetCore.Mvc;

namespace Consumer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportCardController : ControllerBase
    {
        private readonly IStudentApiClient _studentApiClient;

        public ReportCardController(IStudentApiClient studentApiClient)
        {
            _studentApiClient = studentApiClient;
        }

        [HttpGet]
        [Route("student/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentApiClient.GetStudentById(id);

            if (student is null)
                return NotFound();

            var report = new ReportCard
            {
                Id = Random.Shared.Next(),
                Student = student,
                Score = 2 * 100
            };

            return Ok(report);
        }
    }
}
