using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace ComputerRepairs.Filters
{
    public class LogFieldValidationAttribute : ActionFilterAttribute
    {

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new
                {
                    Field = e.Key,
                    Error = e.Value?.Errors.First().ErrorMessage
                }).ToList();

                Log.Warning(
                    "Missing or invalid fields in POST /api/items: {Errors}",
                    errors
                );
            }
        }
    }
}
