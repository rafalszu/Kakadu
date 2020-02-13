using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kakadu.DTO.HttpExceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Kakadu.Common.Tests")]
namespace Kakadu.Common.HttpClients
{
    public class HttpClientBase
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientBase> _logger;

        public Dictionary<string, IEnumerable<string>> CustomRequestHeaders { get; set; }

        public HttpClientBase(HttpClient client, ILogger<HttpClientBase> logger)
        {
            _httpClient = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected async Task<T> GetAsync<T>(Uri uri, CancellationToken cancellationToken) => await SendAsync<T>(HttpMethod.Get, uri, null, cancellationToken);
        
        protected async Task<T> GetAsync<T>(string relativeUri, CancellationToken cancellationToken) => await GetAsync<T>(new Uri(relativeUri, UriKind.Relative), cancellationToken);

        protected async Task<T> PostAsync<T>(object obj, Uri uri, CancellationToken cancellationToken) => await SendAsync<T>(HttpMethod.Post, uri, obj, cancellationToken);

        protected async Task<T> PostAsync<T>(object obj, string relativeUri, CancellationToken cancellationToken) => await PostAsync<T>(obj, new Uri(relativeUri, UriKind.Relative), cancellationToken);

        protected async Task<T> PatchAsync<T>(object obj, Uri uri, CancellationToken cancellationToken) => await SendAsync<T>(HttpMethod.Patch, uri, obj, cancellationToken);

        protected async Task<T> PatchAsync<T>(object obj, string relativeUri, CancellationToken cancellationToken) => await PatchAsync<T>(obj, new Uri(relativeUri, UriKind.Relative), cancellationToken);

        protected async Task<string> DeleteAsync(Uri uri, CancellationToken cancellationToken) => await SendAsync<string>(HttpMethod.Delete, uri, null, cancellationToken);

        protected async Task<string> DeleteAsync(string relativeUri, CancellationToken cancellationToken) => await DeleteAsync(new Uri(relativeUri, UriKind.Relative), cancellationToken);

        internal async Task<T> SendAsync<T>(HttpMethod httpMethod, Uri uri, object obj, CancellationToken cancellationToken)
        {
            if (uri == null)
                throw new ArgumentNullException("uri");
            
            using(var request = new HttpRequestMessage(httpMethod, uri))
            {
                using(var httpContent = CreateHttpContent(obj))
                {
                    request.Content = httpContent;
                    AddCustomRequestHeaders(request);

                    using(var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                        return await TryGetContentAsync<T>(response);
                }
            }
        }

        internal void AddCustomRequestHeaders(HttpRequestMessage request)
        {
            if(CustomRequestHeaders == null || !CustomRequestHeaders.Keys.Any())
                return;

            foreach(var customHeader in CustomRequestHeaders.Keys)
            {
                if(string.IsNullOrWhiteSpace(customHeader) || CustomRequestHeaders[customHeader] == null || !CustomRequestHeaders[customHeader].Any())
                    continue;
                    
                request.Headers.Add(customHeader, CustomRequestHeaders[customHeader]);
            }            
        }

        internal async Task<T> TryGetContentAsync<T>(HttpResponseMessage response)
        {
            if(response == null || response.Content == null)
                return default;
            
            var stream = await response.Content.ReadAsStreamAsync();
            if (response.IsSuccessStatusCode)
                return DeserializeJsonFromStream<T>(stream);

            var content = await StreamToStringAsync(stream);
            throw new HttpResponseException(response.StatusCode, content);            
        }

        internal void SerializeJsonIntoStream(object value, Stream stream)
        {
            if(stream == null || value == null)
                return;
            
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

        internal HttpContent CreateHttpContent(object content)
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

        internal T TryDeserialize<T>(JsonTextReader jsonReader, bool throwException = true)
        {
            var result = default(T);
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
                    throw new Exception("Response not understandable");
            }

            return result;
        }

        internal T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var reader = new StreamReader(stream))
                return TryDeserialize<T>(new JsonTextReader(reader));                
        }

        internal async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync();

            return content;
        }
    }
}