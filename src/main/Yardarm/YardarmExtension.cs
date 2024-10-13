using Microsoft.Extensions.DependencyInjection;

namespace Yardarm
{
    public abstract class YardarmExtension
    {
        /// <summary>
        /// Extension name.
        /// </summary>
        public virtual string Name => GetType().Name;

        public abstract IServiceCollection ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Returns true if the work done by this extension is trimmable.
        /// </summary>
        /// <remarks>
        /// If all extensions return true the output SDK will be marked as trimmable.
        /// The default is false, extensions which produce trimmable output should
        /// override this property to return true.
        /// </remarks>
        public virtual bool IsOutputTrimmable(GenerationContext context) => false;
    }
}
