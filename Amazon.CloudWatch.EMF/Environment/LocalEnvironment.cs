namespace Amazon.CloudWatch.EMF.Environment
{
    using Amazon.CloudWatch.EMF.Config;
    using Amazon.CloudWatch.EMF.Model;
    using Amazon.CloudWatch.EMF.Sink;

    public class LocalEnvironment : IEnvironment
    {
        private ISink _sink = null;
        private Configuration _config;

        LocalEnvironment(Configuration config)
        {
            _config = config;
        }
        public bool Probe()
        {
            return false;
        }

        public string Name => !string.IsNullOrEmpty(_config.ServiceName) ? _config.ServiceName : Constants.UNKNOWN;

        public string Type => !string.IsNullOrEmpty(_config.ServiceType) ? _config.ServiceType : Constants.UNKNOWN;
        
        public string LogGroupName => !string.IsNullOrEmpty(_config.LogGroupName) ? _config.LogGroupName : Name + "_metrics";

        public void ConfigureContext(MetricsContext context) 
        {
        }

        public ISink Sink
        {
            get
            {
                return _sink ??= new ConsoleSink();
            }
        }
    }
}
