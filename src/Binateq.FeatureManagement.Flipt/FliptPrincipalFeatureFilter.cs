using System.Security.Claims;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Binateq.FeatureManagement.Flipt;

public class FliptPrincipalFeatureFilter : IContextualFeatureFilter<ClaimsPrincipal>, IDisposable
{
    private readonly ILogger<FliptPrincipalFeatureFilter> _logger;

    /// <summary>
    /// Creates Flipt service based principal feature filter.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="configuration">Configuration.</param>
    /// <exception cref="ArgumentNullException">Throws if <paramref name="logger" />
    /// or <paramref name="configuration"/> are <c>null</c>.</exception>
    public FliptPrincipalFeatureFilter(ILogger<FliptPrincipalFeatureFilter> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        var parameters = configuration.GetSection(FliptParameters.SectionName)
                                      .Get<FliptParameters>();

        if (parameters?.Url == null)
        {
            GrpcChannel = null;
            EvaluationServiceClient = null;

            _logger.LogWarning($"{nameof(FliptParameters.Url)} doesn't assigned in {FliptParameters.SectionName} configuration section.");
        }
        else
        {
            GrpcChannel = GrpcChannel.ForAddress(parameters.Url);
            EvaluationServiceClient = new Protos.EvaluationService.EvaluationServiceClient(GrpcChannel);

            _logger.LogInformation($"{nameof(FliptPrincipalFeatureFilter)} has been started.");
        }

        if (string.IsNullOrWhiteSpace(parameters?.ClientToken))
        {
            Metadata = null;
            
            _logger.LogInformation($"{nameof(FliptPrincipalFeatureFilter)} started without authorization.");
        }
        else
        {
            Metadata = new Metadata
            {
                { "authorization", $"Bearer {parameters.ClientToken}"}
            };

            _logger.LogInformation($"{nameof(FliptPrincipalFeatureFilter)} started with authorization.");
        }
    }

    /// <summary>
    /// Gets gRPC client.
    /// </summary>
    internal GrpcChannel? GrpcChannel { get; private set; }
    
    /// <summary>
    /// Gets EvaluationService client.
    /// </summary>
    internal Protos.EvaluationService.EvaluationServiceClient? EvaluationServiceClient { get; private set; }
    
    /// <summary>
    /// Gets metadata for gRPC request.
    /// </summary>
    internal Metadata? Metadata { get; private set; }

    /// <summary>
    /// Disposes managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">Disposing flag.</param>
    protected virtual void Dispose(bool disposing)
    {
        GrpcChannel?.Dispose();
        
        _logger.LogInformation($"{nameof(FliptFeatureFilter)} has been disposed.");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext featureFilterContext, ClaimsPrincipal appContext) => throw new NotImplementedException();
}
