using System.Security.Claims;

namespace Binateq.FeatureManagement.Flipt;

/// <summary>
/// Provides configuration parameters for Flipt feature filters.
/// </summary>
public class FliptParameters
{
    /// <summary>
    /// Name of configuration section.
    /// </summary>
    public const string SectionName = "Flipt";
    
    /// <summary>
    /// Gets or sets anonymous identifier.
    /// </summary>
    /// <remarks>Default <see cref="System.Guid.Empty" />.</remarks>
    public string? AnonymousId { get; set; }
    
    /// <summary>
    /// Gets of sets client token.
    /// </summary>
    /// <remarks>Static token made in Flipt UI.</remarks>
    public string? ClientToken { get; set; }
    
    /// <summary>
    /// Gets or sets URL of Flipt gRPS API.
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// Gets of sets name of claim containing group identifier.
    /// </summary>
    /// <remarks>F.e. "id" or "ClaimTypes.NameIdentifier".</remarks>
    public string? UserIdClaim { get; set; }
    
    /// <summary>
    /// Gets or sets name of claim containing user identifier.
    /// </summary>
    /// <remarks>F.e. "group" or "ClaimTypes.GroupSid".</remarks>
    public string? GroupIdClaim { get; set; }
}
