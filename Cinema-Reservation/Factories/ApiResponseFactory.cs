using Microsoft.AspNetCore.Mvc;
using Shared.ErrorModels;

namespace Cinema_Reservation.Factories
{
    public static class ApiResponseFactory
    {
        public static IActionResult GenerateApiValidationErrorResponse(ActionContext context)
        {
            var errors = context.ModelState
                .Where(e => e.Value.Errors.Count > 0)
                .Select(e => new ValidationError
                {
                    Field = e.Key,
                    Errors = e.Value.Errors.Select(x => x.ErrorMessage).ToList()
                }).ToList(); 
            var validationResponse = new ValidationErrorStruct
            {
                Errors = errors
            };

            return new BadRequestObjectResult(validationResponse);
        }
    }
}
