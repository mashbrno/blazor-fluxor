using Blazor.Fluxor;

namespace MiddlewareSample.Client.Store.Counter
{
	public class IncrementActionReducer : Reducer<CounterState, IncrementCounterAction>
	{
		public override CounterState Reduce(CounterState state, IncrementCounterAction action) =>
			new CounterState(state.ClickCount + 1);
	}
}
