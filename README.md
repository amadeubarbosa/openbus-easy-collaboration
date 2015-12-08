# OpenBus Easy Collaboration Lib

## Lib Java

### Building

* cd java
* mvn compile

### Running an example

* edit src/main/resources/test.properties (or target/classes/test.properties)
* execute the sender: mvn exec:java -Dexec.mainClass=demo.Sender &
* execute the receiver: mvn exec:java -Dexec.mainClass=demo.Receiver &

## Lib CSharp

* cd csharp
* open the solution in Visual Studio 2010 (or above): OpenBus-Easy-Collaboration.sln
* edit demo\Sender\bin\Debug\Sender.exe.config demo\Receiver\bin\Debug\Receiver.exe.config
* execute the sender: demo\Sender\Bin\Debug\Sender.exe
* execute the receiver: demo\Receiver\Bin\Debug\Receiver.exe