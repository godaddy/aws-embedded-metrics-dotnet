using System;
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
    public class Ec2EnvironmentTests
    {
        private readonly IFixture _fixture;
        private IConfiguration _configuration;
        private IResourceFetcher _resourceFetcher;
        private EC2Environment _environment;

        public Ec2EnvironmentTests()
        {
            _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
            _configuration = _fixture.Create<IConfiguration>();
            _resourceFetcher = _fixture.Create<IResourceFetcher>();
            _environment = new EC2Environment(_configuration, _resourceFetcher);
        }

        [Fact]
        public void Test_Probe_Returns_True()
        {
            var result = _environment.Probe();
            Assert.True(result);
        }

        [Fact]
        public void Test_Probe_Returns_False()
        {
            _resourceFetcher.Fetch<EC2Metadata>(Arg.Any<Uri>()).Throws<EMFClientException>();
            var result = _environment.Probe();
            Assert.False(result);
        }

        [Fact]
        public void Test_GetType_WithNoMetadata()
        {
            _resourceFetcher.Fetch<EC2Metadata>(Arg.Any<Uri>()).Throws<EMFClientException>();
            _environment.Probe();
            Assert.Equal(Constants.UNKNOWN, _environment.Type);
        }
        
        [Fact]
        public void Test_GetType_FromConfiguration()
        {
            var serviceType = _fixture.Create<string>();
            _configuration.ServiceType.Returns(serviceType);
            _resourceFetcher.Fetch<EC2Metadata>(Arg.Any<Uri>()).Returns(new EC2Metadata());

            _environment.Probe();

            Assert.Equal(serviceType, _environment.Type);
        }

        [Fact]
        public void Test_GetType_WithMetadata()
        {
            _resourceFetcher.Fetch<EC2Metadata>(Arg.Any<Uri>()).Returns(new EC2Metadata());
            _environment.Probe();
            Assert.Equal("AWS::EC2::Instance", _environment.Type);
        }

        [Fact]
        public void Test_ConfigureContext()
        {
            var ec2Metadata = _fixture.Create<EC2Metadata>();
            _resourceFetcher.Fetch<EC2Metadata>(Arg.Any<Uri>()).Returns(ec2Metadata);
            _environment.Probe();

            MetricsContext context = new MetricsContext();
            _environment.ConfigureContext(context);

            Assert.Equal(ec2Metadata.ImageId, context.GetProperty("imageId"));
            Assert.Equal(ec2Metadata.InstanceId, context.GetProperty("instanceId"));
            Assert.Equal(ec2Metadata.InstanceType, context.GetProperty("instanceType"));
            Assert.Equal(ec2Metadata.PrivateIp, context.GetProperty("privateIp"));
            Assert.Equal(ec2Metadata.AvailabilityZone, context.GetProperty("availabilityZone"));
        }
    }
}