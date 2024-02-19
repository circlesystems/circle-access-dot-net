using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework.Legacy;


public class Approval
{
    public string email { get; set; }
    public int weight { get; set; }
    public bool required { get; set; }
    public string phone { get; set; }
}
public class Data
{
    public Approval[] approvals { get; set; }
    public string authID { get; set; }
    public long creationMiliUnixTime { get; set; }
    public string customID { get; set; }
    public string factorID { get; set; }
    public string factorUrl { get; set; }
    public long lastUpdateMiliUnixTime { get; set; }
    public string question { get; set; }
    public string returnUrl { get; set; }
    public string state { get; set; }
    public string type { get; set; }
}

public class AuthorizationResponse
{
    public Data data { get; set; }
    public string signature { get; set; }
}

public interface IHttpClientWrapper
{
    Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content);
    Task<HttpResponseMessage> GetAsync(string requestUri);
}

[TestFixture]
public class CircleAccessTests
{
    private CircleAccess _circleAccessSession;
    private Mock<HttpClient> _mockHttpClient;

    [SetUp]
    public void Setup()
    {
        _mockHttpClient = new Mock<HttpClient>();
        _circleAccessSession = new CircleAccess("YOUR_APPKEY", "YOUR_READKEY", "YOUR_WRITEKEY");

        // Use reflection to set the private field '_httpClient'
        // var httpClientField = typeof(CircleAccessSession).GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);
        // httpClientField.SetValue(_circleAccessSession, _mockHttpClient.Object);
        if (_circleAccessSession != null)
        {
            // Use reflection to set the private field '_httpClient'
            var httpClientField = _circleAccessSession.GetType().GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance);

            if (httpClientField != null)
            {
                httpClientField.SetValue(_circleAccessSession, _mockHttpClient.Object);
            }
            else
            {
                // Log or handle the case where '_httpClient' field is not found
                Console.WriteLine("_httpClient field not found.");
            }
        }
        else
        {
            // Log or handle the case where '_circleAccessSession' is null
            Console.WriteLine("_circleAccessSession is null.");
        }
    }

    [Test]
    public async Task CreateAuthorizationAsync_ValidData_ReturnsAuthID()
    {
        // Arrange
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
        new Approval {
                email = "curcio@me.com",
                weight = 10,
                required = false,
                phone = null
            },
            new Approval {
                email = "curcio@me.com",
                weight = 90,
                required = false,
                phone = null
            }
        };
        var expectedAuthID = "auth7V9ySbXQHDgaNcbUsZU";
        // var responseContent = $"{{ \"data\": {{ \"authID\": \"{expectedAuthID}\" }} }}";
        // var responseMessage = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(responseContent) };
        // _httpClientWrapperMock.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).ReturnsAsync(responseMessage);

        // Act
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        Console.WriteLine("CreateAuthorization Function");
        Console.WriteLine(result);

        // Assert
        // ClassicAssert.AreEqual(expectedAuthID, result);
    }

    [Test]
    public async Task GetAuthorizationContract_ValidAuthID_ReturnsData()
    {
        // Arrange
        string authID = "auth7V9ySbXQHDgaNcbUsZU";
        string jsonData = @"
                {
                    ""data"": {
                        ""approvals"": [
                            {
                                ""email"": ""curcio@me.com"",
                                ""required"": false,
                                ""weight"": 10
                            },
                            {
                                ""email"": ""curcio@me.com"",
                                ""required"": false,
                                ""weight"": 90
                            }
                        ],
                        ""authID"": ""authDGKM9jEoRM5rGCbmRNd"",
                        ""creationMiliUnixTime"": 1707406227424,
                        ""customID"": ""123"",
                        ""factorID"": ""factorNxnL712RpqVXoAacHHWWWUSdUCAwdtEwJ"",
                        ""factorUrl"": ""https://circleaccess.circlesecurity.ai/2fa/appNvYLUHGqLJQYxUKdf2DBcRuNtWPTs7chc/factorNxnL712RpqVXoAacHHWWWUSdUCAwdtEwJ"",
                        ""lastUpdateMiliUnixTime"": 1707406227424,
                        ""question"": ""Confirm 1 ETH withdrawal?"",
                        ""returnUrl"": ""http://circleaccess.circlesecurity.ai/demo/authorization/"",
                        ""state"": ""pending"",
                        ""type"": ""authorization""
                    },
                    ""signature"": ""twklZM0tHgHrB9uHs06V63ZnMtdLA0ASC2JcHl8Xna0=""
                }";

        // Deserialize the JSON into AuthorizationResponse object
        // var expectedData = JsonConvert.DeserializeObject<AuthorizationResponse>(jsonData);
        dynamic expectedData = JsonConvert.DeserializeObject(jsonData);
        // var jsonString = JsonConvert.SerializeObject(expectedData);

        // Act
        var result = await _circleAccessSession.GetAuthorizationContract(authID);
        Console.WriteLine("GetAuthorizationContract Funciton");
        Console.WriteLine(result);
        Console.WriteLine(expectedData.data);

        // Assert
        // ClassicAssert.AreEqual(expectedData.data, result);
    }
    [Test]
    public async Task CreateAuhtorizationWithGetAuthorizationContract()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
        new Approval {
                email = "curcio@me.com",
                weight = 10,
                required = false,
                phone = null
            },
            new Approval {
                email = "curcio@me.com",
                weight = 90,
                required = false,
                phone = null
            }
        };
        Console.WriteLine("Create Authorization with Get Authorization Contract");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        Console.WriteLine(result);
        dynamic expectedData = await _circleAccessSession.GetAuthorizationContract(result);
        Console.WriteLine(expectedData);
    }

    [Test]
    public async Task CreateAuhtorization_WithEmptyCustomID()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "";
        var approvals = new Approval[] {
        new Approval {
                email = "curcio@me.com",
                weight = 10,
                required = false,
                phone = null
            },
            new Approval {
                email = "curcio@me.com",
                weight = 90,
                required = false,
                phone = null
            }
        };
        Console.WriteLine("Create Authorization with Empty Custom ID");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        ClassicAssert.IsNull(result);
    }

    [Test]
    public async Task CreateAuhtorization_WithEmptyQuestion()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "";
        string customId = "123";
        var approvals = new Approval[] {
        new Approval {
                email = "curcio@me.com",
                weight = 10,
                required = false,
                phone = null
            },
            new Approval {
                email = "curcio@me.com",
                weight = 90,
                required = false,
                phone = null
            }
        };
        Console.WriteLine("Create Authorization with Empty Question");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        ClassicAssert.IsNull(result);
    }

    [Test]
    public async Task CreateAuhtorization_WithEmptyReturnURL()
    {
        string returnUrl = "";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
        new Approval {
                email = "curcio@me.com",
                weight = 10,
                required = false,
                phone = null
            },
            new Approval {
                email = "curcio@me.com",
                weight = 90,
                required = false,
                phone = null
            }
        };
        Console.WriteLine("Create Authorization with Empty Return Url");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        ClassicAssert.IsNull(result);
    }
    [Test]
    public async Task CreateAuhtorization_WithEmptyApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] { };
        Console.WriteLine("Create Authorization with Empty Approvals");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        ClassicAssert.IsNull(result);
    }
    [Test]
    public async Task CreateAuhtorization_InvalidEmailInApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
        new Approval {
                email = null,
                weight = 10,
                required = false,
                phone = null
            }
        };
        Console.WriteLine("Create Authorization with Invalid Email in Approval");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        ClassicAssert.IsNull(result);
    }
    [Test]
    public async Task CreateAuthorization_InvalidWeightInApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
        new Approval {
                email = "curcio@me.com",
                weight = 1000,
                required = false,
                phone = null
            },
            new Approval {
                email = "curcio@me.com",
                weight = 90,
                required = false,
                phone = null
            }
        };
        Console.WriteLine("Create Authorization with Invalid Weight in Approval");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        ClassicAssert.IsNull(result);
    }
}
