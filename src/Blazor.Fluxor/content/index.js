const TryInitializeFluxor = function () {
	var initializeFluxorScriptElement = document.getElementById("InitializeFluxor");
	if (initializeFluxorScriptElement) {
		const script = initializeFluxorScriptElement.innerHTML.replace("<!--!-->", "");
		eval(script);
	} else {
		setTimeout(() => TryInitializeFluxor(), 100);
	}
};
TryInitializeFluxor();	