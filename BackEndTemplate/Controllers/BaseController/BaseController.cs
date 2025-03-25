using BackEndTemplate.Models.Attributes;
using BackEndTemplate.Models.DataBaseModels.Context;
using Casbin.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using BackEndTemplate.Models.DataBaseModels;

namespace BackEndTemplate.Controllers.BaseController
{
    [ApiController]
    [Route("api/[controller]")]
    [AllApiVersions]
    public class BaseController<T> : ControllerBase
    {
        protected readonly IStringLocalizer<I18N> localizer;
        protected readonly VideoShareContext ctx;
        protected readonly IEnforcerProvider enforcerProvider;
        protected readonly ILogger<T> logger;

        public BaseController(ILogger<T> _logger, IStringLocalizer<I18N> _localizer, VideoShareContext _ctx, IEnforcerProvider _enforcerProvider)
        {
            logger = _logger;
            localizer = _localizer;
            ctx = _ctx;
            enforcerProvider = _enforcerProvider;
        }
    }
}