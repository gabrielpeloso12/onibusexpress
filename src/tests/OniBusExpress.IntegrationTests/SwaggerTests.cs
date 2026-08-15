using System.Net;
using FluentAssertions;

namespace OniBusExpress.IntegrationTests;

/// <summary>Confirma que a própria geração da documentação Swagger não lança exceção.</summary>
public class SwaggerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SwaggerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SwaggerJson_IsGeneratedSuccessfully()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"title\": \"OniBus Express API\"");
    }

    [Fact]
    public async Task SwaggerUI_IsServed()
    {
        var response = await _client.GetAsync("/swagger/index.html");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
