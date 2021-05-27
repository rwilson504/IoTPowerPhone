using CommandLine;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Provisioning.Client.PlugAndPlay;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PowerPhone
{
    class Program
    {
        // DTDL interface is located in the Model folder.  Import this model into the IoT Central templates area before
        // running this program.
        // The PowerPhone model contains a ringer components that has a ring command which will ring the phone.
        private const string ModelId = "dtmi:com:example:PowerPhone;2";
        private const string IOTHUB_DEVICE_DPS_ENDPOINT = "global.azure-devices-provisioning.net";
        private const string IOTHUB_DEVICE_DPS_ID_SCOPE = "YOUR_ID_SCOPE";
        private const string IOTHUB_DEVICE_DPS_DEVICE_ID = "YOUR_DEVICE_ID";
        private const string IOTHUB_DEVICE_DPS_DEVICE_KEY = "YOUR_DEVICE_KEY";

        private static ILogger s_logger;

        public static async Task Main(string[] args)
        {
            //Parse application parameters
            Parameters parameters = null;
            ParserResult<Parameters> result = Parser.Default.ParseArguments<Parameters>(args)
                .WithParsed(parsedParams =>
                {
                    parameters = parsedParams;
                })
                .WithNotParsed(errors =>
                {
                    Environment.Exit(1);
                });

            s_logger = InitializeConsoleDebugLogger();
            if (!parameters.Validate(s_logger))
            {
                throw new ArgumentException("Required parameters are not set. Please recheck required variables by using \"--help\"");
            }

            var runningTime = parameters.ApplicationRunningTime != null
                ? TimeSpan.FromSeconds((double)parameters.ApplicationRunningTime)
                : Timeout.InfiniteTimeSpan;

            s_logger.LogInformation("Press Control+C to quit the sample.");
            using var cts = new CancellationTokenSource(runningTime);
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cts.Cancel();
                s_logger.LogInformation("Sample execution cancellation requested; will exit.");
            };

            s_logger.LogDebug($"Set up the device client.");
            using DeviceClient deviceClient = await SetupDeviceClientAsync(parameters, cts.Token);
            var sample = new PowerPhone(deviceClient, s_logger);
            await sample.PerformOperationsAsync(cts.Token);

            // PerformOperationsAsync is designed to run until cancellation has been explicitly requested, either through
            // cancellation token expiration or by Console.CancelKeyPress.
            // As a result, by the time the control reaches the call to close the device client, the cancellation token source would
            // have already had cancellation requested.
            // Hence, if you want to pass a cancellation token to any subsequent calls, a new token needs to be generated.
            // For device client APIs, you can also call them without a cancellation token, which will set a default
            // cancellation timeout of 4 minutes: https://github.com/Azure/azure-iot-sdk-csharp/blob/64f6e9f24371bc40ab3ec7a8b8accbfb537f0fe1/iothub/device/src/InternalClient.cs#L1922
            await deviceClient.CloseAsync();
        }

        private static ILogger InitializeConsoleDebugLogger()
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                .AddFilter(level => level >= LogLevel.Debug)
                .AddConsole(options =>
                {                    
                    options.TimestampFormat = "[MM/dd/yyyy HH:mm:ss]";
                });
            });

            return loggerFactory.CreateLogger<PowerPhone>();
        }

        private static async Task<DeviceClient> SetupDeviceClientAsync(Parameters parameters, CancellationToken cancellationToken)
        {
            DeviceClient deviceClient;
            switch (parameters.DeviceSecurityType.ToLowerInvariant())
            {
                case "dps":
                    s_logger.LogDebug($"Initializing via DPS");
                    DeviceRegistrationResult dpsRegistrationResult = await ProvisionDeviceAsync(parameters, cancellationToken);
                    var authMethod = new DeviceAuthenticationWithRegistrySymmetricKey(dpsRegistrationResult.DeviceId, parameters.DeviceSymmetricKey);
                    deviceClient = InitializeDeviceClient(dpsRegistrationResult.AssignedHub, authMethod);
                    break;

                case "connectionstring":
                    s_logger.LogDebug($"Initializing via IoT Hub connection string");
                    deviceClient = InitializeDeviceClient(parameters.PrimaryConnectionString);
                    break;

                default:
                    throw new ArgumentException($"Unrecognized value for device provisioning received: {parameters.DeviceSecurityType}." +
                        $" It should be either \"dps\" or \"connectionString\" (case-insensitive).");
            }
            return deviceClient;
        }

        // Provision a device via DPS, by sending the PnP model Id as DPS payload.
        private static async Task<DeviceRegistrationResult> ProvisionDeviceAsync(Parameters parameters, CancellationToken cancellationToken)
        {
            SecurityProvider symmetricKeyProvider = new SecurityProviderSymmetricKey(parameters.DeviceId, parameters.DeviceSymmetricKey, null);
            ProvisioningTransportHandler mqttTransportHandler = new ProvisioningTransportHandlerMqtt();
            ProvisioningDeviceClient pdc = ProvisioningDeviceClient.Create(parameters.DpsEndpoint, parameters.DpsIdScope, symmetricKeyProvider, mqttTransportHandler);

            var pnpPayload = new ProvisioningRegistrationAdditionalData
            {
                JsonData = PnpConvention.CreateDpsPayload(ModelId),
            };
            return await pdc.RegisterAsync(pnpPayload, cancellationToken);
        }

        // Initialize the device client instance using connection string based authentication, over Mqtt protocol (TCP, with fallback over Websocket) and
        // setting the ModelId into ClientOptions.This method also sets a connection status change callback, that will get triggered any time the device's
        // connection status changes.
        private static DeviceClient InitializeDeviceClient(string deviceConnectionString)
        {
            var options = new ClientOptions
            {
                ModelId = ModelId,
            };

            DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt, options);
            deviceClient.SetConnectionStatusChangesHandler((status, reason) =>
            {
                s_logger.LogDebug($"Connection status change registered - status={status}, reason={reason}.");
            });

            return deviceClient;
        }

        // Initialize the device client instance using symmetric key based authentication, over Mqtt protocol (TCP, with fallback over Websocket)
        // and setting the ModelId into ClientOptions. This method also sets a connection status change callback, that will get triggered any time the device's connection status changes.
        private static DeviceClient InitializeDeviceClient(string hostname, IAuthenticationMethod authenticationMethod)
        {
            var options = new ClientOptions
            {
                ModelId = ModelId,
            };

            DeviceClient deviceClient = DeviceClient.Create(hostname, authenticationMethod, TransportType.Mqtt, options);
            deviceClient.SetConnectionStatusChangesHandler((status, reason) =>
            {
                s_logger.LogDebug($"Connection status change registered - status={status}, reason={reason}.");
            });

            return deviceClient;
        }
    }
}
