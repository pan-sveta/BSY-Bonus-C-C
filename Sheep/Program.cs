// See https://aka.ms/new-console-template for more information

using System.Net.NetworkInformation;
using DeviceId;
using Sheep.Core;

Console.WriteLine(@"
                  ,-''''-.
                 (.  ,.   L        ___...__
                 /7} ,-`  `'-==''``        ''._
                //{                           '`.
                \_,X ,    BSY                  : )
                    7         Filip Stepanek    ;`
                    :                  ,       /
                     \_,                \     ;
                       Y   L_    __..--':`.    L
                       |  /| ````       ;  y  J
                       [ j J            / / L ;
                       | |Y \          /_J  | |
                       L_J/_)         /_)   L_J
                      /_)               sk /_)
");

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