/**
 * @author Zhang, Yin
 */

/******** Basic setups ********/

let ep = "ws://127.0.0.1:8081/";

var websocket = {
    ws: new WebSocket(ep + "endpoint"),

    send: function (json) {
        if (json.timestamp == null) {
            json.timestamp = Date.now();
        }
        this.ws.send(JSON.stringify(json));
    }
};

function initializeWebsocket() {
    websocket.ws.onclose = function (event) {
        setTimeout(() => {
            websocket.ws = new WebSocket(ep + "endpoint");
            initializeWebsocket();
        }, 500);
    };
}

initializeWebsocket();