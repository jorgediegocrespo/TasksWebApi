using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TasksWebApi.Models;

namespace TasksWebApi.Tests.Controllers.v1_0;

[TestClass]
public class UserControllerTest : BaseControllerTests
{
    private static readonly string url = "/api/v1.0/user";

    [TestInitialize]
    public async Task InitializeAsync()
    {
        var factory = await BuildWebApplicationFactoryAsync(Guid.NewGuid().ToString());
        _client = factory.CreateClient();
        AddApiKeyHeader();
    }
    
    [TestCleanup]
    public async Task CleanupAsync()
    {
        await DeleteDatabaseAsync();
        _client.Dispose();
    }
    
    [TestMethod]
    public async Task signin_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signin", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signin_null_bad_request()
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signin", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signin_password_too_sort_bad_request()
    {
        var signInInfo = new SignInRequest("user", "_Ab1");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signin", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signin_password_without_digit_unauthorized()
    {
        var signInInfo = new SignInRequest("user", "!_-ABCabcdef");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signin", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signin_password_without_lowercase_unauthorized()
    {
        var signInInfo = new SignInRequest("user", "!_-ABCABC123");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signin", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signin_password_without_upercase_unauthorized()
    {
        var signInInfo = new SignInRequest("user", "!_-abcabc123");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signin", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signin_password_without_non_alphanumeric_unauthorized()
    {
        var signInInfo = new SignInRequest("user", "123ABCabc123");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signin", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signin_ok()
    {
        var signInInfo = new SignInRequest("user", "!_-ABCabc123");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signin", content);

        var result = await response.Content.ReadAsStringAsync();
        var tokenInfo = JsonSerializer.Deserialize<TokenResponse>(result, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(tokenInfo != null);
        Assert.IsFalse(string.IsNullOrWhiteSpace(tokenInfo.Token));
        Assert.IsFalse(string.IsNullOrWhiteSpace(tokenInfo.RefreshToken));
    }
    
    [TestMethod]
    public async Task signup_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signup", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signup_null_bad_request()
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signup", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signup_password_too_sort_bad_request()
    {
        var signupInfo = new SignUpRequest("userTest", "user@test.com", "_Ab1");
        var content = new StringContent(JsonSerializer.Serialize(signupInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signup", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signup_password_without_digit_bad_request()
    {
        var signupInfo = new SignUpRequest("userTest", "user@test.com", "!_-ABCabcdef");
        var content = new StringContent(JsonSerializer.Serialize(signupInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signup", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signup_password_without_lowercase_bad_request()
    {
        var signInInfo = new SignUpRequest("userTest", "user@test.com", "!_-ABCABC123");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signup", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signup_password_without_upercase_bad_request()
    {
        var signInInfo = new SignUpRequest("userTest", "user@test.com", "!_-abcabc123");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signup", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signup_password_without_non_alphanumeric_bad_request()
    {
        var signInInfo = new SignUpRequest("userTest", "user@test.com", "123ABCabc123");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signup", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signup_incorrect_email_bad_request()
    {
        var signInInfo = new SignUpRequest("userTest", "usertest", "!_-ABCabc123");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signup", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task signup_ok()
    {
        var signInInfo = new SignUpRequest("userTest", "user@test.com", "!_-ABCabc123");
        var content = new StringContent(JsonSerializer.Serialize(signInInfo), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/signup", content);

        var result = await response.Content.ReadAsStringAsync();
        var tokenInfo = JsonSerializer.Deserialize<TokenResponse>(result, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(tokenInfo != null);
        Assert.IsFalse(string.IsNullOrWhiteSpace(tokenInfo.Token));
        Assert.IsFalse(string.IsNullOrWhiteSpace(tokenInfo.RefreshToken));
    }
    
    [TestMethod]
    public async Task delete_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_null_bad_request()
    {
        await AdminSignIn();
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_user_not_exists_conflict()
    {
        await AdminSignIn();
        var signupInfo = new UserDeleteRequest("noUser");
        var content = new StringContent(JsonSerializer.Serialize(signupInfo), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_user_not_allowed_to_remove_other_user_forbidden()
    {
        await UserSignIn();
        var signupInfo = new UserDeleteRequest("diego");
        var content = new StringContent(JsonSerializer.Serialize(signupInfo), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_super_admin_user_not_allowed_to_be_removed_forbidden()
    {
        await AdminSignIn();
        var signupInfo = new UserDeleteRequest("admin");
        var content = new StringContent(JsonSerializer.Serialize(signupInfo), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_user_with_task_list_conflict()
    {
        await AdminSignIn();
        var signupInfo = new UserDeleteRequest("user");
        var content = new StringContent(JsonSerializer.Serialize(signupInfo), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_own_user_ok()
    {
        await EmptySignIn();
        var signupInfo = new UserDeleteRequest("diego");
        var content = new StringContent(JsonSerializer.Serialize(signupInfo), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_other_user_by_super_admin_ok()
    {
        await AdminSignIn();
        var signupInfo = new UserDeleteRequest("diego");
        var content = new StringContent(JsonSerializer.Serialize(signupInfo), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task refresh_token_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/refreshToken", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task refresh_token_null_bad_request()
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/refreshToken", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task refresh_token_bad_token_unauthorized()
    {
        var user = await EmptySignIn();
        var refreshTokenRequest = new RefreshTokenRequest("1234", user.RefreshToken);
        var content = new StringContent(JsonSerializer.Serialize(refreshTokenRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/refreshToken", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task refresh_token_already_used_unauthorized()
    {
        var user = await EmptySignIn();
        var refreshTokenRequest = new RefreshTokenRequest(user.Token, "12345678");
        var content = new StringContent(JsonSerializer.Serialize(refreshTokenRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/refreshToken", content);
        response = await _client.PostAsync($"{url}/refreshToken", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task refresh_token_ok()
    {
        var user = await EmptySignIn();
        var refreshTokenRequest = new RefreshTokenRequest(user.Token, user.RefreshToken);
        var content = new StringContent(JsonSerializer.Serialize(refreshTokenRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/refreshToken", content);
        
        var result = await response.Content.ReadAsStringAsync();
        var tokenInfo = JsonSerializer.Deserialize<TokenResponse>(result, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue(tokenInfo != null);
        Assert.IsFalse(string.IsNullOrWhiteSpace(tokenInfo.Token));
        Assert.IsFalse(string.IsNullOrWhiteSpace(tokenInfo.RefreshToken));
    }
}