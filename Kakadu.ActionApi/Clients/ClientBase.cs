using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.ActionApi.Models;
using Kakadu.DTO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kakadu.ActionApi.Clients
{
    public class ClientBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClientBase> _logger;

        public ClientBase(HttpClient client, ILogger<ClientBase> logger)
        {
            _httpClient = client;
            _logger = logger;
        }

        internal async Task<T> GetAsync<T>(Uri uri, CancellationToken cancellationToken) => await SendAsync<T>(HttpMethod.Get, uri, null, cancellationToken);
        
        internal async Task<T> GetAsync<T>(string relativeUri, CancellationToken cancellationToken) => await GetAsync<T>(new Uri(relativeUri, UriKind.Relative), cancellationToken);

        internal async Task<T> PostAsync<T>(object obj, Uri uri, CancellationToken cancellationToken) => await SendAsync<T>(HttpMethod.Post, uri, obj, cancellationToken);

        internal async Task<T> PostAsync<T>(object obj, string relativeUri, CancellationToken cancellationToken) => await PostAsync<T>(obj, new Uri(relativeUri, UriKind.Relative), cancellationToken);

        internal async Task<T> PatchAsync<T>(object obj, Uri uri, CancellationToken cancellationToken) => await SendAsync<T>(HttpMethod.Patch, uri, obj, cancellationToken);

        internal async Task<T> PatchAsync<T>(object obj, string relativeUri, CancellationToken cancellationToken) => await PatchAsync<T>(obj, new Uri(relativeUri, UriKind.Relative), cancellationToken);

        internal async Task<string> DeleteAsync(Uri uri, CancellationToken cancellationToken) => await SendAsync<string>(HttpMethod.Delete, uri, null, cancellationToken);

        internal async Task<string> DeleteAsync(string relativeUri, CancellationToken cancellationToken) => await DeleteAsync(new Uri(relativeUri, UriKind.Relative), cancellationToken);

        private async Task<T> SendAsync<T>(HttpMethod httpMethod, Uri uri, object obj, CancellationToken cancellationToken)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            
            using(var request = new HttpRequestMessage(httpMethod, uri))
            {
                using(var httpContent = CreateHttpContent(obj))
                {
                    request.Content = httpContent;

                    using(var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                    {
                        var stream = await response.Content.ReadAsStreamAsync();
                        if(response.IsSuccessStatusCode)
                            return DeserializeJsonFromStream<T>(stream);

                        var content = await StreamToStringAsync(stream);
                        throw new ApiException
                        {
                            StatusCode = (int)response.StatusCode,
                            Content = content
                        };
                    }
                }
            }
        }

        private void SerializeJsonIntoStream(object value, Stream stream)
        {
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            {
                using (var textWriter = new JsonTextWriter(writer) { Formatting = Formatting.None })
                {
                    var js = new JsonSerializer();
                    js.Serialize(textWriter, value);
                    textWriter.Flush();
                }
            }
        }

        private HttpContent CreateHttpContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var ms = new MemoryStream();
                SerializeJsonIntoStream(content, ms);
                ms.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(ms);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }

        private T TryDeserialize<T>(JsonTextReader jsonReader, out T result, bool throwException = true)
        {
            result = default(T);
            if(jsonReader == null)
                return result;

            try
            {
                using(jsonReader)
                {
                    var jsonSerializer = new JsonSerializer();
                    return jsonSerializer.Deserialize<T>(jsonReader);
                }
            }
            catch(Exception ex)
            {
                if(_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"Response not uderstandable: {ex.ToString()}");

                if(throwException)
                    throw new Exception("Response not uderstandable");
            }

            return result;
        }

        private T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var reader = new StreamReader(stream))
                return TryDeserialize<T>(new JsonTextReader(reader), out T result);                
        }

        private async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync();

            return content;
        }
    }
}