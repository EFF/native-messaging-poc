var NATIVE_APP_NAME = "__enter_native_app_name__",
    MESSAGING_METHODS_MAP = {
    'native' : sendNativeMessage,
    'socket' : sendMessageToSocket
}

var port,
    messagingMethod;

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

function sendMessageToSocket(message){
    console.log('WEBSOCKET NOT IMPLEMENTED YET', message)
}

function sendMessage(message){
    messagingMethod.call(this, message);
}

function updateConnectionStatus() {
    var statusElement = $('#status');

    var status = navigator.onLine ? "Connecté" : "Déconnecté";
    var className = navigator.onLine ? "text-success" : "text-danger";

    statusElement.removeClass();
    statusElement.text(status);
    statusElement.addClass(className);
}

function updateMessagingMethodChanged(){
    var _messagingMethod = $(this).val();
    messagingMethod = MESSAGING_METHODS_MAP[_messagingMethod];
}

$(window).load(function(){
    connectToNativeApp();
    updateConnectionStatus();

    $('#message-form').submit(function(e){
        var message = $("#message-input").val()
        sendMessage(message);
        
        e.preventDefault();
    });

    $('#messaging-method-select').change(updateMessagingMethodChanged);
    window.addEventListener('online',  updateConnectionStatus);
      window.addEventListener('offline', updateConnectionStatus);
});
