# AllLogger Firefox/Chrome Add-on
The AllLogger Firefox/Chrome Add-on is to log:
  * web page requests and referrers,
  * activations of tabs.
# Dependencies
The [AllLogger Add-on Server](../3%20Add-on%20Server) is required to use the
 add-on.
# How to use
Setup the [AllLogger Add-on Server](../3%20Add-on%20Server).
Change the server endpoint in websocket.js to point to the add-on server:

    let ep = "ws://[AddonServerAddress]:[Port]/";