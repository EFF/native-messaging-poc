var port,
    messaging;

var NATIVE_APP_NAME = "test_native_app",
    NATIVE_MESSAGING_METHODS = [
        {name : 'NativeMessaging', method: sendNativeMessage},
        {name : 'WebSocket', method: sendMessageToSocket}
    ],
    CROSS_APP_MESSAGING_METHODS = [{name : 'MessagePassing', method: sendMessageToApp}];

function onDisconnected () {
    console.log('DISCONNECTED FROM APP');
}

function connectToNativeApp() {
  port = chrome.runtime.connectNative(NATIVE_APP_NAME);
  port.onDisconnect.addListener(onDisconnected);
}

function sendMessageToApp (message) {
    console.log('CROSS APP MESSAGING NOT IMPLEMENTED YET', message)
}

function sendNativeMessage(message) {
  port.postMessage(message);
}

function sendMessageToSocket(message){
    console.log('WEBSOCKET NOT IMPLEMENTED YET', message)
}

function updateConnectionStatus() {
    messaging.connectionStatus.text(navigator.onLine ? "Connecté" : "Déconnecté");
    messaging.connectionStatus.className(navigator.onLine ? "text-success" : "text-danger");
}

function initializeApp () {
    messaging = new MessagingViewModel();
    ko.applyBindings(messaging);
    
    connectToNativeApp();
    updateConnectionStatus();
}

function MessagingViewModel() {
    this.destinationsAllowedMethods = [
        {name : 'Application native', allowedMethodsMap: NATIVE_MESSAGING_METHODS},
        {name : 'Application web', allowedMethodsMap: CROSS_APP_MESSAGING_METHODS}
    ];
    this.allowedMethods = ko.observable();
    this.selectedMessagingMethod = ko.observable();
    this.message = '';
    this.connectionStatus = {
        text : ko.observable(),
        className: ko.observable()
    };

    this.sendMessage = function (){
        var messagingMethod = this.selectedMessagingMethod()
        messagingMethod.call(this, this.message);
    }
}  

$(window).load(function()   {    
    initializeApp();

    window.addEventListener('online',  updateConnectionStatus);
    window.addEventListener('offline', updateConnectionStatus);
});
