using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Binateq.FeatureManagement.Flipt.Tests;

public class FliptFeatureFilterShould
{
    [Fact]
    public void CreateGrpcChannelAndFliptClient_WhenUrlIsPresent()
    {
        var logger = new NullLogger<FliptFeatureFilter>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new[]
                                                      {
                                                          KeyValuePair.Create("Flipt:Url", "http://localhost:9000")
                                                      })
                                                      .Build();

        var filter = new FliptFeatureFilter(logger, configuration);
        
        Assert.NotNull(filter.GrpcChannel);
        Assert.NotNull(filter.FliptClient);
    }
    
    [Fact]
    public void DontCreateGrpcChannelAndFliptClient_WhenUrlIsMissing()
    {
        var logger = new NullLogger<FliptFeatureFilter>();
        var configuration = new ConfigurationBuilder().Build();

        var filter = new FliptFeatureFilter(logger, configuration);
        
        Assert.Null(filter.GrpcChannel);
        Assert.Null(filter.FliptClient);
    }

    [Fact]
    public void CreateMetadata_WhenClientTokenIsPresent()
    {
        var logger = new NullLogger<FliptFeatureFilter>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new[]
                                                      {
                                                          KeyValuePair.Create("Flipt:ClientToken", "client-token")
                                                      })
                                                      .Build();

        var filter = new FliptFeatureFilter(logger, configuration);
        
        Assert.NotNull(filter.Metadata);
        Assert.Equal("authorization", filter.Metadata[0].Key);
        Assert.Equal("Bearer client-token", filter.Metadata[0].Value);
    }
    
    [Fact]
    public void DontCreateMetadata_WhenClientTokenIsMissing()
    {
        var logger = new NullLogger<FliptFeatureFilter>();
        var configuration = new ConfigurationBuilder().Build();

        var filter = new FliptFeatureFilter(logger, configuration);
        
        Assert.Null(filter.Metadata);
    }

    [Fact]
    public void FillNamespaceWithValue_WhenNamespaceIsSpecified()
    {
        var logger = new NullLogger<FliptFeatureFilter>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new[]
                                                      {
                                                          KeyValuePair.Create("Flipt:Namespace", "foo")
                                                      })
                                                      .Build();

        var filter = new FliptFeatureFilter(logger, configuration);
        
        Assert.Equal("foo", filter.Namespace);
    }

    [Fact]
    public void FillNamespaceWithDefault_WhenNamespaceDoesNotSpecified()
    {
        var logger = new NullLogger<FliptFeatureFilter>();
        var configuration = new ConfigurationBuilder().Build();

        var filter = new FliptFeatureFilter(logger, configuration);
        
        Assert.Equal("default", filter.Namespace);
    }
}
