using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Amazon.CloudWatch.EMF.Config;
using Amazon.CloudWatch.EMF.Environment;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Xunit;

namespace Amazon.CloudWatch.EMF.Tests.Environment
{

    public class ResourceFetcherTests
    {
        private static readonly string endpoint_path = "/fake/endpoint";
        private static readonly TimeSpan _clientTimeout = TimeSpan.FromSeconds(10);
        private static Uri _endpoint;
        private ResourceFetcher _fetcher;
        private static readonly WireMockServer _mockServer = WireMockServer.Start();

        public ResourceFetcherTests()
        {
            _endpoint = new Uri("http://localhost:" + _mockServer.Ports[0] + endpoint_path);
            _fetcher = new ResourceFetcher();
        }
        
        [Fact]
        public void Test_ReadData_With200Response() 
        {
            GenerateStub(200, "{\"name\":\"test\",\"size\":10}");
            
            var testMetadata = _fetcher.Fetch<TestMetadata>(_endpoint);

            Assert.Equal("test", testMetadata.Name );
            Assert.Equal(10, testMetadata.Size);
        }
        
        [Fact]
        public void Test_ReadData_With200Response_ButInvalidJson() {

            GenerateStub(200, "error");
            try 
            {
                var testMetadata = _fetcher.Fetch<TestMetadata>(_endpoint);
            } 
            catch (EMFClientException ex) 
            {
                Assert.Contains("Unable to parse json string", ex.Message);
            }
        }

        [Fact]
        public void Test_Fetch_ThrowsException_WhenNoConnection()
        {
        }
        
        [Fact]
        public void Test_Fetch_ThrowsException_For404Response() 
        {
            GenerateStub(404, "Not Found");
            try
            {
                _fetcher.Fetch<TestMetadata>(_endpoint);
            }
            catch (EMFClientException ex)
            {
                Assert.Contains("Not Found", ex.InnerException.Message);
            }
        }
        
        [Fact]
        public void Test_Fetch_ThrowsExceptionFor500Response() 
        {
            GenerateStub(500, "Internal Server Error");
            try 
            {
                _fetcher.Fetch<TestMetadata>(_endpoint);
            } 
            catch (EMFClientException ex)
            {
                Assert.Contains("Internal Server Error", ex.InnerException.Message);
            }
        }
        private void GenerateStub(int statusCode, string message) 
        {
            _mockServer.Given(Request.Create()
                    .WithPath(endpoint_path)
                    .UsingGet())
                .RespondWith(Response.Create()
                        .WithStatusCode(statusCode)
                        .WithHeader(HeaderNames.ContentType, "application/json")
                        .WithBody(message));
        }    

    }
    
    public class TestMetadata 
    {
        [JsonProperty("name")]
        internal string Name { get; set; }
        
        [JsonProperty("size")]
        internal int Size { get; set; }  

    }
}