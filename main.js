var NATIVE_APP_NAME = "__enter_native_app_name__"

var port;

function onDisconnected () {
	console.log('DISCONNECTED FROM APP');
}

function connectToNativeApp() {
  port = chrome.runtime.connectNative(NATIVE_APP_NAME);
  port.onDisconnect.addListener(onDisconnected);
}

function sendNativeMessage(message) {
  port.postMessage(message);
}

$(window).load(function(){
	connectToNativeApp();

	$('#message-form').submit(function(e){
		var message = $("#message-input").val()
		sendNativeMessage(message);
		
		e.preventDefault();
	});
});