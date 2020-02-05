using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kakadu.DTO;
using Newtonsoft.Json;

namespace Kakadu.ActionApi.Clients
{
    public class ClientBase
    {
        private const string EXCEPTION_TASK_CANCELLED_BY_CALLER = "Task has been cancelled by the caller";
        private const string EXCEPTION_TIMEOUT_OCCURRED = "Timeout occurred while waiting for response from Configuration API";
        private const string ApiClientMediaTypeJson = "application/json";

        private readonly HttpClient Client;

        public ClientBase(HttpClient client) => Client = client;

        internal async Task<string> GetAsync(Uri uri)
        {
            string responseContent = string.Empty;
            try
            {
                using (var response = await Client.GetAsync(uri))
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }
            }
            catch(TaskCanceledException exc)
            {
                if (exc.CancellationToken.IsCancellationRequested)
                    throw new Exception(EXCEPTION_TASK_CANCELLED_BY_CALLER, new Exception(uri?.ToString()));
                else
                    throw new Exception(EXCEPTION_TIMEOUT_OCCURRED, new Exception(uri?.ToString()));
            }
            catch(Exception exc)
            {
                HandleExceptionInternal(exc, responseContent, uri?.ToString());
            }

            return responseContent;
        }

        internal async Task<string> PostAsync(object obj, Uri uri)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (uri == null)
                throw new ArgumentNullException("uri");

            string responseContent = string.Empty;

            try
            {
                var payload = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, ApiClientMediaTypeJson);
                using (var response = await Client.PostAsync(uri, payload))
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (TaskCanceledException exc)
            {
                if (exc.CancellationToken.IsCancellationRequested)
                    throw new Exception(EXCEPTION_TASK_CANCELLED_BY_CALLER, new Exception(uri?.ToString()));
                else
                    throw new Exception(EXCEPTION_TIMEOUT_OCCURRED, new Exception(uri?.ToString()));
            }
            catch (Exception exc)
            {
                HandleExceptionInternal(exc, responseContent, uri?.ToString());
            }

            return responseContent;
        }

        internal async Task<string> PatchAsync(object obj, Uri uri)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (uri == null)
                throw new ArgumentNullException("uri");

            string responseContent = string.Empty;

            try
            {
                var payload = new StringContent(JsonConvert.SerializeObject(obj), Encoding.UTF8, ApiClientMediaTypeJson);
                using (var response = await Client.PatchAsync(uri, payload))
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (TaskCanceledException exc)
            {
                if (exc.CancellationToken.IsCancellationRequested)
                    throw new Exception(EXCEPTION_TASK_CANCELLED_BY_CALLER, new Exception(uri?.ToString()));
                else
                    throw new Exception(EXCEPTION_TIMEOUT_OCCURRED, new Exception(uri?.ToString()));
            }
            catch (Exception exc)
            {
                HandleExceptionInternal(exc, responseContent, uri?.ToString());
            }

            return responseContent;
        }

        internal async Task DeleteAsync(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");

            string responseContent = string.Empty;
            try
            {
                using (var response = await Client.DeleteAsync(uri))
                {
                    responseContent = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (TaskCanceledException exc)
            {
                if (exc.CancellationToken.IsCancellationRequested)
                    throw new Exception(EXCEPTION_TASK_CANCELLED_BY_CALLER, new Exception(uri?.ToString()));
                else
                    throw new Exception(EXCEPTION_TIMEOUT_OCCURRED, new Exception(uri?.ToString()));
            }
            catch (Exception exc)
            {
                HandleExceptionInternal(exc, responseContent, uri?.ToString());
            }
        }

        internal bool TryDeserialize<T>(string content, out T result)
        {
            result = default(T);
            try
            {
                result = JsonConvert.DeserializeObject<T>(content);
                return true;
            }
            catch(Exception)
            {
                throw new Exception("Unable to deserialize response");
            }
        }

        private void HandleExceptionInternal(Exception exception, string response, string url)
        {
            // TODO: adapt to ApiError

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            if (string.IsNullOrWhiteSpace(response))
                throw new Exception($"{url}: {exception.Message}");

            throw new Exception($"{url}: {response}", exception);
        }
    }
}