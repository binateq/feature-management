# Binateq.FeatureManagement.Flipt

The NuGet package **Binateq.FeatureManagement.Flipt** allows to use the feature-flagging solution
[**Flipt**](https://www.flipt.io/) together with the
[**Microsoft.FeatureManagement**](https://www.nuget.org/packages/Microsoft.FeatureManagement/) package.

## What for?

The **Microsoft.FeatureManagement** provides simple and portable standard for .NET applications. It can be
extended with __feature filters__, so you can use both static and dynamic feature flags.

You can add the packet
[**Microsoft.Azure.AppConfiguration.AspNetCore**](https://www.nuget.org/packages/Microsoft.Azure.AppConfiguration.AspNetCore)
to use dynamic feature flags configured in __Microsoft Azure App Configuration service__.

But what can you do, if you use __Docker/Kubernetes__ instead of __Azure__? You can use any feature toggle solution like
**Flipt** that has its own unique methods to work with feature flags.

Or you can use this package that implements feature filters' interface for **Flipt** gRPC API. In the latter case you
can move your application from __Azure__ to __Docker__ and vice versa.

## Quick Start

### Classic Microsoft.FeatureManagement

### Append dynamic flag from Flipt

### Append user-specific flag form Flipt

