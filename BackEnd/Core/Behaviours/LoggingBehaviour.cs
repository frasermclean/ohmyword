using MediatR;
using Microsoft.Extensions.Logging;

namespace OhMyWord.Core.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        this.logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        using (logger.BeginScope(request))
        {
            logger.LogInformation("Handling request: {RequestType}", typeof(TRequest).Name);
            var response = await next();
            logger.LogInformation("Received response: {ResponseType}", typeof(TResponse).Name);
            return response;
        }
    }
}
