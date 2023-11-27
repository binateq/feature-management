using System.Security.Claims;
using Binateq.FeatureManagement.Flipt.Protos;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace Binateq.FeatureManagement.Flipt;

/// <summary>
/// A feature filter that can be used to activate user-specified or group-specified feature from Flipt service.
/// </summary>
public class FliptPrincipalFeatureFilter : IContextualFeatureFilter<ClaimsPrincipal>, IDisposable
{
    private readonly ILogger<FliptPrincipalFeatureFilter> _logger;
    private readonly ClaimExtractor _claimExtractor;

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

        Namespace = string.IsNullOrWhiteSpace(parameters?.Namespace) ? "default" : parameters.Namespace;

        _claimExtractor = new ClaimExtractor(parameters?.AnonymousId, parameters?.UserIdClaim, parameters?.GroupIdClaim);
    }

    /// <summary>
    /// Gets gRPC client.
    /// </summary>
    internal GrpcChannel? GrpcChannel { get; private set; }
    
    /// <summary>
    /// Gets EvaluationService client.
    /// </summary>
    internal EvaluationService.EvaluationServiceClient? EvaluationServiceClient { get; private set; }
    
    /// <summary>
    /// Gets metadata for gRPC request.
    /// </summary>
    internal Metadata? Metadata { get; private set; }

    /// <summary>
    /// Gets namespace.
    /// </summary>
    internal string Namespace { get; private set; }

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
    public async Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context, ClaimsPrincipal claimsPrincipal)
    {
        if (EvaluationServiceClient == null)
            return false;

        var userId = _claimExtractor.GetUserId(claimsPrincipal);
        var groupIds = _claimExtractor.GetGroupIds(claimsPrincipal);

        try
        {
            var requestId = Guid.NewGuid().ToString("N");
            var request = MakeRequest(requestId, Namespace, context.FeatureName, userId, groupIds);
            var response = await EvaluationServiceClient.BatchAsync(request, Metadata);
            
            return response.Responses.Aggregate(false, (a, r) => a | r.BooleanResponse.Enabled);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Exception occurs while reading feature flag {FeatureName}, the false value has returned.", context.FeatureName);
            
            return false;
        }
    }

    /// <summary>
    /// Makes batch evaluation request.
    /// </summary>
    /// <param name="requestId">Request Id.</param>
    /// <param name="namespace">Namespace.</param>
    /// <param name="flag">Flag.</param>
    /// <param name="userId">User Id.</param>
    /// <param name="groupIds">Group Id.</param>
    /// <returns>Batch evaluation request.</returns>
    internal static BatchEvaluationRequest MakeRequest(string requestId, string @namespace, string flag, string userId, IEnumerable<string> groupIds)
    {
        var request = new BatchEvaluationRequest
        {
            RequestId = requestId
        };
        
        request.Requests.Add(new EvaluationRequest
        {
            RequestId = requestId,
            NamespaceKey = @namespace,
            FlagKey = flag,
            EntityId = userId,
            Context = { { "UserId", userId }}
        });
        
        request.Requests.AddRange(from groupId in groupIds
                                  select new EvaluationRequest
                                  {
                                      RequestId = requestId,
                                      NamespaceKey = @namespace,
                                      FlagKey = flag,
                                      EntityId = groupId,
                                      Context = { { "GroupId", groupId } }
                                  });

        return request;
    }
}
