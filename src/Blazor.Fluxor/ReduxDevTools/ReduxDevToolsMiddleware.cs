using Blazor.Fluxor.ReduxDevTools.CallbackObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json = System.Text.Json.Serialization.JsonSerializer;

namespace Blazor.Fluxor.ReduxDevTools
{
	/// <summary>
	/// Middleware for interacting with the Redux Devtools extension for Chrome
	/// </summary>
	[Obsolete("Do not use until System.Text.Json.Serialization.JsonSerializer.Parse bug has been fixed https://github.com/dotnet/corefx/issues/38435")]
	public class ReduxDevToolsMiddleware : Middleware
	{
		private int SequenceNumberOfCurrentState = 0;
		private int SequenceNumberOfLatestState = 0;
		private readonly ReduxDevToolsInterop ReduxDevToolsInterop;

		/// <summary>
		/// Creates a new instance of the middleware
		/// </summary>
		public ReduxDevToolsMiddleware(ReduxDevToolsInterop reduxDevToolsInterop)
		{
			ReduxDevToolsInterop = reduxDevToolsInterop;
			ReduxDevToolsInterop.JumpToState += OnJumpToState;
			ReduxDevToolsInterop.Commit += OnCommit;
		}

		/// <see cref="IMiddleware.Initialize(IStore)"/>
		public override void Initialize(IStore store)
		{
			base.Initialize(store);
			ReduxDevToolsInterop.Init(GetState());
		}

		/// <see cref="IMiddleware.MayDispatchAction(IAction)"/>
		public override bool MayDispatchAction(IAction action)
		{
			return SequenceNumberOfCurrentState == SequenceNumberOfLatestState;
		}

		/// <see cref="IMiddleware.AfterDispatch(IAction)"/>
		public override void AfterDispatch(IAction action)
		{
			ReduxDevToolsInterop.Dispatch(action, GetState());

			// As actions can only be executed if not in a historical state (yes, "a" historical, pronounce your H!)
			// ensure the latest is incremented, and the current = latest
			SequenceNumberOfLatestState++;
			SequenceNumberOfCurrentState = SequenceNumberOfLatestState;
		}

		private IDictionary<string, object> GetState()
		{
			var state = new Dictionary<string, object>();
			foreach (IFeature feature in Store.Features.Values.OrderBy(x => x.GetName()))
				state[feature.GetName()] = feature.GetState();
			return state;
		}

		private void OnCommit(object sender, EventArgs e)
		{
			ReduxDevToolsInterop.Init(GetState());
			SequenceNumberOfCurrentState = SequenceNumberOfLatestState;
		}

		private void OnJumpToState(object sender, JumpToStateCallback e)
		{
			SequenceNumberOfCurrentState = e.payload.actionId;
			using (Store.BeginInternalMiddlewareChange())
			{
				var newFeatureStates = Json.Parse<Dictionary<string, object>>(e.state);
				foreach (KeyValuePair<string, object> newFeatureState in newFeatureStates)
				{
					// Get the feature with the given name
					if (!Store.Features.TryGetValue(newFeatureState.Key, out IFeature feature))
						continue;

					JsonElement serializedFeatureStateElement = (JsonElement)newFeatureState.Value;
					object stronglyTypedFeatureState = Json.Parse(serializedFeatureStateElement.ToString(), feature.GetStateType());

					byte[] bytes = Json.ToUtf8Bytes(stronglyTypedFeatureState, feature.GetStateType());
					string json = System.Text.UTF8Encoding.UTF8.GetString(bytes);
					System.Diagnostics.Debug.WriteLine("Feature json " + json);

					// Now set the feature's state to the deserialized object
					feature.RestoreState(stronglyTypedFeatureState);
				}
			}
		}

		/// <see cref="IMiddleware.GetClientScripts"/>
		public override string GetClientScripts()
		{
			return ReduxDevToolsInterop.GetClientScripts();
		}
	}
}
