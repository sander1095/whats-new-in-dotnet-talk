#!/usr/bin/env dotnet

#:package Colorful.Console@1.2.15

var message = args.FirstOrDefault() ?? "Hello World!";
Colorful.Console.WriteAscii(message);