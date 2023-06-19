# IoT Power Phone

If you want to learn how to build other types of devices with Azure IoT and a Raspberry Pi check out this awesome simiulator which integrated with Azure IoT Service in real time. [Raspberry Pi Azure IoT Online Simulator](https://azure-samples.github.io/raspberry-pi-web-simulator/)

## Setup for Raspberry Pi with .Net Core

Video coming soon

## Setting up the PowerPhone.service Variables
In order for the device to connect to Azure IoT central and listen for commands we need to create a service that runs on the Pi and is actively listenting.  Details for enabling this service can be found in the How to Deploy section, here we will walk through the settings in the service file and where to get those values.

Name	Value Description
IOTHUB_DEVICE_SECURITY_TYPE	DPS  The IoT Hub Device Provisioning Service is a helper service that enables just-in-time profiviions to the IoT hub.  Do not change this value.
IOTHUB_DEVICE_DPS_ENDPOINT	global.azure-devices-provisioning.net  This is the gloabl endpoint for DPS provisioning, in this example there is no reason to change this value.
IOTHUB_DEVICE_DPS_ID_SCOPE	In your IoT Central application, navigate to Permissions > Device connection groups. Make a note of the ID scope value.
IOTHUB_DEVICE_DPS_DEVICE_ID Decide what you want to call this specific device.  In this example I named the device power-phone.  If you have multiple devices you may wan to also add in a number at the end of the name like power-power-01
IOTHUB_DEVICE_DPS_DEVICE_KEY	In your IoT Central application, navigate to Permissions > Device connection groups > SAS-IoT-Devices. Make a note of the shared access signature Primary key value.



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
