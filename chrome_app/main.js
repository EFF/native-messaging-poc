var nativePort,
    viewModel,
    websocketConnection;

var NATIVE_APP_NAME = "com.mentum.native.proof",
    WEB_RECEIVER_APP_ID = "hmicbhcbfohplllmheflpledfgllklnl",
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
    nativePort = chrome.runtime.connectNative(NATIVE_APP_NAME);
    nativePort.onDisconnect.addListener(onDisconnected);
    nativePort.onMessage.addListener(onMessageFromNative);
}

function sendNativeMessage(message) {
  nativePort.postMessage(message);
}

function onMessageFromNative (message) {
    viewModel.inBoundMessage(message.data);
}

// CROSS APP MESSAGE PASING
function sendMessageToApp (message) {
    chrome.runtime.sendMessage(WEB_RECEIVER_APP_ID, message);
}

// WEBSOCKET
function connectToSocket () {
    websocketConnection = $.connection(SOCKET_URI)
    websocketConnection.start();
    websocketConnection.error(socketError);
    websocketConnection.received(onMessageFromNative)
}

function socketError (error){
    console.warn('Error in socket connection', error);
}

function sendMessageToSocket(message){
    websocketConnection.send(message);
}

// NETWORK CONNECTION
function updateNetworkConnectionStatus() {
    viewModel.connectionStatus.text(navigator.onLine ? "Connecté" : "Déconnecté");
    viewModel.connectionStatus.className(navigator.onLine ? "text-success" : "text-danger");
}

function MessagingViewModel() {
    this.destinationsAllowedMethods = [
        {name : 'Application native', allowedMethodsMap: NATIVE_MESSAGING_METHODS},
        {name : 'Application web', allowedMethodsMap: CROSS_APP_MESSAGING_METHODS}
    ];
    this.allowedMethods = ko.observable();
    this.selectedMessagingMethod = ko.observable();
    this.message = ko.observable();
    this.connectionStatus = {
        text : ko.observable(),
        className: ko.observable()
    };
    this.inBoundMessage = ko.observable();

    this.sendMessage = function (){
        var messagingMethod = this.selectedMessagingMethod()
        messagingMethod.call(this, this.message());
        this.message('');
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
    viewModel = new MessagingViewModel();
    ko.applyBindings(viewModel);
}

function initializeApp () {
    initializeViewModel();
    connectToNativeApp();
    connectToSocket();
    updateNetworkConnectionStatus();
}

$(window).load(function()   {    
    initializeApp();

    window.addEventListener('online',  updateNetworkConnectionStatus);
    window.addEventListener('offline', updateNetworkConnectionStatus);
});
