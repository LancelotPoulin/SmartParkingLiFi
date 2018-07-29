# SmartParkingLiFi

Visit http://lancelotpoulin.com/projects to find more informations about this (in french).

A Xamarin mobile application which allow the customer to reserve and access to the parking with the Li-Fi technology. The Li-Fi is a standard for data communication by light. My part consists of developing the client mobile application and the access point at the parking entrance.

Xamarin application is the SmartParkingLiFi folder and the Code::Blocks C++ console app (access point) is the SmartParkingLiFiBorne folder.

The most interesting part is the region "LiFi" in TabbedMainPage.cs where you will find methods to receive Li-Fi Data using Oledcomm receiver connected with micro-USB to the Android smartphone. I use a Serial USB manager to get data from it. To send data I use the flashlight (I put it ON for 100ms to send "1", 200ms for "2" etc). And the C++ console app (access point) use a LDR (photoresistor) and a capacitor to detect the time the flashlight is ON. To send Li-Fi data, the Oledcomm module and LED light are connected to the UART ports (TX/RX) of the Raspberry.

Somehow it's like a bidirectionnal light data transmission between a smartphone and a computer (Raspberry). (flashlight to LDR is not "Li-Fi", Oledcomm module is Li-Fi, the difficult part is how to use it with a smartphone, and I succeeded !)

I think you should create new project and copy my files to avoid bugs of visual studio and xamarin.
To use the code, you need to install some NuGet packages corresponding to unfindables "using" directives like "LusoVU.XamarinUsbSerialForAndroid.0.2.3" or "Plugin.CurrentActivity.1.0.1" etc...
The application is connected to a webservice to log users, you can just delete this page.

Contact me at poulinponnelle.lancelot@outlook.com if you have any questions.

-------------------------------------------------------------------------------------------------------------------------------------------
Transfert de données par la lumière Light data communication LiFi Li-Fi Visual Studio Xamarin Forms Android Application Mobile Smartphone
Code::Blocks Oledcomm receiver C++ C# Flashlight transmission Photoresistor Photorésistance LDR Capacitor Condensateur
Envoyer et recevoir des données par la lumière phone raspberry Send and receive date by light
