# HDD Temperature Fancontrol

This is a utility tool written in C# for linux (similar to lm-sensor's fancontrol) that sets the speed of the available fans on a system based on the highest HDD temperature. Currently internally this uses `hddtemp` to get the HDD temperatures, so this utility app needs to be installed.

This utility may be run as a systemd service with auto startup at boot.

## Installing
**Compiling**

1) Clone repository
2) Open the workspace in VSCode and run publish task, which will create a single executable in the dist folder
3) Edit pwm.settings.json file with the required settings (for more info see [Settings](##Settings))

**Systemd setup**

4) Create a systemd unit file with
```apache
[Service]
Type=notify
NotifyAccess=all
WorkingDirectory=/path/to/the/settings/directory/
ExecStart=/path/to/the/executable
```
5) Add other setup to the unit file as required and copy it to the `/etc/systemd/system/` folder
6) Reload systemd (`systemctl daemon-reload`)

**Install script**

Install script may be used to copy the necessary files to a specific location from the `./dist/` folder. Install script has two options please see `./install.sh --help` for usage.

## Settings

All settings should be included in the pwm.settings.json. The pwm.settings.json file should be in the same directory as the executable.

The appsettings.json file includes logging related settings, file is optional, the default logging folder is `Logs` in the working directory, with a logging level of warning, formatted by the [compact json formatter](https://github.com/serilog/serilog-formatting-compact) of Serilog. Since logging uses Serilog most of the logging settings (e.g. output templates) are available as per the Serilog docs.

***pwm.settings.json***

```json
{
    "generalSettings": {
        "interval": 10, // refresh interval in seconds
        "devPath": "/sys/class/hwmon/hwmon1/" // path to the sysfs hardware monitor folder
    },
    "pwmSettings": [ // an array where the length of the array corresponds to the number of fans to control
        {
            "minTemp": 29, // minimum temperature (in C°) where fan should start at minStart pwm
            "maxTemp": 43, // temperature (in C°) where max pwm is set
            "minStart": 48, // minimum pwm to start with at minimum temperature
            "minPwm": 0, // minimum pwm when temperature is below minTemp
            "maxPwm": 255 // maximum pwm when temperature is at maxTemp
        },
        { // second fan settings (e.g. chassis fan or CPU fan if settable separately)
            "minTemp": 29,
            "maxTemp": 43,
            "minStart": 30,
            "minPwm": 0,
            "maxPwm": 255
        }
    ]
}
```

***appsettings.json***

```json
{
    "Serilog": {
        "MinimumLevel": {
            "Default": "Warning"
        },
        "LogDirectory": "/var/log/"
    }
}
```