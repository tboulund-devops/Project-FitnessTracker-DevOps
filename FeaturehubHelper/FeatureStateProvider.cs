using FeatureHubSDK;

namespace FeaturehubHelper;

public class FeatureStateProvider
{
    private readonly EdgeFeatureHubConfig _config;
    
    public FeatureStateProvider()
    {
        var config = new EdgeFeatureHubConfig("http://featurehub:8085", "ba378ba8-5ed6-42e0-80fb-7a34805ea402/DszMrjiR3LKWf31JlakdyrL2UQkE7F*hbSqu4L4Wb6nUVtyLhru" );
        config.Init().Wait();
        _config = config;
    }

    public bool IsEnabled(string featureKey)
    {
        //if we have a context like percent or country rollout, we can set it here before checking the feature state
        //need to do this in featurehub to - and we need a user database because we need to know something about the user
        //e.g. var context = _config.NewContext()
        //.country(StrategyAttributeValue.Denmark)
        //.build().GetAwaiter.GetResult();
        //return (bool)context[featureKey].Value;
        
        return (bool)_config.Repository[featureKey].Value;
        
    }
}