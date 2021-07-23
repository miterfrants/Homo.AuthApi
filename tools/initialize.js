const fs = require('fs');
const { exec } = require('child_process');
const inquirer = require('inquirer');
const randomstring = require('randomstring');

(async () => {
    await installHomoApi();
    // build secrets.json
    const secrets = JSON.parse(fs.readFileSync('secrets.template.json').toString()).Config.Secrets;
    const answersForSecrets = await promptQuestion(secrets);
    for (const key in answersForSecrets) {
        if (!answersForSecrets[key] && key.endsWith('Key')) {
            answersForSecrets[key] = randomstring.generate(32);
        }
    }
    const config = { Config: { Secrets: answersForSecrets } };
    fs.writeFileSync('./secrets.json', JSON.stringify(config, null, 4));
    // build appsettings.json
    const appsettingsRaw = JSON.parse(fs.readFileSync('appsettings.template.json').toString());
    const common = appsettingsRaw.Config.Common;
    const answersForCommon = await promptQuestion(common);
    for (const key in appsettingsRaw.Config.Common) {
        if (answersForCommon[key]) {
            appsettingsRaw.Config.Common[key] = answersForCommon[key];
        }
    }
    fs.writeFileSync('./appsettings.json', JSON.stringify(appsettingsRaw, null, 4));

    // resotre database from ef
    console.log('resotre database from ef');
    exec('dotnet ef migrations add InitialCreate');
    await new Promise(resolve => setTimeout(resolve, 3000));
    exec('dotnet ef database update');

    // build rsa key for protect sensitive information
    if (!fs.existsSync('./secrets')) {
        fs.mkdirSync('./secrets');
    }
    exec('openssl genrsa -out ./secrets/key.pem 4096');
    exec('openssl rsa -in ./secrets/key.pem -out ./secrets/key.pub -pubout -outform pem');
    exec('openssl rsa -pubin -in ./secrets/key.pub -RSAPublicKey_out -outform dem > ./secrets/key.pub.der');
    exec('openssl rsa -in ./secrets/key.pem -outform dem > ./secrets/key.der');
    console.log('initialize finish');

    // run api server and dev-front-end-server
    // exec('dotnet watch run --launch-profile dev');
    // exec('cd ./tools/dev-front-end-server && npm install && node server.js');
})();

async function promptQuestion (keys) {
    const arrayOfSecrets = [];
    for (const key in keys) {
        arrayOfSecrets.push({
            type: 'input',
            name: key,
            message: `what is ur ${key}`
        });
    }

    return new Promise((resolve, reject) => {
        inquirer.prompt(arrayOfSecrets)
            .then(answers => {
                resolve(answers);
            })
            .catch(error => {
                reject(error);
            });
    });
}

function installHomoApi () {
    if (fs.existsSync('./Homo.Api')) {
        fs.rmSync('./Homo.Api', {
            recursive: true,
            force: true
        });
    };
    return new Promise((resolve, reject) => {
        exec('dotnet new -u Homo.Api && dotnet nuget locals all --clear && dotnet new -i Homo.Api::5.0.5-alpha.2 && dotnet new homo-api -o Homo.Api && dotnet restore', (error, stdout, stderr) => {
            if (error) {
                throw error;
            }
            if (stderr) {
                throw error;
            }
            resolve();
            console.log(`stdout: ${stdout}`);
        });
    }).then(() => {
        fs.rmSync('./Homo.Api/Properties', { recursive: true, force: true });
        fs.rmSync('./Homo.Api/Localization/Common', { recursive: true, force: true });
        fs.rmSync('./Homo.Api/Localization/Error', { recursive: true, force: true });
        fs.rmSync('./Homo.Api/Localization/Validation', { recursive: true, force: true });
        fs.unlinkSync('./Homo.Api/api.csproj');
        fs.unlinkSync('./Homo.Api/appsettings.dev.json');
        fs.unlinkSync('./Homo.Api/appsettings.json');
        fs.unlinkSync('./Homo.Api/appsettings.staging.json');
        fs.unlinkSync('./Homo.Api/appsettings.prod.json');
        fs.unlinkSync('./Homo.Api/package-lock.json');
        fs.unlinkSync('./Homo.Api/Program.cs');
        fs.unlinkSync('./Homo.Api/Startup.cs');
    });
}
