using AutoFixture;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UPSTask.Model;
using UPSTask.Services.Services;

namespace UPSTask.Services.Test.Services
{
    [TestFixture]
    public class EmployeeServiceTests
    {
        private MockRepository _mockRepository;
        private Fixture _fixture;
        private Mock<HttpClient> _mockHttpClient;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private const string accessToken = "test";
        private const string baseUrl = "https://test.co.in/public/v2/";

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _mockRepository = new MockRepository(MockBehavior.Strict);

            _mockHttpClient = new Mock<HttpClient>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        }

        private EmployeeService CreateService(HttpResponseMessage response, bool isError)
        {
            if(!isError)
            {
                _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
            }
            else
            {
                _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new ArgumentOutOfRangeException("Test Exception Message"));
            }
                
            

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            httpClient.BaseAddress = new Uri(baseUrl);

            return new EmployeeService(httpClient);
        }

        [Test]
        public async Task GetEmployeeGridModelAsync_WhenStatusCodeIs200AndValueExists_ThenSuccessTrueAndMapEmployee()
        {
            // Arrange
            int? pageNumber = null;
            var stringvalue = "[{\"id\":5803080,\"name\":\"VidhyaMishra\",\"email\":\"vidhya_mishra@rempel-fritsch.example\",\"gender\":\"female\",\"status\":\"active\"},{\"id\":5803079,\"name\":\"JyotiVarman\",\"email\":\"jyoti_varman@corkery-stoltenberg.test\",\"gender\":\"female\",\"status\":\"active\"},{\"id\":5803078,\"name\":\"SanjayDhawan\",\"email\":\"dhawan_sanjay@cassin-bosco.example\",\"gender\":\"male\",\"status\":\"inactive\"},{\"id\":5803077,\"name\":\"KannanPatilJD\",\"email\":\"jd_kannan_patil@terry-hermann.example\",\"gender\":\"female\",\"status\":\"inactive\"},{\"id\":5803076,\"name\":\"AvantikaAgarwal\",\"email\":\"avantika_agarwal@stokes-breitenberg.example\",\"gender\":\"male\",\"status\":\"active\"},{\"id\":5803075,\"name\":\"RaviBhattacharya\",\"email\":\"bhattacharya_ravi@hermiston.example\",\"gender\":\"female\",\"status\":\"active\"},{\"id\":5803074,\"name\":\"PriyalaAgarwal\",\"email\":\"agarwal_priyala@langworth.test\",\"gender\":\"female\",\"status\":\"inactive\"},{\"id\":5803073,\"name\":\"MalatiBhattathiri\",\"email\":\"bhattathiri_malati@oberbrunner.test\",\"gender\":\"female\",\"status\":\"inactive\"},{\"id\":5803072,\"name\":\"DhanuKocchar\",\"email\":\"kocchar_dhanu@brekke-will.test\",\"gender\":\"female\",\"status\":\"active\"},{\"id\":5803071,\"name\":\"LakshmiNehru\",\"email\":\"lakshmi_nehru@cremin.test\",\"gender\":\"male\",\"status\":\"active\"}]";
            var content = new StringContent(stringvalue);
            
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = content
            };

            var service = CreateService(response, false);

            // Act
            var result = await service.GetEmployeeGridModelAsync(
                pageNumber);

            // Assert
            Assert.That(result.GetResult().Count(), Is.EqualTo(10));
            Assert.IsTrue(result.IsSuccess);
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEmployeeGridModelAsync_WhenStatusCodeIsNot200_ThenSuccessIsFalseAndResultIsNull()
        {
            // Arrange
            int? pageNumber = null;

            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };

            var service = CreateService(response, false);

            // Act
            var result = await service.GetEmployeeGridModelAsync(
                pageNumber);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsError);
            Assert.That(result.Error.error, Is.EqualTo("No Result Found"));
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEmployeeGridModelAsync_WhenHttpClientThrowsError_ThenSuccessIsFalseAndErrorIsExceptionMessage()
        {
            // Arrange
            int? pageNumber = null;

            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };

            var service = CreateService(response, true);

            // Act
            var result = await service.GetEmployeeGridModelAsync(
                pageNumber);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsError);
            Assert.That(result.Error.error.IndexOf("Test Exception Message"), Is.GreaterThan(0));
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEmployeeGridModelAsyncByEmployeeId_FilterByEmployeeId_Success()
        {
            // Arrange
            var stringvalue = "[{\"id\":5803080,\"name\":\"VidhyaMishra\",\"email\":\"vidhya_mishra@rempel-fritsch.example\",\"gender\":\"female\",\"status\":\"active\"}]"; 
            var content = new StringContent(stringvalue);

            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                Content = content
            };
            var value = "5803080";
            var field = "Id";

            var service = CreateService(response, false);

            // Act
            var result = await service.GetEmployeeGridModelAsyncByEmployeeId(
                value,
                field);

            // Assert
            Assert.That(result.GetResult().Count, Is.EqualTo(1));
            Assert.That(result.IsSuccess);
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEmployeeGridModelAsyncByEmployeeId_FilterByEmployeeIdReturnsError_ThenFail()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadGateway
            };
            var value = "5803080";
            var field = "Id";

            var service = CreateService(response, false);

            // Act
            var result = await service.GetEmployeeGridModelAsyncByEmployeeId(
                value,
                field);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsError);
            Assert.That(result.Error.error, Is.EqualTo("No Result Found"));
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task GetEmployeeGridModelAsyncByEmployeeId_FilterByEmployeeIdThrowsError_ThenFailAndHandleError()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadGateway
            };
            var value = "5803080";
            var field = "Id";

            var service = CreateService(response, true);

            // Act
            var result = await service.GetEmployeeGridModelAsyncByEmployeeId(
                value,
                field);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsError);
            Assert.That(result.Error.error.IndexOf("Test Exception Message"), Is.GreaterThan(0));
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteEmployeeAsync_When200Request_ThenReturnSuccessIsTrue()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK
            };
            var service = this.CreateService(response, false);
            int employeeId = 0;

            // Act
            var result = await service.DeleteEmployeeAsync(
                employeeId);

            // Assert
            Assert.IsTrue(result.GetResult());
            Assert.IsTrue(result.IsSuccess);
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteEmployeeAsync_When400Request_ThenReturnFailWithReasonPhrase()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ReasonPhrase = "BadRequest"
            };
            var service = this.CreateService(response, false);
            int employeeId = 0;

            // Act
            var result = await service.DeleteEmployeeAsync(
                employeeId);

            // Assert
            Assert.IsTrue(result.IsError);
            Assert.That(result.Error.error, Is.EqualTo("BadRequest"));
            Assert.IsFalse(result.IsSuccess);
            _mockRepository.VerifyAll();
        }

        [Test]
        public async Task DeleteEmployeeAsync_WhenThrowsError_ThenReturnFailWithExceptionMessage()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest,
            };
            var service = this.CreateService(response, true);
            int employeeId = 0;

            // Act
            var result = await service.DeleteEmployeeAsync(
                employeeId);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsError);
            Assert.That(result.Error.error.IndexOf("Test Exception Message"), Is.GreaterThan(0));
            _mockRepository.VerifyAll();
        }
    }
}
