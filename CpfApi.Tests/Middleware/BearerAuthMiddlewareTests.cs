using CpfApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CpfApi.Tests.Middleware;

public class BearerAuthMiddlewareTests
{
    private static BearerAuthMiddleware BuildMiddleware(
        RequestDelegate next,
        string configuredToken = "secret123")
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiToken"] = configuredToken })
            .Build();

        return new BearerAuthMiddleware(next, config, NullLogger<BearerAuthMiddleware>.Instance);
    }

    [Fact]
    public async Task InvokeAsync_ValidToken_CallsNext()
    {
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var ctx = CreateContext("/consulta/cpf/12345678909", "Bearer secret123");
        await middleware.InvokeAsync(ctx);

        Assert.True(nextCalled);
        Assert.Equal(200, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_MissingAuthHeader_Returns401()
    {
        var middleware = BuildMiddleware(_ => Task.CompletedTask);

        var ctx = CreateContext("/consulta/cpf/12345678909", authHeader: null);
        await middleware.InvokeAsync(ctx);

        Assert.Equal(401, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WrongToken_Returns401()
    {
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var ctx = CreateContext("/consulta/cpf/12345678909", "Bearer wrongtoken");
        await middleware.InvokeAsync(ctx);

        Assert.False(nextCalled);
        Assert.Equal(401, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_BearerPrefixMissing_Returns401()
    {
        var middleware = BuildMiddleware(_ => Task.CompletedTask);

        var ctx = CreateContext("/consulta/cpf/12345678909", "secret123");
        await middleware.InvokeAsync(ctx);

        Assert.Equal(401, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_HealthPath_SkipsAuth()
    {
        var nextCalled = false;
        var middleware = BuildMiddleware(_ => { nextCalled = true; return Task.CompletedTask; });

        var ctx = CreateContext("/health", authHeader: null);
        await middleware.InvokeAsync(ctx);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_EmptyConfiguredToken_Returns401()
    {
        var middleware = BuildMiddleware(_ => Task.CompletedTask, configuredToken: "");

        var ctx = CreateContext("/consulta/cpf/12345678909", "Bearer anytoken");
        await middleware.InvokeAsync(ctx);

        Assert.Equal(401, ctx.Response.StatusCode);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static HttpContext CreateContext(string path, string? authHeader)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Path = path;
        ctx.Response.Body = new MemoryStream();

        if (authHeader is not null)
            ctx.Request.Headers.Authorization = authHeader;

        return ctx;
    }
}
