using Microsoft.JSInterop;
using System;

namespace Blazor.Fluxor
{
	/// <summary>
	/// Provides standard interactions with the browser via Javascript
	/// </summary>
	public static class BrowserInterop
	{
		private const string OnPageLoadedId = "Blazor.Fluxor.OnPageLoaded";
		/// <summary>
		/// Executed when the browser finishes loading the page
		/// </summary>
		public static event EventHandler PageLoaded;

		/// <summary>
		/// Gets Javascripts required to support the features of this class
		/// </summary>
		/// <returns></returns>
		public static string GetClientScripts()
		{
			string assemblyName = typeof(BrowserInterop).Assembly.GetName().Name;

			return $@"
setTimeout(function() {{
	if (window.DotNet) {{
		DotNet.invokeMethodAsync('Blazor.Fluxor', 'Blazor.Fluxor.OnPageLoaded');
	}}
}}, 20);

";
		}

		/// <summary>
		/// Called from JavaScript when document.ready is executed
		/// </summary>
		[JSInvokable(OnPageLoadedId)]
		//TODO: Make private if private callbacks are permitted https://github.com/aspnet/Blazor/issues/1218
		public static void OnPageLoaded()
		{
			PageLoaded?.Invoke(null, EventArgs.Empty);
		}
	}
}
