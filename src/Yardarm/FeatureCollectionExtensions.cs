namespace Yardarm
{
    public static class FeatureCollectionExtensions
    {
        public static TFeature GetOrAdd<TFeature, TImplementation>(this IFeatureCollection features)
            where TFeature : class
            where TImplementation : TFeature, new()
        {
            var feature = features.Get<TFeature>();
            if (feature == null)
            {
                feature = new TImplementation();
                features.Set(feature);
            }

            return feature;
        }
    }
}
