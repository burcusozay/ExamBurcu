using ExamBurcu.Data;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VaccineExam.UnitOfWork
{
    public class AuditActionFilter : IAsyncActionFilter
    {
        //private readonly IAuditService _auditService;
        private readonly AppDbContext _context;

        //public AuditActionFilter(IAuditService auditService, AppDbContext context)
        public AuditActionFilter( AppDbContext context)
        {
            //_auditService = auditService;
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();

            if (resultContext.Exception == null || resultContext.ExceptionHandled)
            {
                //await _auditService.SaveAuditLogsAsync();
                await _context.SaveChangesAsync();
            }
        }
    }
}