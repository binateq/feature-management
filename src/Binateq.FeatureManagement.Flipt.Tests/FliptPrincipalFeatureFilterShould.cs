using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Binateq.FeatureManagement.Flipt.Tests;

public class FliptPrincipalFeatureFilterShould
{
    [Fact]
    public void CreateGrpcChannelAndEvaluationServiceClient_WhenUrlIsPresent()
    {
        var logger = new NullLogger<FliptPrincipalFeatureFilter>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new[]
                                                      {
                                                          KeyValuePair.Create("Flipt:Url", "http://localhost:9000")
                                                      })
                                                      .Build();

        var filter = new FliptPrincipalFeatureFilter(logger, configuration);
        
        Assert.NotNull(filter.GrpcChannel);
        Assert.NotNull(filter.EvaluationServiceClient);
    }
    
    [Fact]
    public void DontCreateGrpcChannelAndEvaluationServiceClient_WhenUrlIsMissing()
    {
        var logger = new NullLogger<FliptPrincipalFeatureFilter>();
        var configuration = new ConfigurationBuilder().Build();

        var filter = new FliptPrincipalFeatureFilter(logger, configuration);
        
        Assert.Null(filter.GrpcChannel);
        Assert.Null(filter.EvaluationServiceClient);
    }

    [Fact]
    public void CreateMetadata_WhenClientTokenIsPresent()
    {
        var logger = new NullLogger<FliptPrincipalFeatureFilter>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new[]
                                                      {
                                                          KeyValuePair.Create("Flipt:ClientToken", "client-token")
                                                      })
                                                      .Build();

        var filter = new FliptPrincipalFeatureFilter(logger, configuration);
        
        Assert.NotNull(filter.Metadata);
        Assert.Equal("authorization", filter.Metadata[0].Key);
        Assert.Equal("Bearer client-token", filter.Metadata[0].Value);
    }
    
    [Fact]
    public void DontCreateMetadata_WhenClientTokenIsMissing()
    {
        var logger = new NullLogger<FliptPrincipalFeatureFilter>();
        var configuration = new ConfigurationBuilder().Build();

        var filter = new FliptPrincipalFeatureFilter(logger, configuration);
        
        Assert.Null(filter.Metadata);
    }

    [Fact]
    public void MakeValidRequest()
    {
        var request = FliptPrincipalFeatureFilter.MakeRequest(requestId: "foo",
                                                              @namespace: "bar",
                                                              flag: "baz",
                                                              userId: "u123",
                                                              new[]
                                                              {
                                                                  "g456",
                                                                  "g789"
                                                              });
        
        Assert.Equal("foo", request.RequestId);
        
        Assert.Equal("foo", request.Requests[0].RequestId);
        Assert.Equal("bar", request.Requests[0].NamespaceKey);
        Assert.Equal("baz", request.Requests[0].FlagKey);
        Assert.Equal("u123", request.Requests[0].EntityId);
        Assert.Equal("u123", request.Requests[0].Context["UserId"]);
        
        Assert.Equal("foo", request.Requests[1].RequestId);
        Assert.Equal("bar", request.Requests[1].NamespaceKey);
        Assert.Equal("baz", request.Requests[1].FlagKey);
        Assert.Equal("g456", request.Requests[1].EntityId);
        Assert.Equal("g456", request.Requests[1].Context["GroupId"]);
    }
}
