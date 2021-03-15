#!/bin/bash

function install(){
    echo "Copying files"
    
    mkdir -p "$1"
    \cp "./dist/HddFancontrol.ConsoleApp" "$1/HddFancontrol"
    cp -n "./dist/appsettings.json" "./dist/pwm.settings.json" "$1"
    ln -s "$1/HddFancontrol" "/usr/local/bin/hddfancontrol"
    
    echo "Copy is completed to $1"
}


function usage() {
    echo "Usage: install.sh [OPTIONS]"
    echo "Options:"
    echo "      -s, --service-name NAME    Set name of systemd service if applicable (if set the script will try to stop the service and restart after finish)"
    echo "      -p, --install-path PATH    Set install path (default: /usr/local/lib/hdd-fancontrol/)"
}

for arg in "$@"; do
    case "$arg" in
        -h|--help)
            usage
            exit
        ;;
    esac
done

args=("$@")

INSTALL_DIR=/usr/local/lib/hdd-fancontrol
for i in "${!args[@]}"; do
    case "${args[$i]}" in
        -p|--install-path)
            if [ -n "${args[$i+1]}" ] && [ "${args[$i+1]:0:1}" != "-" ]; then
                INSTALL_DIR=${args[$i+1]}
                unset "args[$i]"
                unset "args[$i+1]"
            else
                echo "Error: Argument for ${args[$i]} is missing, please add name of service" >&2
                exit 1
            fi
        ;;
    esac
done

for i in "${!args[@]}"; do
    case "${args[$i]}" in
        -s|--service-name)
            if [ -n "${args[$i+1]}" ] && [ "${args[$i+1]:0:1}" != "-" ]; then
                SERVICE_NAME=${args[$i+1]}
                unset "args[$i]"
                unset "args[$i+1]"
                echo "Installing HddFancontrol"
                
                echo "Stoppting service $SERVICE_NAME"
                service "$SERVICE_NAME" stop
                install "$INSTALL_DIR"
                
                echo "Restarting service $SERVICE_NAME"
                service "$SERVICE_NAME" start
                
                exit
            else
                echo "Error: Argument for ${args[$i]} is missing, please add name of service" >&2
                exit 1
            fi
        ;;
    esac
done

echo "Installing HddFancontrol"
install "$INSTALL_DIR"
