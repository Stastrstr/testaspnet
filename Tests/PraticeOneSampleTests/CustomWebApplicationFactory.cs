using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public CustomWebApplicationFactory()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Optional: Configure test-specific services or settings
        builder.ConfigureServices(services =>
        {
            // Example: Replace a service with a mock for testing
            // services.AddSingleton<IMyService, MockMyService>();
        });
    }
}