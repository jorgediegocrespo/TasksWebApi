using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TasksWebApi.Models;

namespace TasksWebApi.Tests.Controllers.v1_0;

[TestClass]
public class TaskControllerTests : BaseControllerTests
{
    private static readonly string url = "/api/v1.0/task";

    [TestInitialize]
    public async Task InitializeAsync()
    {
        var factory = await BuildWebApplicationFactoryAsync(Guid.NewGuid().ToString());
        _client = factory.CreateClient();
        AddApiKeyHeader();
        await UserSignIn();
    }
    
    [TestCleanup]
    public async Task CleanupAsync()
    {
        await DeleteDatabaseAsync();
        _client.Dispose();
    }
    
    [TestMethod]
    public async Task get_all_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/getAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task get_all_unauthorized_with_api_key_without_token()
    {
        RemoveBearerTokenHeader();
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/getAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task get_all_null_bad_request()
    {
        var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/getAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task get_all_page_size_too_sort_bad_request()
    {
        var pagination = new TaskPaginationRequest(1, 0, 1);
        var content = new StringContent(JsonSerializer.Serialize(pagination), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/getAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task get_all_page_size_too_long_bad_request()
    {
        var pagination = new TaskPaginationRequest(1, 101, 1);
        var content = new StringContent(JsonSerializer.Serialize(pagination), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/getAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task get_all_ok()
    {
        var pagination = new TaskPaginationRequest(1, 2, 1);
        var content = new StringContent(JsonSerializer.Serialize(pagination), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/getAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
    
    [TestMethod]
    public async Task get_by_id_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var response = await _client.GetAsync($"{url}/1");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task get_by_id_unauthorized_with_api_key_without_token()
    {
        RemoveBearerTokenHeader();
        var response = await _client.GetAsync($"{url}/1");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task get_by_id_not_found()
    {
        var response = await _client.GetAsync($"{url}/1456");

        Assert.AreEqual(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
    
    [TestMethod]
    public async Task get_by_id_ok()
    {
        var response = await _client.GetAsync($"{url}/2");

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
    
    [TestMethod]
    public async Task post_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task post_unauthorized_with_api_key_without_token()
    {
        RemoveBearerTokenHeader();
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task post_null_bad_request()
    {
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task post_name_bad_request()
    {
        var creatingTask = new CreateTaskRequest(1, "abc", null);
        var content = new StringContent(JsonSerializer.Serialize(creatingTask), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task post_list_not_exists_conflict()
    {
        var creatingTask = new CreateTaskRequest(5, "Task 8", null);
        var content = new StringContent(JsonSerializer.Serialize(creatingTask), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task post_created()
    {
        var creatingTask = new CreateTaskRequest(2, "Task 8", null);
        var content = new StringContent(JsonSerializer.Serialize(creatingTask), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_unauthorized_with_api_key_without_token()
    {
        RemoveBearerTokenHeader();
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_null_bad_request()
    {
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_name_bad_request()
    {
        var updatingTask = new UpdateTaskRequest(1, new byte[] {0x01}, 1, "abc", null);
        var content = new StringContent(JsonSerializer.Serialize(updatingTask), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_list_not_exists_conflict()
    {
        var dbTask = _dbTasks.First(x => x.Id == 1);
        var updatingTask = new UpdateTaskRequest(1, dbTask.RowVersion, 3, "Task 8", null);
        var content = new StringContent(JsonSerializer.Serialize(updatingTask), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_id_not_exists_conflict()
    {
        var updatingTask = new UpdateTaskRequest(4, new byte[]{}, 1, "Task 8", null);
        var content = new StringContent(JsonSerializer.Serialize(updatingTask), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_concurrency_conflict()
    {
        var updatingTask = new UpdateTaskRequest(1, new byte[]{0x01}, 2, "Task 1 updated", null);
        var content = new StringContent(JsonSerializer.Serialize(updatingTask), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_no_content()
    {
        var dbTask = _dbTasks.First(x => x.Id == 1);
        var updatingTask = new UpdateTaskRequest(1, dbTask.RowVersion, 2, "Task 1 updated", null);
        var content = new StringContent(JsonSerializer.Serialize(updatingTask), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_unauthorized_with_api_key_without_token()
    {
        RemoveBearerTokenHeader();
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_null_bad_request()
    {
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_task_not_exists_conflict()
    {
        var deleteRequest = new DeleteRequest(4, new byte[]{});
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_concurrency_conflict()
    {
        var deleteRequest = new DeleteRequest(2, new byte[]{});
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_no_content()
    {
        var dbTask = _dbTasks.First(x => x.Id == 2);
        var deleteRequest = new DeleteRequest(2, dbTask.RowVersion);
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_all_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/deleteAll", content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_all_unauthorized_with_api_key_without_token()
    {
        RemoveBearerTokenHeader();
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/deleteAll", content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_all_null_bad_request()
    {
        var content = new StringContent(String.Empty, Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/deleteAll", content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_all_list_not_exists_conflict()
    {
        var deleteRequest = new DeleteRequest(3, new byte[]{});
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/deleteAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_all_concurrency_conflict()
    {
        var deleteRequest = new DeleteRequest(4, new byte[]{});
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/deleteAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_all_no_content()
    {
        var dbTask = _dbTaskLists.First(x => x.Id == 4);
        var deleteRequest = new DeleteRequest(4, dbTask.RowVersion);
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/deleteAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }
}