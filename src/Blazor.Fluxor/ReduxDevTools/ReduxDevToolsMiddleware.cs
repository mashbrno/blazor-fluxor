using Blazor.Fluxor.ReduxDevTools.CallbackObjects;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Blazor.Fluxor.ReduxDevTools
{
	/// <summary>
	/// Middleware for interacting with the Redux Devtools extension for Chrome
	/// </summary>
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
			var state = (IDictionary<string, object>)new ExpandoObject();
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
				var newFeatureStates = (IDictionary<string, object>)Json.Deserialize<object>(e.state);
				foreach (KeyValuePair<string, object> newFeatureState in newFeatureStates)
				{
					// Get the feature with the given name
					if (!Store.Features.TryGetValue(newFeatureState.Key, out IFeature feature))
						continue;

					// Get the generic method of JsonUtil.Deserialize<> so we have the correct object type for the state
					string deserializeMethodName = nameof(Json.Deserialize);
					MethodInfo deserializeMethodInfo = typeof(Json)
						.GetMethod(deserializeMethodName)
						.GetGenericMethodDefinition()
						.MakeGenericMethod(new Type[] { feature.GetStateType() });

					// Get the state we were given as a json string
					string serializedFeatureState = newFeatureState.Value?.ToString();
					// Deserialize that json using the generic method, so we get an object of the correct type
					object stronglyTypedFeatureState =
						string.IsNullOrEmpty(serializedFeatureState)
						? null
						: deserializeMethodInfo.Invoke(null, new object[] { serializedFeatureState });

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
