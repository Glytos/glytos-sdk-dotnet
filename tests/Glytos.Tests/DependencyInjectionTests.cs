using System;
using Glytos;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Glytos.Tests
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddGlytosResolvesASingletonClient()
        {
            var services = new ServiceCollection();
            services.AddGlytos("gly_test", options => options.Environment = "prod");

            using var provider = services.BuildServiceProvider();

            var client = provider.GetRequiredService<GlytosClient>();
            var again = provider.GetRequiredService<GlytosClient>();

            Assert.NotNull(client);
            Assert.Same(client, again);
        }

        [Fact]
        public void AddGlytosRequiresAnApiKey()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentException>(() => services.AddGlytos(string.Empty));
        }
    }
}
