# OpenBus Easy Collaboration Lib

## Lib Java

### Building

* cd java
* mvn compile

### Running an example

* edit src/main/resources/test.properties (or target/classes/test.properties)
* execute the sender: mvn exec:java -Dexec.mainClass=demo.Sender &
* execute the receiver: mvn exec:java -Dexec.mainClass=demo.Receiver &

## Lib C#

* cd csharp
* open the solution in Visual Studio 2010 (or above): OpenBus-Easy-Collaboration.sln
