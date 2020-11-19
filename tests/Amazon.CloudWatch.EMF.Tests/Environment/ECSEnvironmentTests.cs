using System;
using System.Collections.Generic;
using System.Net;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Amazon.CloudWatch.EMF.Model;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{
    public class EcsEnvironmentTests : IDisposable
    {
        private readonly IFixture _fixture;
        private IConfiguration _configuration;
        private IResourceFetcher _resourceFetcher;
        private ECSEnvironment _environment;
        public EcsEnvironmentTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _configuration = _fixture.Create<IConfiguration>();
            _resourceFetcher = _fixture.Create<IResourceFetcher>();
            _environment = new ECSEnvironment(_configuration, _resourceFetcher);
            
            System.Environment.SetEnvironmentVariable(ECSEnvironment.ECS_CONTAINER_METADATA_URI, "http://ecs-metata.com");
        }

        public void Dispose()
        {
            System.Environment.SetEnvironmentVariable(ECSEnvironment.ECS_CONTAINER_METADATA_URI, null);
            System.Environment.SetEnvironmentVariable(ECSEnvironment.FLUENT_HOST, null);
        }

        [Fact]
        public void Test_Probe_Returns_False_If_NoUri()
        {
            System.Environment.SetEnvironmentVariable(ECSEnvironment.ECS_CONTAINER_METADATA_URI, null);
            var result = _environment.Probe();
            Assert.False(result);
        }
        
        [Fact]
        public void Test_Probe_Returns_False_If_ResourceFetcher_Throws_Exception()
        {
            _resourceFetcher.Fetch<ECSMetadata>(Arg.Any<Uri>()).Throws<EMFClientException>();
            var result = _environment.Probe();
            Assert.False(result);
        }
        
        
        [Fact]
        public void Test_Probe_Returns_True()
        {
            _resourceFetcher.Fetch<ECSMetadata>(Arg.Any<Uri>()).ReturnsForAnyArgs(new ECSMetadata());
            var result = _environment.Probe();
            Assert.True(result);
        }

        [Fact]
        public void Test_GetName_FromConfiguration()
        {
            var serviceName = _fixture.Create<string>();
            _configuration.ServiceName.Returns(serviceName);
            Assert.Equal(serviceName, _environment.Name);
        }
        
        
        [Fact]
        public void Test_GetName_FromMetaData()
        {
            _configuration.ServiceName.Returns(string.Empty);
            
            var ecsMetaData = GetECSMetadata();
            ecsMetaData.Image = "testAccount.dkr.ecr.us-west-2.amazonaws.com/testImage:latest";
            
            _resourceFetcher.Fetch<ECSMetadata>(Arg.Any<Uri>()).ReturnsForAnyArgs(ecsMetaData);
            
            Assert.True(_environment.Probe());
            Assert.Equal("testImage:latest", _environment.Name);
        }
        
        [Fact]
        public void Test_GetName_Returns_Unknown()
        {
            Assert.Equal(Constants.UNKNOWN, _environment.Name);
        }
        
       
        [Fact]
        public void Test_GetType()
        {
            Assert.Equal("AWS::ECS::Container", _environment.Type);
        }
        
        [Fact]
        public void Test_GetType_FromConfiguration()
        {
            var serviceType = _fixture.Create<string>();
            _configuration.ServiceType.Returns(serviceType);
            Assert.Equal(serviceType, _environment.Type);
        }
        
        [Fact]
        public void Test_GetLogGroupName_Returns_Empty()
        {
            System.Environment.SetEnvironmentVariable(ECSEnvironment.FLUENT_HOST, "localhost");
            _environment.Probe();

            Assert.Equal("", _environment.LogGroupName);
        }
        
        [Fact]
        public void Test_GetLogGroupName_Returns_NonEmpty()
        {
            _environment.Probe();
            Assert.Equal(Constants.UNKNOWN + "_metrics", _environment.LogGroupName);
        }
        
        [Fact]
        public void Test_ConfigureContext()
        {
            var ecsMetadata = GetECSMetadata();
            _resourceFetcher.Fetch<ECSMetadata>(Arg.Any<Uri>()).Returns(ecsMetadata);
            _environment.Probe();

            MetricsContext context = new MetricsContext();
            _environment.ConfigureContext(context);
            
            Assert.Equal(Dns.GetHostName(), context.GetProperty("containerId"));
            Assert.Equal(ecsMetadata.Image, context.GetProperty("image"));
            Assert.Equal(ecsMetadata.CreatedAt, context.GetProperty("createdAt"));
            Assert.Equal(ecsMetadata.StartedAt, context.GetProperty("startedAt"));
            Assert.Equal(ecsMetadata.Labels["com.amazonaws.ecs.cluster"], context.GetProperty("cluster"));
            Assert.Equal(ecsMetadata.Labels["com.amazonaws.ecs.task-arn"], context.GetProperty("taskArn"));
        }
        
        private ECSMetadata GetECSMetadata()
        {
            var metadata = new ECSMetadata
            {
                CreatedAt = _fixture.Create<DateTime>().ToString(),
                StartedAt = _fixture.Create<DateTime>().ToString(),
                Image = _fixture.Create<string>(),
                Labels = new Dictionary<string, string>
                {
                    {"com.amazonaws.ecs.cluster", _fixture.Create<string>()},
                    {"com.amazonaws.ecs.task-arn", _fixture.Create<string>()}
                }
            };
            return metadata;
        }
    }
}