# SR-POC - Sample App - Embedded Browser Interaction - Listener Approach

## Background
.NET Windows Form App that uses a .NET browser based control to load an Esri ArcGIS WebApp - https://lol-iframe-example.azurewebsites.net/embeddedMap.html. 

This Esri ArcGIS WebApp has been created using the ArcGIS API for JavaScript - 3.27 - https://developers.arcgis.com/javascript/

<img src="https://raw.githubusercontent.com/Jiriteach/SR.POC-.NET-App/master/Screenshots/Screen%20Shot%202019-02-14%20at%2012.48.03.png" width="50%" height="50%">

## Purpose
The Esri ArcGIS WebApp is fully functional when embedded using a .NET browser based control within the .NET Windows Form App.

## Notes
1. The .NET browser based control for C# Windows Forms Apps, makes use of Internet Explorer as a rendering engine. The mode of IE has been specifically set to IE=edge for compatibility purposes and to ensure JavaScript works as expected.

2. Selections OR interactions in a JSON format, from the Esri ArcGIS WebApp (when embedded using a .NET browser based control) can be returned to the .NET Windows Form App.
3. This is made possible via an HTTP service listener which is started when the .NET Windows Form App is run. The default configuration has the HTTP service listener satrted on port 8989. This HTTP service listener can accept POST requests.
4. Selections OR interactions OR payloads from the Esri ArcGIS WebApp are passed as JSON to the HTTP service listener using XMLHttpRequest.
5. JSON is de-serialized within the .NET Windows Form App into an object which can then be easily accessed.
 
## Credits 
- Scott Tansley & Jithen Singh

## Copyright © 2019 - Eagle Technology Group Ltd
