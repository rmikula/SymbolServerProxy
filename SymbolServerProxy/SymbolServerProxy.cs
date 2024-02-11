using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;

namespace SymbolServerProxy
{
    public class SymbolServerProxy
    {
        public const string SymbolServerCfg = "SymbolServerCfg";

        private readonly ILogger<SymbolServerProxy> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public SymbolServerProxy(ILogger<SymbolServerProxy> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        [Function("PATInjector")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Symbols/{pat}/{name}/{id}/{file}")] HttpRequest req,
            string pat, string name, string id, string file)
        {
            try
            {
                var fileStream = await GetSymbols(pat, name, id, file);
                return new FileStreamResult(fileStream, "application/octet-stream");
            }
            catch (HttpRequestException ex)
            {
                return new StatusCodeResult(statusCode: (int)(ex.StatusCode ?? HttpStatusCode.InternalServerError));
            }
        }

        public async Task<Stream> GetSymbols(string pat, string name, string id, string file)
        {

            using var client = _httpClientFactory.CreateClient(SymbolServerCfg);
            
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{""}:{pat}")));

            using var response = await client.GetAsync($"RomanMikula/_apis/symbol/symsrv/{name}/{id}/{file}");

            response.EnsureSuccessStatusCode();

            var stream = new MemoryStream();
            await (await response.Content.ReadAsStreamAsync()).CopyToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }
    }
}
