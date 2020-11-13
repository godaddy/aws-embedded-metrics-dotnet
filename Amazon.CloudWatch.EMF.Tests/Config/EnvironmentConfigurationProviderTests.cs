using System;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Config
{
    public class EnvironmentConfigurationProviderTests :IDisposable
    {
        private readonly IFixture _fixture;
        
        public EnvironmentConfigurationProviderTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
        }

        public void Dispose()
        {
            EnvironmentConfigurationProvider.Config = null;
            DeleteEnv(ConfigurationKeys.SERVICE_NAME);
            DeleteEnv(ConfigurationKeys.SERVICE_TYPE);
            DeleteEnv(ConfigurationKeys.LOG_GROUP_NAME);
            DeleteEnv(ConfigurationKeys.LOG_STREAM_NAME);
            DeleteEnv(ConfigurationKeys.AGENT_ENDPOINT);
            DeleteEnv(ConfigurationKeys.ENVIRONMENT_OVERRIDE);
        }
        
        [Fact]
        public void Test_GetConfig_Returns_NewConfig() 
        {
            PutEnv(ConfigurationKeys.SERVICE_NAME, "TestServiceName");
            PutEnv(ConfigurationKeys.SERVICE_TYPE, "TestServiceType");
            PutEnv(ConfigurationKeys.LOG_GROUP_NAME, "TestLogGroup");
            PutEnv(ConfigurationKeys.LOG_STREAM_NAME, "TestLogStream");
            PutEnv(ConfigurationKeys.AGENT_ENDPOINT, "Endpoint");
            PutEnv(ConfigurationKeys.ENVIRONMENT_OVERRIDE, "Agent");
            
            IConfiguration config = EnvironmentConfigurationProvider.Config;

            Assert.Equal("TestServiceName", config.ServiceName );
            Assert.Equal("TestServiceType", config.ServiceType );
            Assert.Equal("TestLogGroup", config.LogGroupName );
            Assert.Equal("TestLogStream", config.LogStreamName );
            Assert.Equal("Endpoint", config.AgentEndPoint );
            Assert.Equal(Environments.Agent, config.EnvironmentOverride );
        }
        
        [Fact]
        public void Test_GetConfig_Returns_ExistingConfig()
        {
            var serviceName = _fixture.Create<string>();
            var serviceType = _fixture.Create<string>();
            var logGroupName = _fixture.Create<string>();
            var logStreamName = _fixture.Create<string>();
            var agentEndPoint = _fixture.Create<string>();
            
            EnvironmentConfigurationProvider.Config = new Configuration(serviceName, serviceType, logGroupName,
                logStreamName, agentEndPoint, Environments.Agent);
            
            Assert.Equal(serviceName, EnvironmentConfigurationProvider.Config.ServiceName );
            Assert.Equal(serviceType, EnvironmentConfigurationProvider.Config.ServiceType );
            Assert.Equal(logGroupName, EnvironmentConfigurationProvider.Config.LogGroupName );
            Assert.Equal(logStreamName, EnvironmentConfigurationProvider.Config.LogStreamName );
            Assert.Equal(agentEndPoint, EnvironmentConfigurationProvider.Config.AgentEndPoint );
            Assert.Equal(Environments.Agent, EnvironmentConfigurationProvider.Config.EnvironmentOverride );
        }
        
        [Fact]
        public void Test_GetEnvironmentOverride_With_No_EnvVar_Returns_Unknown() 
        {
            IConfiguration config = EnvironmentConfigurationProvider.Config;
            Assert.Equal(Environments.Unknown, config.EnvironmentOverride );
        }
        
        [Fact]
        public void Test_GetEnvironmentOverride_With_Invalid_EnvVar_Returns_Unknown() 
        {
            PutEnv(ConfigurationKeys.ENVIRONMENT_OVERRIDE, "InvalidValue");
            IConfiguration config = EnvironmentConfigurationProvider.Config;
            Assert.Equal(Environments.Unknown, config.EnvironmentOverride );
        }

        private void PutEnv(string key, string value)
        {
            System.Environment.SetEnvironmentVariable(ConfigurationKeys.ENV_VAR_PREFIX + "_" + key, value);
        }
        
        private void DeleteEnv(string key)
        {
            PutEnv(key, null);
        }
    }
}