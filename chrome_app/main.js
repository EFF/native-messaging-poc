var port,
    nativeMessaging,
    websocketConnection;

var NATIVE_APP_NAME = "com.mentum.native.proof",
    SOCKET_URI = 'http://localhost:8085/connection',
    NATIVE_MESSAGING_METHODS = [
        {name : 'NativeMessaging', method: sendNativeMessage},
        {name : 'WebSocket', method: sendMessageToSocket}
    ],
    CROSS_APP_MESSAGING_METHODS = [{name : 'MessagePassing', method: sendMessageToApp}];

// NATIVE MESSAGING
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

// CROSS APP MESSAGE PASING
function sendMessageToApp (message) {
    console.warn('CROSS APP MESSAGING NOT IMPLEMENTED YET', message)
}

// WEBSOCKET
function connectToSocket () {
    websocketConnection = $.connection(SOCKET_URI)
    websocketConnection.start();
    websocketConnection.error(socketError);
}

function socketError (error){
    console.warn('Error in socket connection', error);
}

function sendMessageToSocket(message){
    websocketConnection.send(message);
}

// NETWORK CONNECTION
function updateNetworkConnectionStatus() {
    nativeMessaging.connectionStatus.text(navigator.onLine ? "Connecté" : "Déconnecté");
    nativeMessaging.connectionStatus.className(navigator.onLine ? "text-success" : "text-danger");
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

function initializeViewModel () {
    var koSecureBindingOptions = {
       attribute: "data-bind",
       globals: window, 
       bindings: ko.bindingHandlers,
       noVirtualElements: false
    };

    ko.bindingProvider.instance = new ko.secureBindingsProvider(koSecureBindingOptions);
    nativeMessaging = new MessagingViewModel();
    ko.applyBindings(nativeMessaging);
}

function initializeApp () {
    initializeViewModel()
    connectToNativeApp();
    connectToSocket();

    updateNetworkConnectionStatus();
}

$(window).load(function()   {    
    initializeApp();

    window.addEventListener('online',  updateNetworkConnectionStatus);
    window.addEventListener('offline', updateNetworkConnectionStatus);
});
