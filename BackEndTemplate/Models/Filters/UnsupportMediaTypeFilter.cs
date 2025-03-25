using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BackEndTemplate.Models.Filters
{
    //自定义不支持的MediaType 返回415
    public class UnsupportMediaTypeFilter : IActionFilter, IOrderedFilter
    {
        public int Order { get; set; } = -3000;

        /// <inheritdoc />
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (HasUnsupportedContentTypeError(context))
            {
                context.Result = new UnsupportedMediaTypeResult();
            }
        }
        private static bool HasUnsupportedContentTypeError(ActionExecutingContext context)
        {
            var modelState = context.ModelState;
            if (modelState.IsValid)
            {
                return false;
            }

            foreach (var kvp in modelState)
            {
                var errors = kvp.Value.Errors;
                for (var i = 0; i < errors.Count; i++)
                {
                    var error = errors[i];
                    if (error.Exception is UnsupportedContentTypeException)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <inheritdoc />
        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

    }
}
