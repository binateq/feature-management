using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;

namespace Binateq.FeatureManagement.Flipt.Tests;

public class FeatureFilterEvaluationContextExtensionsShould
{
    [Fact]
    public void ReturnNamespace_IfItIsSpecified()
    {
        var context = new FeatureFilterEvaluationContext
        {
            Parameters = new ConfigurationBuilder().AddInMemoryCollection(new[]
                                                   {
                                                       KeyValuePair.Create("Namespace", (string?)"foo")
                                                   })
                                                   .Build()
        };

        var actual = context.GetNamespace();
        
        Assert.Equal("foo", actual);
    }

    [Fact]
    public void ReturnDefault_IfNamespaceIsNotSpecified()
    {
        var context = new FeatureFilterEvaluationContext
        {
            Parameters = new ConfigurationBuilder().Build()
        };

        var actual = context.GetNamespace();
        
        Assert.Equal("default", actual);
    }
}
