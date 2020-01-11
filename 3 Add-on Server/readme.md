# AllLogger Add-on Server
The AllLogger Add-on Server is to work with the [AllLogger Firefox/Chrome add-on](../3%20Add-on)
 to log data into MongoDB.
## Dependencies
The AllLogger Add-on Server use maven to manage all the dependencies. However,
 [thesallab.configuration](https://github.com/zhangyin-github/thesallab.configuration)
 and
 [thesallab.foundation](https://github.com/zhangyin-github/thesallab.configuration)
 have to be installed manually using maven.

Apache Tomcat 8.0/8.5 and MongoDB 3.4 are required to run the AllLogger Add-on
 Server.
## How to run
Add the following jvm parameters:

    -Dcx.config.file=/path/to/your/configuration/file
On Windows:

    -Dcx.config.file=X:/path/to/your/configuration/file
In the configuration file, set:

    alllogger.server.user.mongodbservers=[MongoDBServerAddress]:[Port]