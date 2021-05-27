using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Device.Gpio;
using Iot.Device.Buzzer;

namespace PowerPhone
{
    internal enum StatusCode
    {
        Completed = 200,
        InProgress = 202,
        NotFound = 404,
        BadRequest = 400
    }

    public class PowerPhone
    {        
        private readonly DeviceClient _deviceClient;
        private readonly ILogger _logger;
        
        public PowerPhone(DeviceClient deviceClient, ILogger logger)
        {
            _deviceClient = deviceClient ?? throw new ArgumentNullException($"{nameof(deviceClient)} cannot be null.");
            _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<PowerPhone>();
        }

        public async Task PerformOperationsAsync(CancellationToken cancellationToken)
        {
            // This sample follows the following workflow:            
            // -> Set handler to receive "ring" command - on "Ringer" components.                     

            // For a component-level command, the command name is in the format "<component-name>*<command-name>".
            _logger.LogDebug($"Set handler for \"ring\" command.");
            await _deviceClient.SetMethodHandlerAsync("ringer*ring", HandleRingCommandAsync, "ringer", cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {                
            }                
        }

        // The callback to handle "reboot" command. This method will send a temperature update (of 0°C) over telemetry for both associated components.
        private async Task<MethodResponse> HandleRingCommandAsync(MethodRequest request, object userContext)
        {
            try
            {
                int numberOfRings = JsonConvert.DeserializeObject<int>(request.DataAsJson);

                _logger.LogDebug($"Command: Received - Ringing the phone {numberOfRings} times");                
                using var controller = new GpioController();
                using var buzzer = new Buzzer(14);
                controller.OpenPin(21, PinMode.Output);
                for (int i = 0; i < numberOfRings; i++)
                {
                    buzzer.StartPlaying(440);                   
                    controller.Write(21, PinValue.High);      

                    await Task.Delay(2000);

                    buzzer.StopPlaying();
                    controller.Write(21, PinValue.Low);

                    await Task.Delay(2000);
                }
                                
            }
            catch (JsonReaderException ex)
            {
                _logger.LogDebug($"Command input is invalid: {ex.Message}.");
                return new MethodResponse((int)StatusCode.BadRequest);
            }

            return new MethodResponse((int)StatusCode.Completed);
        }
        
    }
}
