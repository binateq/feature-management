using Binateq.FeatureManagement.Flipt.Protos;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Binateq.FeatureManagement.Flipt;

/// <summary>
/// A feature filter that can be used to activate feature from Flipt service.
/// </summary>
public class FliptFeatureFilter : IFeatureFilter, IDisposable
{
    private readonly ILogger<FliptFeatureFilter> _logger;

    /// <summary>
    /// Creates Flipt service based feature filter.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="configuration">Configuration.</param>
    /// <exception cref="ArgumentNullException">Throws if <paramref name="logger" />
    /// or <paramref name="configuration"/> are <c>null</c>.</exception>
    public FliptFeatureFilter(ILogger<FliptFeatureFilter> logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        var parameters = configuration.GetSection(FliptParameters.SectionName)
                                      .Get<FliptParameters>();

        if (parameters?.Url == null)
        {
            GrpcChannel = null;
            FliptClient = null;

            _logger.LogWarning($"{nameof(FliptParameters.Url)} doesn't assigned in {FliptParameters.SectionName} configuration section.");
        }
        else
        {
            GrpcChannel = GrpcChannel.ForAddress(parameters.Url);
            FliptClient = new Protos.Flipt.FliptClient(GrpcChannel);

            _logger.LogInformation($"{nameof(FliptFeatureFilter)} has been started.");
        }

        if (string.IsNullOrWhiteSpace(parameters?.ClientToken))
        {
            Metadata = null;
        }
        else
        {
            Metadata = new Metadata
            {
                { "authorization", $"Bearer {parameters.ClientToken}"}
            };
        }
    }

    /// <summary>
    /// Gets gRPC client.
    /// </summary>
    internal GrpcChannel? GrpcChannel { get; private set; }
    
    /// <summary>
    /// Gets Flipt client.
    /// </summary>
    internal Protos.Flipt.FliptClient? FliptClient { get; private set; }
    
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
    public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        if (FliptClient == null)
            return false;

        try
        {
            var request = new GetFlagRequest
            {
                Key = context.FeatureName,
                NamespaceKey = context.GetNamespace()
            };

            var response = await FliptClient.GetFlagAsync(request, Metadata);
            
            return response.Enabled;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurs while reading feature flag {FeatureName}, the false value has returned.", context.FeatureName);
            
            return false;
        }
    }
}
