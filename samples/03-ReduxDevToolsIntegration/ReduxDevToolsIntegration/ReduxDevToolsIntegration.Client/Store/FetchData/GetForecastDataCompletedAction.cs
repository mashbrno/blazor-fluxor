using Blazor.Fluxor;
using ReduxDevToolsIntegration.Shared;

namespace ReduxDevToolsIntegration.Client.Store.FetchData
{
	public class GetForecastDataCompletedAction 
	{
		public string ErrorMessage { get; private set; }
		public WeatherForecast[] WeatherForecasts { get; private set; }

		public GetForecastDataCompletedAction(WeatherForecast[] weatherForecasts)
		{
			WeatherForecasts = weatherForecasts;
		}
		public GetForecastDataCompletedAction(string errorMessage)
		{
			ErrorMessage = errorMessage;
		}
	}
}