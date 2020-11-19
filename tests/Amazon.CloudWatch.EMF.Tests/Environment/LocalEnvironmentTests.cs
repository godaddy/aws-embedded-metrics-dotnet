using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Sink;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class LocalEnvironmentTests
    {
        private readonly IFixture _fixture;
        private readonly IConfiguration _configuration;
        private readonly LocalEnvironment _localEnvironment;
        public LocalEnvironmentTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _configuration = _fixture.Create<IConfiguration>();
            _localEnvironment = new LocalEnvironment(_configuration);
        }

        [Fact]
        public void Test_Probe_Returns_False()
        {
            Assert.False(_localEnvironment.Probe());
        }

        [Fact]
        public void Test_GetName_When_Configuration_NotSet()
        {
            Assert.Equal("Unknown", _localEnvironment.Name);
        }
        
        
        [Fact]
        public void Test_GetName_FromConfiguration()
        {
            var serviceName = _fixture.Create<string>();
            _configuration.ServiceName.Returns(serviceName);
            Assert.Equal(serviceName, _localEnvironment.Name);
        }

        [Fact]
        public void Test_GetType_When_Configuration_NotSet()
        {
            Assert.Equal("Unknown", _localEnvironment.Type);
        }
        
        [Fact]
        public void Test_GetType_FromConfiguration()
        {
            var serviceType = _fixture.Create<string>();
            _configuration.ServiceType.Returns(serviceType);

            Assert.Equal(serviceType, _localEnvironment.Type);
        }
        
        [Fact]
        public void Test_GetLogGroupName_When_Configuration_NotSet()
        {
            Assert.Equal("Unknown_metrics", _localEnvironment.LogGroupName);
        }
        
        
        [Fact]
        public void Test_GetLogGroupName_FromConfiguration()
        {
            var logGroupName = _fixture.Create<string>();
            _configuration.LogGroupName.Returns(logGroupName);

            Assert.Equal(logGroupName, _localEnvironment.LogGroupName);
        }

        [Fact]
        public void Test_GetSink_Returns_ConsoleSink()
        {
            var sink = _localEnvironment.Sink;
            Assert.True(sink is ConsoleSink);
        }
    }
}