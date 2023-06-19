# IoT Power Phone
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/17870f15-86a6-43f0-bbe9-b31057bafa4d)

If you want to learn how to build other types of devices with Azure IoT and a Raspberry Pi check out this awesome simiulator which integrated with Azure IoT Service in real time. [Raspberry Pi Azure IoT Online Simulator](https://azure-samples.github.io/raspberry-pi-web-simulator/)

## Physical Buildout
I really wanted to make this device look like the bat phone from the 60's.  I spend a lot of time at antique shops looks for old "direct" phone like you would use in a factory to call a specific location.  Finally on one of my trips to Hocking Hills I found one.  I wanted the device to ring and blink like the original so I wired up all up on a solder free breadboard. I first used a larger breadboard for testing them moved everything to a smaller one that installed inside the phone with double sided tape.  I also spend a lot of time with Red Apple colordered spray paint.

Here is the pinout from the Raspberry Pi  
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/5e429538-0a1c-4d6a-b404-ebadffcc733b)

Breadboard showing LED  
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/023a469a-b60c-4509-b437-f6ac3bd13ef8)

Breakboard showing Buzzer  
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/627f5dc9-b5e7-44cd-be11-5c7bd4ef31f8)

Final smaller breadboard installed inside phone  
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/890c5772-7090-42c7-9033-db9f716a1bcb)

Overall view of the inside after installation was complete  
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/10bc9fed-8355-4ff3-b755-769dad965267)

Soooo much paint  
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/6325bf92-1f5c-40e7-8101-95d946855a96)


## Setup for Raspberry Pi with .Net Core
I used the scripted install to add .Net core onto the Raspberry Pi devices.
[Scripted Install](https://docs.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install)

Download the files onto the device using wget  
'''wget https://dot.net/v1/dotnet-install.sh'''

Run the following command from the terminal.   
'''sudo bash dotnet-install.sh -c Current -InstallDir $HOME/.dotnet'''

This will ensure that every time you open a bash terminal dotnet will be available.  
'''printf '\nexport DOTNET_ROOT=$HOME/.dotnet' >> .bashrc'''  
'''printf '\nexport PATH=$PATH:$HOME/.dotnet' >> .bashrc'''  

*NOTE: it is important to use the single quotes around these otherwise the variables will be expanded within the text file.

If you want to check to make sure the file was updated correctly you can run the following
Cat .bashrc

## Setting up the PowerPhone.service Variables
In order for the device to connect to Azure IoT central and listen for commands we need to create a service that runs on the Pi and is actively listenting.  Details for enabling this service can be found in the How to Deploy section, here we will walk through the settings in the service file and where to get those values.

|Name	|Value |Description|
| --- | --- | --- |
|IOTHUB_DEVICE_SECURITY_TYPE	|DPS  |The IoT Hub Device Provisioning Service is a helper service that enables just-in-time profiviions to the IoT hub.  Do not change this value.|
|IOTHUB_DEVICE_DPS_ENDPOINT	|global.azure-devices-provisioning.net  |This is the gloabl endpoint for DPS provisioning, in this example there is no reason to change this value.|
|IOTHUB_DEVICE_DPS_ID_SCOPE	|In your IoT Central application, navigate to Permissions > Device connection groups. Make a note of the ID scope value.| ![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/25e3e650-4e76-4f45-a293-6d2df1c66579)|
|IOTHUB_DEVICE_DPS_DEVICE_ID |power-phone |Decide what you want to call this specific device. In this example I named the device power-phone. If you have multiple devices you may wan to also add in a number at the end of the name like power-power-01|
|IOTHUB_DEVICE_DPS_DEVICE_KEY	| You will generate this in the Azure Cloud Shell | In your IoT Central application, navigate to Permissions > Device connection groups > SAS-IoT-Devices.<br><br>Make a note of the shared access signature Primary key value. ![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/70e72227-fa5b-4d94-84b5-ef2bbfbae23a)<br><br>Open the Azure Cloud Shell and run the following commands: <br><br>```az extension add --name azure-iot```<br>```az iot central device compute-device-key --device-id <<device name you chose>> --pk <the SAS key you copied>```<br><br>![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/607bf7b5-6395-434e-8d67-81ff1fe1f172)<br><br>Copy and past the generated device key as the value for this attribute.|

Additional references:
[Tutorial: Create and connect a client application to your Azure IoT Central application](https://learn.microsoft.com/en-us/azure/iot-central/core/tutorial-connect-device?pivots=programming-language-ansi-c#get-connection-information)

## How to Deploy

-Create a deployment folder on the Raspberry Pi

``sudo mkdir /srv/PowerPhone``

-Ensure your user owns the folder

``sudo chown pi /srv/PowerPhone``

-From the project folder run this command to publish the app self contained for linux.  This will ensure all the depdencies are included for the linux platform.

``dotnet publish -c Release -o /srv/PowerPhone -r linux-arm``

-Open the PiService/PowerPhone.service file
-Update the environment variables to match your device
-Copy the file to the /lib/systemd/system

``sudo cp /home/pi/IoTPowerPhone/PowerPhone/PiService/PowerPhone.service /lib/systemd/system``

-Start the service and make sure there are no errors

``sudo systemctl start PowerPhone``

-Ensure that the service starts up on every reboot.

``sudo systemctl enable PowerPhone``

## Create Canvas App and Flow
I also created a simple power app that would allow me to ring the phone manually.  The call to the device is done through Power Automate using the Azure IoT Central connector.

-First create a simple Canvas app with a button.
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/c1da8c2b-a989-447d-9fe7-fa82c2bc7c61)

-Create a new Power Automate Flow which will be run when the button is clicked. Because the command I created is within the "Ringer" component of the devices, I chose the _Run a component command_ activity.
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/4d4f078a-3484-4f1f-9544-99cc5ee9ab61)

-Configure the activity based on your settings.  The Device Id should match the device name you used in the PowerPhone.Service file.
![image](https://github.com/rwilson504/IoTPowerPhone/assets/7444929/df88f9ad-7c12-4ad2-8841-ddac996d906a)
