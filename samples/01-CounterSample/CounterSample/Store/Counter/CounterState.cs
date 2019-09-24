namespace CounterSample.Store.Counter
{
	public class CounterState
	{
		public int ClickCount { get; private set; }

		public CounterState(int clickCount)
		{
			ClickCount = clickCount;
		}
	}
}
