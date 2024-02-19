using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



public class CircleAccessSettings
{
    public string AppKey { get; set; }
    public string LoginUrl { get; set; }
    public string ReadKey { get; set; }
    public string WriteKey { get; set; }
}

public class CircleAccessStatus
{
    public Boolean Success { get; set; }
    public string Email { get; set; }
    public string ErrorMsg { get; set; }
}

public class CircleAccess
{
    string AppKey { get; }
    string ReadKey { get; }
    string WriteKey { get; }
    private readonly HttpClient _httpClient;

    public CircleAccess(string appKey, string readKey, string writeKey)
    {
        _httpClient = new HttpClient();
        AppKey = appKey;
        ReadKey = readKey;
        WriteKey = writeKey;
    }

    /*public async Task<Boolean> (string sessionId, string userId)
    {
        try
        {
            var dataObj = new { sessionID = sessionId, userID = userId };
            var sig = ComputeSignature(JsonConvert.SerializeObject(dataObj), WriteKey);
            var obj = new { data = dataObj, signature = sig };

            using HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 1, 0, 0);
            client.DefaultRequestHeaders.Add("x-ua-appKey", AppKey);
            var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

            var r = client.PostAsync("https://circleaccess.circlesecurity.ai/api/user/session/expire", content).Result;
            return r.StatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return false;
    }
*/
    public async Task<dynamic> GetSessionAsync(string sessionId, Boolean bExpireSession = true)
    {
        try
        {
            string toSign = string.Format($"?s={sessionId}");
            string sig = ComputeSignature(toSign, WriteKey);
            string URL = string.Format($"https://circleaccess.circlesecurity.ai/api/session/{toSign}&signature={sig}");

            using HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 1, 0, 0);
            client.DefaultRequestHeaders.Add("x-ua-appKey", AppKey);
            var response = await client.GetAsync(URL);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string responseString = await response.Content.ReadAsStringAsync();
                dynamic obj = JObject.Parse(responseString);

                // Assuming obj.Data is a JToken (JSON object)
                JToken toTest = obj.data;
                // Convert the JToken to a string without formatting
                string rawJson = JsonConvert.SerializeObject(toTest, new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.Default
                });
                string toCheck = ComputeSignature(rawJson, ReadKey);
                string signature = obj.signature;
                if (toCheck != signature)
                {
                    Console.WriteLine("Signature check failed!");
                    return null;
                }

                dynamic data = obj.data;
                string userID = data.userID;
                if (bExpireSession)
                {
                    if (!await ExpireSessionAsync(sessionId, userID))
                    {
                        Console.WriteLine("Failed to expire session!");
                    }
                }

                return data;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return null;
    }

    public async Task<string> CreateAuthorizationAsync(string returnUrl, string question, string customID, object[] approvals)
    {
        try
        {
            var dataObj = new
            {
                returnUrl = returnUrl,
                question = question,
                customID = customID,
                approvals = approvals
            };

            var signature = ComputeSignature(JsonConvert.SerializeObject(dataObj), WriteKey);
            var obj = new { data = dataObj, signature = signature };

            using HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 1, 0, 0);
            client.DefaultRequestHeaders.Add("x-ua-appKey", AppKey);
            var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://circleaccess.circlesecurity.ai/api/authorization/create/", content);
            // return response.StatusCode == HttpStatusCode.OK;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                return responseData.data.authID;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

        }
        return null;
    }

    public async Task<dynamic> GetAuthorizationContract(string authID)
    {
        try
        {
            string toSign = string.Format($"?authID={authID}");
            string sig = ComputeSignature(toSign, WriteKey);
            string URL = $"https://circleaccess.circlesecurity.ai/api/authorization/get/{toSign}&signature={sig}";

            using HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 1, 0, 0);
            client.DefaultRequestHeaders.Add("x-ua-appKey", AppKey);

            var response = await client.GetAsync(URL);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                dynamic obj = JObject.Parse(responseData);

                // Assuming obj.Data is a JToken (JSON object)
                JToken toTest = obj.data;
                // Convert the JToken to a string without formatting

                string rawJson = JsonConvert.SerializeObject(toTest, new JsonSerializerSettings
                {
                    StringEscapeHandling = StringEscapeHandling.Default
                });
                string toCheck = ComputeSignature(rawJson, ReadKey);
                string signature = obj.signature;

                if (toCheck != signature)
                {
                    Console.WriteLine("Signature check failed!");
                    return null;
                }

                dynamic data = obj.data;
                return data;
            }
            else
            {
                Console.WriteLine($"Failed to retrieve authorization contract. Status code: {response.StatusCode}");
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return null;
    }

    public async Task<Boolean> ExpireSessionAsync(string sessionId, string userId)
    {
        try
        {
            var dataObj = new { sessionID = sessionId, userID = userId };
            var sig = ComputeSignature(JsonConvert.SerializeObject(dataObj), WriteKey);
            var obj = new { data = dataObj, signature = sig };

            using HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 1, 0, 0);
            client.DefaultRequestHeaders.Add("x-ua-appKey", AppKey);
            var content = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json");

            var r = client.PostAsync("https://circleaccess.circlesecurity.ai/api/user/session/expire", content).Result;
            return r.StatusCode == HttpStatusCode.OK;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return false;
    }

    public async Task<CircleAccessStatus> GetScanningEmailAsync(string sessionId, string userId, List<string> emailsToCheck)
    {
        CircleAccessStatus ret = new CircleAccessStatus();
        ret.Success = false;
        dynamic data = await GetSessionAsync(sessionId);
        if (data == null)
        {
            ret.ErrorMsg = "Unable to get the session!";
            return ret;
        }

        string status = data.status;
        if ((!string.IsNullOrEmpty(status)) && (status == "expired"))
        {
            ret.ErrorMsg = "Expired session!";
            return ret;
        }

        string userIdTest = data.userID;
        if (string.Compare(userIdTest, userId) != 0)
        {
            ret.ErrorMsg = "Passed in user id doesn't match what's in the session!";
            return ret;
        }

        if (data.userHashedEmails != null) // just in case they didn't register an email
        {
            HashSet<string> hs = data.userHashedEmails.ToObject<HashSet<string>>();
            foreach (string email in emailsToCheck)
            {
                string toCheck = HashToHex(email.ToLower());
                if (hs.Contains(toCheck))
                {
                    ret.Success = true;
                    ret.ErrorMsg = "No error";
                    ret.Email = email;
                    return ret;
                }
            }
        }

        ret.Success = true;
        ret.ErrorMsg = "No match found.";
        ret.Email = "";
        return ret;
    }
    string ComputeSignature(string stringToSign, string secret)
    {
        using (var hmacsha256 = new HMACSHA256(System.Text.ASCIIEncoding.UTF8.GetBytes(secret)))
        {
            var bytes = Encoding.ASCII.GetBytes(stringToSign);
            var hashedBytes = hmacsha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashedBytes);
        }
    }

    string HashToHex(string str)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }
    }
}
