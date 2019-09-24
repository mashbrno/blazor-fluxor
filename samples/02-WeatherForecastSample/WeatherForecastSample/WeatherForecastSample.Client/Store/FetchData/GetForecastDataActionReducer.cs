using Blazor.Fluxor;

namespace WeatherForecastSample.Client.Store.FetchData
{
	public class GetForecastDataActionReducer : Reducer<FetchDataState, GetForecastDataAction>
	{
		public override FetchDataState Reduce(FetchDataState state, GetForecastDataAction action) =>
			new FetchDataState(
				isLoading: true,
				errorMessage: null,
				forecasts: null);
	}
}
