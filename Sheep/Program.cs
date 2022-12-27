// See https://aka.ms/new-console-template for more information

using System.Net.NetworkInformation;
using DeviceId;
using Shepherd.Utility.IdGenerator;
using Sheep.Core;

var sheepId = new DeviceIdBuilder()
    .AddMachineName()
    .AddOsVersion()
    .AddUserName()
    .AddMacAddress()
    .ToString();

var sheepController = new SheepController(sheepId);

await sheepController.Start();


while (true)
{
    await sheepController.TryReceiveMessage();
    System.Threading.Thread.Sleep(30000);
}

//await sheepController.End();

