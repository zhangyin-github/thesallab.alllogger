# AllLogger Visual Studio Extension
The AllLogger Visual Studio Extension is to snapshot the active code tab of
 Visual Studio.
* A snapshot is created when the keyboard is idle for 2 seconds.
* Only the active code tab (in which the last keypress happens) is
 snapshotted.
* Files are zipped to save space.
* Files are saved at %LocalApplicationData%\AllLoggerVisualStudioExtension\
## How to build
Install the "Visual Studio extension development" workload of Visual Studio
 2019 to build the source codes.