const fs = require('fs');
const { exec } = require("child_process");

(async () => {
    if(fs.existsSync('./Homo.Api')){
        fs.rmdirSync('./Homo.Api', {
            recursive: true
        });
    };
    await installHomoApi();
    fs.rmdirSync('./Homo.Api/Properties',{ recursive: true });
    fs.rmdirSync('./Homo.Api/Resources',{ recursive: true });
    fs.rmdirSync('./Homo.Api/Localization/Common',{ recursive: true });
    fs.rmdirSync('./Homo.Api/Localization/Error',{ recursive: true });
    fs.rmdirSync('./Homo.Api/Localization/Validation',{ recursive: true });
    fs.unlinkSync('./Homo.Api/api.csproj');
    fs.unlinkSync('./Homo.Api/appsettings.dev.json');
    fs.unlinkSync('./Homo.Api/appsettings.json');
    fs.unlinkSync('./Homo.Api/appsettings.staging.json');
    fs.unlinkSync('./Homo.Api/appsettings.prod.json');
    fs.unlinkSync('./Homo.Api/package-lock.json');
    fs.unlinkSync('./Homo.Api/Program.cs');
    fs.unlinkSync('./Homo.Api/Startup.cs');
})();

function installHomoApi(){
    return new Promise((resolve, reject)=>{
        exec('dotnet new -u Homo.Api && dotnet nuget locals all --clear && dotnet new -i Homo.Api::5.0.5-alpha.1 && dotnet new homo-api -o Homo.Api && dotnet restore', (error, stdout, stderr) => {
            if (error) {
                throw error;
            }
            if (stderr) {
                throw error;
            }
            resolve();
            console.log(`stdout: ${stdout}`);
        });
    })
}
