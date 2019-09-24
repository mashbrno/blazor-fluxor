using Blazor.Fluxor;

namespace MiddlewareSample.Client.Store.FetchData
{
	public class GetForecastDataSuccessActionReducer : Reducer<FetchDataState, GetForecastDataSuccessAction>
	{
		public override FetchDataState Reduce(FetchDataState state, GetForecastDataSuccessAction action) =>
			new FetchDataState(
				isLoading: false,
				errorMessage: null,
				forecasts: action.WeatherForecasts);
	}
}
