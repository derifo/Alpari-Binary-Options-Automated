# Alpari-Binary-Options-Automated
The program allows to automate binary options trading with Alpari broker. Written in C# .NET for Windows.

# How it works

It logs in to Alpari website and opens a websocket connection to their binary trading platform. 

Then it waits for special files in special local folder. When you (or your bot) wants to open a position, you should create a file there with Instrument name, pos size etc. And the position will be opened automatically. 

# For better usage

You are free to modify the source code to meet your needs. For example, you can make a DLL library and attach it to your external project.
