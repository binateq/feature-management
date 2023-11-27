using System.Security.Claims;

namespace Binateq.FeatureManagement.Flipt.Tests;

public class ClaimExtractorTests
{
    [Fact]
    public void GetClaimByName_WithConstClaim_ReturnsClaimValue()
    {
        var actual = ClaimExtractor.GetClaimByName("ClaimTypes.NameIdentifier");
        Assert.Equal(ClaimTypes.NameIdentifier, actual);
    }

    [Fact]
    public void GetClaimByName_WithSimpleClaim_ReturnsNull()
    {
        var actual = ClaimExtractor.GetClaimByName("iss");
        Assert.Equal("iss", actual);
    }

    [Fact]
    public void GetClaimByName_WithNull_ReturnsNull()
    {
        var actual = ClaimExtractor.GetClaimByName(null);
        Assert.Null(actual);
    }

    [Fact]
    public void GetUserId_WithAnonymous_ReturnsEmptyGuid()
    {
        var claimExtractor = new ClaimExtractor(null, null, null);
        var claimPrincipal = new ClaimsPrincipal();
        var expected = Guid.Empty.ToString("N");

        var actual = claimExtractor.GetUserId(claimPrincipal);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetUserId_WithAnonymousAndSpecifiedAnonymousId_ReturnsAnonymousId()
    {
        var claimExtractor = new ClaimExtractor("foo", null, null);
        var claimPrincipal = new ClaimsPrincipal();

        var actual = claimExtractor.GetUserId(claimPrincipal);

        Assert.Equal("foo", actual);
    }

    [Fact]
    public void GetUserId_WithAuthenticatedUser_ReturnsNameIdentifierValue()
    {
        var claimExtractor = new ClaimExtractor(null, null, null);
        var claimPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "bar")
        }));

        var actual = claimExtractor.GetUserId(claimPrincipal);
        
        Assert.Equal("bar", actual);
    }

    [Fact]
    public void GetUserId_WithAuthenticatedUserAndIss_ReturnsPrimarySidValue()
    {
        var claimExtractor = new ClaimExtractor(null, "iss", null);
        var claimPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("iss", "bar")
        }));

        var actual = claimExtractor.GetUserId(claimPrincipal);
        
        Assert.Equal("bar", actual);
    }

    [Fact]
    public void GetGroupIds_WithAuthenticatedUser_ReturnsGroupSidValues()
    {
        var claimExtractor = new ClaimExtractor(null, null, null);
        var claimPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.GroupSid, "bar"),
            new Claim(ClaimTypes.GroupSid, "baz"),
            new Claim(ClaimTypes.GroupSid, "qux"),
        }));

        var actual = claimExtractor.GetGroupIds(claimPrincipal);
        
        Assert.Equal(new[] { "bar", "baz", "qux" }, actual);
    }

    [Fact]
    public void GetGroupIds_WithAuthenticatedUserAndPrimaryGroupSid_ReturnsPrimaryGroupSidValues()
    {
        var claimExtractor = new ClaimExtractor(null, null, "ClaimTypes.PrimaryGroupSid");
        var claimPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.PrimaryGroupSid, "bar"),
            new Claim(ClaimTypes.PrimaryGroupSid, "baz"),
            new Claim(ClaimTypes.GroupSid, "qux"),
        }));

        var actual = claimExtractor.GetGroupIds(claimPrincipal);
        
        Assert.Equal(new[] { "bar", "baz" }, actual);
    }
}
