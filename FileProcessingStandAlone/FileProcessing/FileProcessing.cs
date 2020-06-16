using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Data;
using System.Fabric.Description;
using System.Diagnostics;
//using CSS.Cloud.Framework;

namespace CSS.Connector.FileProcessing
{
    /// <summary>
    /// The FabricRuntime creates an instance of this class for each service type instance. 
    /// </summary>
    internal sealed class FileProcessing : StatelessService
    {
        public FileProcessing(StatelessServiceContext context)
            : base(context)
        {
            InboundFileWatcher.Path = context.CodePackageActivationContext.WorkDirectory;
        }

        /// <summary>
        /// Optional override to create listeners (like tcp, http) for this service instance.
        /// </summary>
        /// <returns>The collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
			try
			{
				bool allowHTTP = true;

				var endpoints = Context.CodePackageActivationContext.GetEndpoints()
				.Where(endpoint => endpoint.Name != "DebugEndpoint" && (endpoint.Protocol == EndpointProtocol.Http && allowHTTP == true)
									|| endpoint.Protocol == EndpointProtocol.Https);

#if DEBUG
				// When running in our localhost, we want to define a static Port number for the service, so we switch here to the debug endpoint defined
				// in the ServiceManifest.xml file.
				endpoints = Context.CodePackageActivationContext.GetEndpoints()
				.Where(endpoint => (endpoint.Name == "DebugEndpoint"));
#endif


				return endpoints.Select(endpoint => new ServiceInstanceListener(serviceContext =>
				  // New Service Fabric listener for each endpoint configuration in the manifest.
				  new KestrelCommunicationListener(serviceContext, endpoint.Name, (url, listener) =>
				  {
					  return new WebHostBuilder()
					  .UseKestrel()
					.ConfigureServices(services => services.AddSingleton<StatelessServiceContext>(serviceContext))
					.UseContentRoot(Directory.GetCurrentDirectory())
					.UseStartup<Startup>()
					.UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
					.UseUrls(url)
					.Build();
				  }), endpoint.Name));

			}
			catch (Exception ex)
			{
				Trace.WriteLine("An error occured in the CreateServiceInstanceListeners method: {0}", ex.ToString());
				Logger.Log("CSS.Connector.FileProcessing", "CreateServiceInstanceListeners", ex);
				throw;
			}
		}
	}
}
