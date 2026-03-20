using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using System;

namespace Lothal.BuildingBlocks.Resilience;

public static class ResilienceExtensions
{
    public static IServiceCollection AddResiliencePolicies(this IServiceCollection services)
    {
        // Optimal Circuit Breaker Options:
        // 0.5 failure ratio (50% failures)
        // 30s sampling duration
        // 5 minimum throughput
        // 30s break duration
        
        var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(30),
                minimumThroughput: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (ex, breakDelay) => 
                {
                    Console.WriteLine($"Circuit broken for {breakDelay.TotalSeconds}s due to: {ex.Message}");
                },
                onReset: () => Console.WriteLine("Circuit reset."),
                onHalfOpen: () => Console.WriteLine("Circuit is half-open, testing...")
            );

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {ex.Message}");
                });

        services.AddSingleton<IAsyncPolicy>(Policy.WrapAsync(retryPolicy, circuitBreakerPolicy));

        return services;
    }
}
