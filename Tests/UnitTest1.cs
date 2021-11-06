using AdvancedActions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using PlaywrightSharp;
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
        private IPlaywright Playwright { get; set; }
        public IBrowser Browser { get; private set; }
        public string BaseUrl { get; } = $"https://localhost:{GetRandomUnusedPort()}";

        public WebServerDriver()
        {
            host = Program
                .CreateHostBuilder(null)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls(BaseUrl);
                    // optional to set path to static file assets
                    // webBuilder.UseContentRoot();
                })
                .ConfigureServices(configure =>
                {
                    // override any services
                })
                .Build();
        }

        public async Task InitializeAsync()
        {
            Playwright = await PlaywrightSharp.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync();
            await host.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await host.StopAsync();
            host?.Dispose();
            Playwright?.Dispose();
        }

        public void Dispose()
        {
            host?.Dispose();
            Playwright?.Dispose();
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
            var page = await driver.Browser.NewPageAsync();
            await page.GoToAsync(driver.BaseUrl);

            var title = await page.GetTitleAsync();

            Assert.Equal("Index", title);
        }
    }
}