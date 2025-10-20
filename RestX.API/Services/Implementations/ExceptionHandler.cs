using AutoMapper;
using RestX.API.Controllers;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger logger;
        private readonly IMapper mapper;

        public ExceptionHandler(ILoggerFactory loggerFactory, IMapper mapper)
        {
            logger = loggerFactory.CreateLogger<BaseController>();
            this.mapper = mapper;
        }

        public void RaiseException(Exception ex, string customMessage = "")
        {
            logger.LogError(ex, customMessage);
        }
    }
}
