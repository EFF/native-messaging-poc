var messageViewModel = {
    message: ko.observable()
}

function onMessage (message) {
    messageViewModel.message(message);
    chrome.app.window.current().focus();
}

function initializeViewModel () {
    var koSecureBindingOptions = {
       attribute: "data-bind",
       globals: window, 
       bindings: ko.bindingHandlers,
       noVirtualElements: false
    };

    ko.bindingProvider.instance = new ko.secureBindingsProvider(koSecureBindingOptions);
    ko.applyBindings(messageViewModel);
}

initializeViewModel();
chrome.runtime.onMessageExternal.addListener(onMessage);
