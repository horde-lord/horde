using Autofac;
using Core.Interfaces.Data;
using Core.Utilities;
using System.Net.Http.Json;

namespace Core.Services
{
    public class CallbackService : BaseService
    {
        public CallbackService(ILifetimeScope scope) 
            : base(scope, ContextNames.Ecosystem)
        {
        }
        private async Task<Tout> CallbackAsync<Tin, Tout>(HttpMethod method, Tin input, string uri)
        {
            var request = new HttpRequestMessage(method, uri);
            request.Content = JsonContent.Create(input);
            var response = await Http.SendAsync(request);
            return await response.Content.ReadFromJsonAsync<Tout>();
        }

        
    }
}
