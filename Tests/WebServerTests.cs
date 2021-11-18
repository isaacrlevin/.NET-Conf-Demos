using AdvancedActions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class WebServerDriver : IAsyncLifetime, IDisposable
    {
        private readonly IHost host;
        private IPlaywright playwright { get; set; }
        public IBrowser browser { get; private set; }
        public string baseUrl { get; } = $"https://localhost:{GetRandomUnusedPort()}";

        public WebServerDriver()
        {
            host = AdvancedActions.Program
                .CreateHostBuilder(null)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(baseUrl);
                })
                .ConfigureServices(configure =>
                { })
                .Build();
        }

        public async Task InitializeAsync()
        {
            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.LaunchAsync();
                // Browser = await Playwright.Chromium.LaunchAsync(new LaunchOptions
                // {
                //     Headless = false
                // });
            await host.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await host.StopAsync();
            host?.Dispose();
            playwright?.Dispose();
        }

        public void Dispose()
        {
            host?.Dispose();
            playwright?.Dispose();
        }

        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Any, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }

    public class WebServerTests : IClassFixture<WebServerDriver>
    {
        private readonly WebServerDriver driver;

        public WebServerTests(WebServerDriver driver)
        {
            this.driver = driver;
        }

        [Fact]
        public async Task PageTitleIsIndex()
        {
            await using var context = await driver.browser.NewContextAsync(new() { IgnoreHTTPSErrors = true });
            var page = await context.NewPageAsync();
            await page.GotoAsync(driver.baseUrl);

            var title = await page.TitleAsync();

            Assert.Equal(".NET Community Heros", title);
        }
    }
}