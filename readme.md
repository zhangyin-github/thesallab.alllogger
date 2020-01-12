# thesallab.alllogger
![The SaL Lab](sallab.png)

AllLogger is a toolset used by the SaL Lab to log volunteers' search
 behaviors. It consists of the following tools:
* A .NET executable to log:
  * keyboard key presses,
  * mouse key presses and movements,
  * clipboard changes,
  * title changes of active windows,
  * eye fixations (not available in the open-source edition due to law issues).
* [A Firefox/Chrome add-on](2%20Add-on) to log:
  * web page requests and referrers,
  * activations of tabs.
* [A server endpoint to work with the Firefox/Chrome add-on](3%20Add-on%20Server).
* [A Visual Studio extension to snapshot the active code tab](4%20Extension).
## License
AllLogger is open-source under the MIT license. But please let us know if
 you use AllLogger in your project by sending an email to zhangyin(at)mail.neu.ed.cn