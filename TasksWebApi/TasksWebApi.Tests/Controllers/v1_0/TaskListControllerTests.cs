using System.Text;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TasksWebApi.Models;

namespace TasksWebApi.Tests.Controllers.v1_0;

[TestClass]
public class TaskListControllerTests : BaseControllerTests
{
    private static readonly string url = "/api/v1.0/tasklist";

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
        var pagination = new PaginationRequest(0, 1);
        var content = new StringContent(JsonSerializer.Serialize(pagination), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/getAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task get_all_page_size_too_long_bad_request()
    {
        var pagination = new PaginationRequest(101, 1);
        var content = new StringContent(JsonSerializer.Serialize(pagination), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/getAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task get_all_ok()
    {
        var pagination = new PaginationRequest(2, 1);
        var content = new StringContent(JsonSerializer.Serialize(pagination), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync($"{url}/getAll", content);

        Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
    }
    
    [TestMethod]
    public async Task get_by_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var response = await _client.GetAsync($"{url}/1456");

        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task get_by_unauthorized_with_api_key_without_token()
    {
        RemoveBearerTokenHeader();
        var response = await _client.GetAsync($"{url}/1456");

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
        var creatingTaskList = new CreateTaskListRequest("abc");
        var content = new StringContent(JsonSerializer.Serialize(creatingTaskList), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task post_task_list_name_exists_conflict()
    {
        var creatingTaskList = new CreateTaskListRequest("List 1");
        var content = new StringContent(JsonSerializer.Serialize(creatingTaskList), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task post_created()
    {
        var creatingTaskList = new CreateTaskListRequest("List 4");
        var content = new StringContent(JsonSerializer.Serialize(creatingTaskList), Encoding.UTF8, "application/json");
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
        var dbTaskList = _dbTaskLists.First(x => x.Id == 1);
        var updatingTaskList = new UpdateTaskListRequest(1, dbTaskList.RowVersion, "abc");
        var content = new StringContent(JsonSerializer.Serialize(updatingTaskList), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_name_exists_conflict()
    {
        var dbTaskList = _dbTaskLists.First(x => x.Id == 1);
        var updatingTaskList = new UpdateTaskListRequest(1, dbTaskList.RowVersion, "List 2");
        var content = new StringContent(JsonSerializer.Serialize(updatingTaskList), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_id_not_exists_conflict()
    {
        var updatingTaskList = new UpdateTaskListRequest(3, new byte[]{}, "List 5");
        var content = new StringContent(JsonSerializer.Serialize(updatingTaskList), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_id_concurrency_conflict()
    {
        var updatingTaskList = new UpdateTaskListRequest(2, new byte[]{0x01}, "List 5");
        var content = new StringContent(JsonSerializer.Serialize(updatingTaskList), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task put_no_content()
    {
        var dbTaskList = _dbTaskLists.First(x => x.Id == 2);
        var updatingTaskList = new UpdateTaskListRequest(2, dbTaskList.RowVersion, "List 2 updated");
        var content = new StringContent(JsonSerializer.Serialize(updatingTaskList), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync(url, content);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_unauthorized_without_api_key()
    {
        RemoveApiKeyHeader();
        var dbTaskList = _dbTaskLists.First(x => x.Id == 2);
        var deleteRequest = new DeleteRequest(2, dbTaskList.RowVersion);
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);
    
        Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_unauthorized_with_api_key_without_token()
    {
        RemoveBearerTokenHeader();
        var dbTaskList = _dbTaskLists.First(x => x.Id == 2);
        var deleteRequest = new DeleteRequest(2, dbTaskList.RowVersion);
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
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
    public async Task delete_task_list_not_exists_conflict()
    {
        var deleteRequest = new DeleteRequest(3, new byte[]{});
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_task_list_with_tasks_conflict()
    {
        var dbTaskList = _dbTaskLists.First(x => x.Id == 1);
        var deleteRequest = new DeleteRequest(1, dbTaskList.RowVersion);
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_task_list_concurrency_conflict()
    {
        var deleteRequest = new DeleteRequest(2, new byte[] {0x01});
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
    
    [TestMethod]
    public async Task delete_no_content()
    {
        var dbTaskList = _dbTaskLists.First(x => x.Id == 2);
        var deleteRequest = new DeleteRequest(2, dbTaskList.RowVersion);
        var content = new StringContent(JsonSerializer.Serialize(deleteRequest), Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"{url}/delete", content);

        Assert.AreEqual(System.Net.HttpStatusCode.NoContent, response.StatusCode);
    }
}