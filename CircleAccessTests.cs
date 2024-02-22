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
        _circleAccessSession = new CircleAccess("APP_ID", "READ_KEY", "WRITE_KEY");

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


    // Get Session, get user session and expire session tests

    [Test]
    public async Task GetSessionAsync_withSessionID()
    {
        string sessionID = "session5rSDM3EFGVw8Si8B2s8hCWzxT5XdpiADZ";
        string userID = "46369f9d8baa24567f04472d1988991766adf45687ef5fc6553fbc2a86c675ba";
        var result = await _circleAccessSession.GetSessionAsync(sessionID);
        Console.WriteLine(result);
        //string jsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
        //Console.WriteLine(jsonString);
        ClassicAssert.AreEqual(userID, result.userID.ToString());
    }

    [Test]
    public async Task GetUserSessionAsync_withSessionIDandUserID()
    {
        string sessionID = "session5rSDM3EFGVw8Si8B2s8hCWzxT5XdpiADZ";
        string userID = "46369f9d8baa24567f04472d1988991766adf45687ef5fc6553fbc2a86c675ba";
        dynamic result = await _circleAccessSession.GetUserSessionAsync(sessionID, userID);
        Console.WriteLine(result);
        //string jsonString = JsonConvert.SerializeObject(result, Formatting.Indented);
        //Console.WriteLine(jsonString);
        ClassicAssert.AreEqual(userID, result.userID.ToString());
    }
    [Test]
    public async Task ExpireUserSessionAsync_withSessionIDandUserID()
    {
        string sessionID = "session5rSDM3EFGVw8Si8B2s8hCWzxT5XdpiADZ";
        string userID = "46369f9d8baa24567f04472d1988991766adf45687ef5fc6553fbc2a86c675ba";
        var result = await _circleAccessSession.ExpireSessionAsync(sessionID, userID);
        Console.WriteLine(result);
        ClassicAssert.AreEqual(userID, result.userID.ToString());
    }


    //Create Authorization and get Authorization Tests

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
        ClassicAssert.IsNotNull(result);
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
        ClassicAssert.IsNotNull(result);
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
        ClassicAssert.IsNotNull(expectedData);
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
        string err = "You need the returnUrl parameter";
        ClassicAssert.AreEqual(err, result);
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
        string err = "You need the question parameter. Min 10 characters. Max 310 characters";
        ClassicAssert.AreEqual(err, result);
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
        string err = "You need the customID parameter. Max 256 characters. The customID parameter can be anything to track back to your system. Example: 'session-123'";
        ClassicAssert.AreEqual(err, result);
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
        string err = "You need at least one approval";
        ClassicAssert.AreEqual(err, result);
    }
    [Test]
    public async Task CreateAuhtorization_NullEmail_NullPhone_InApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
          new Approval {
                  email = null,
                  weight = 100,
                  required = false,
                  phone = null
              }
          };
        Console.WriteLine("Create Authorization with No Email & No Phone in Approval");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        string err = "Approval object needs at least one parameter. Email or phone ";
        ClassicAssert.AreEqual(err, result);
    }
    [Test]
    public async Task CreateAuhtorization_NullEmail_ValidPhone_InApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
          new Approval {
                  email = null,
                  weight = 100,
                  required = false,
                  phone = "+919090909090"
              }
          };
        Console.WriteLine("Create Authorization with No Email & Valid Phone in Approval");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        // string err = "Approval object needs at least one parameter. Email or phone ";
        ClassicAssert.IsNotNull(result);
    }
    [Test]
    public async Task CreateAuhtorization_NullEmail_InValidPhone_InApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
          new Approval {
                  email = null,
                  weight = 100,
                  required = false,
                  phone = "9090909090" // Invalid Phone means not starting with country code
              }
          };
        Console.WriteLine("Create Authorization with No Email & InValid Phone in Approval");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        string err = "Approval phone is not valid.The phone parameter needs the country code with + and digits only. Min 10 digits. Max 16 digits. Example: +14074139270";
        ClassicAssert.AreEqual(err, result);
    }
    [Test]
    public async Task CreateAuhtorization_ValidEmail_NullPhone_InApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
          new Approval {
                  email = "curcio@me.com",
                  weight = 100,
                  required = false,
                  phone = null // Invalid Phone means not starting with country code
              }
          };
        Console.WriteLine("Create Authorization with Valid Email & Null Phone in Approval");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        ClassicAssert.IsNotNull(result);
    }
    [Test]
    public async Task CreateAuhtorization_ValidEmail_ValidPhone_InApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
          new Approval {
                  email = "curcio@me.com",
                  weight = 100,
                  required = false,
                  phone = "+919090909090"
              }
          };
        Console.WriteLine("Create Authorization with Valid Email & Valid Phone in Approval");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        string err = "Approval can only have one parameter. Email or phone";
        ClassicAssert.AreEqual(err, result);
    }
    [Test]
    public async Task CreateAuthorization_WeightMoreThan100_InApproval()
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
        Console.WriteLine("Create Authorization with Weight More then 100 in Approval");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        string err = "Approval weight can not be more than 100";
        ClassicAssert.AreEqual(err, result);
    }
    [Test]
    public async Task CreateAuthorization_SumofWeightLessThan100_InApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
          new Approval {
                  email = "curcio@me.com",
                  weight = 5,
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
        Console.WriteLine("Create Authorization with Sum of weight <100");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        string err = "Approval weights must add up to at least 100";
        ClassicAssert.AreEqual(err, result);
    }
    [Test]
    public async Task CreateAuthorization_SumofWeightGreaterThan100_InApproval()
    {
        string returnUrl = "http://circleaccess.circlesecurity.ai/demo/authorization/";
        string question = "Confirm 1 ETH withdrawal?";
        string customId = "123";
        var approvals = new Approval[] {
          new Approval {
                  email = "curcio@me.com",
                  weight = 50,
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
        Console.WriteLine("Create Authorization with Sum of weight >=100");
        var result = await _circleAccessSession.CreateAuthorizationAsync(returnUrl, question, customId, approvals);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        ClassicAssert.IsNotNull(result);
    }
    [Test]
    public async Task GetAuthorizationContract_WithInvalidAuthID()
    {
        string authID = "";
        Console.WriteLine("Get Authorization Contract with Invalid AuthID");
        var result = await _circleAccessSession.GetAuthorizationContract(authID);
        if (result == null) Console.WriteLine("Error");
        else Console.WriteLine(result);
        string err = "Unauthorized";
        ClassicAssert.AreEqual(err, result);
    }
}
