using Autofac;
using Horde.Core.Interfaces.Data;
using Horde.Core.Services;
using Horde.Core.Utilities;
using System.Net.Http.Json;

namespace Horde.Core.Domains.Admin.Services
{
    public class CallbackService : BaseService
    {
        public CallbackService(ILifetimeScope scope)
            : base(scope, ContextNames.World)
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
