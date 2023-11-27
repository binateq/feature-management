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
}
