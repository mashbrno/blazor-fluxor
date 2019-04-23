using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace Blazor.Fluxor.Routing
{
	internal class GoEffect : Effect<Go>
	{
		private readonly IUriHelper UriHelper;

		public GoEffect(IUriHelper uriHelper)
		{
			UriHelper = uriHelper;
		}

		protected override Task HandleAsync(Go action, IDispatcher dispatcher)
		{
			Uri fullUri = UriHelper.ToAbsoluteUri(action.NewUri);
			if (fullUri.ToString() != UriHelper.GetAbsoluteUri())
			{
				// Only navigate if we are not already at the URI specified
				UriHelper.NavigateTo(action.NewUri);
			}
			return Task.CompletedTask;
		}
	}
}
