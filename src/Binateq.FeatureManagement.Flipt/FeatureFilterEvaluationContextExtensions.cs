using Microsoft.FeatureManagement;

namespace Binateq.FeatureManagement.Flipt;

internal static class FeatureFilterEvaluationContextExtensions
{
    public static string GetNamespace(this FeatureFilterEvaluationContext context) =>
        context.Parameters?["Namespace"] ?? "default";
}
