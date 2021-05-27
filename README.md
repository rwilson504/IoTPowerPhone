# IoT Power Phone

If you want to learn how to build other types of devices with Azure IoT and a Raspberry Pi check out this awesome simiulator which integrated with Azure IoT Service in real time. [Raspberry Pi Azure IoT Online Simulator](https://azure-samples.github.io/raspberry-pi-web-simulator/)

## Setup for Raspberry Pi with .Net Core

Video coming soon

## Debugging The App

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