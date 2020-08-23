using Microsoft.Extensions.DependencyInjection;

namespace Yardarm
{
    public abstract class YardarmExtension
    {
        public abstract IServiceCollection ConfigureServices(IServiceCollection services);
    }
}
