using System.Reflection;
using System.Security.Claims;

namespace Binateq.FeatureManagement.Flipt;

/// <summary>
/// Extracts user id and group ids from <see cref="ClaimsPrincipal" />.
/// </summary>
internal class ClaimExtractor
{
    private readonly string _anonymousUserId;
    private readonly string _userIdClaim;
    private readonly string _groupIdClaim;

    /// <summary>
    /// Initializes new instance of the type <see cref="ClaimExtractor" />.
    /// </summary>
    /// <param name="anonymousUserId">Anonymous identifier.</param>
    /// <param name="userIdClaim">Name of claim containing user id.</param>
    /// <param name="groupIdClaim">Name of claim containing group id.</param>
    public ClaimExtractor(string? anonymousUserId, string? userIdClaim, string? groupIdClaim)
    {
        _anonymousUserId = anonymousUserId ?? Guid.Empty.ToString("N");
        _userIdClaim = GetClaimByName(userIdClaim) ?? ClaimTypes.NameIdentifier;
        _groupIdClaim = GetClaimByName(groupIdClaim) ?? ClaimTypes.GroupSid;
    }

    /// <summary>
    /// Gets user id from <see cref="ClaimsPrincipal" />.
    /// </summary>
    /// <param name="claimsPrincipal">Claims principal.</param>
    /// <returns>User Id.</returns>
    public string GetUserId(ClaimsPrincipal claimsPrincipal) =>
        claimsPrincipal.FindFirst(_userIdClaim)?.Value ?? _anonymousUserId;

    /// <summary>
    /// Gets group id from <see cref="ClaimsPrincipal" />. 
    /// </summary>
    /// <param name="claimsPrincipal">Claims principal.</param>
    /// <returns>Group Id.</returns>
    public IEnumerable<string> GetGroupIds(ClaimsPrincipal claimsPrincipal) =>
        claimsPrincipal.FindAll(_groupIdClaim)
                       .Select(x => x.Value);

    /// <summary>
    /// Gets constant's value using its name from the type <see cref="ClaimTypes" />.
    /// </summary>
    /// <param name="claimName">Constant's name, f.e. <see cref="ClaimTypes.NameIdentifier" />.</param>
    /// <returns>Constant's value or <c>null</c>, if the constant is not found.</returns>
    internal static string? GetClaimByName(string? claimName)
    {
        if (claimName == null)
        {
            return null;
        }

        const string claimTypesPrefix = nameof(ClaimTypes) + ".";
        if (claimName.StartsWith(claimTypesPrefix))
        {
            var constName = claimName.Substring(claimTypesPrefix.Length);
            var claims = typeof(ClaimTypes).GetFields(BindingFlags.Public | BindingFlags.Static)
                                           .Where(x => x.IsLiteral && !x.IsInitOnly && x.FieldType == typeof(string))
                                           .SingleOrDefault(x => x.Name == constName);

            return claims?.GetValue(null)
                         ?.ToString();
        }

        return claimName;
    }
}
