#!/bin/bash

# Usage: ./install.sh [All|TeamServer|WebCommander|Commander]
INSTALL_PART=${1:-All}  # Par défaut tout installer
NO_RUN=${2:-""}   # Si "noRun", ne pas lancer le TeamServer à la fin

BASE_DIR="$PWD/FractalC2"
mkdir -p "$BASE_DIR" && cd "$BASE_DIR"

# Génération d'une clé API aléatoire de 64 caractères
USER_API_KEY=$(openssl rand -base64 48 | tr '+/' '-_' | cut -c1-64)
SERVER_KEY=$(openssl rand -base64 32)

# Fonction pour télécharger et dézipper
install_part() {
    local name=$1
    local zip_url=$2

    echo "Installing $name..."
    curl -L "$zip_url" -o "${name}.zip"
    unzip -o "${name}.zip" -d "$BASE_DIR/$name"
    rm "${name}.zip"
}

install_TeamServer() {
	# Installer dotnet runtime si nécessaire
	sudo apt-get update
	sudo apt-get install -y aspnetcore-runtime-8.0
	
	
	install_part "TeamServer" "https://github.com/Fropops/FractalC2/raw/refs/heads/main/Install/TeamServer.zip"
    chmod +x "$BASE_DIR/TeamServer/TeamServer"
	
	#install python
	python3 -m venv pyenv
	pyenv/bin/pip install lief
	
	# Mise à jour du appsettings.json
	TEAMSERVER_APPSETTINGS="TeamServer/appsettings.json"
	jq --arg key "$USER_API_KEY" '.Users[0].Key = $key' "$TEAMSERVER_APPSETTINGS" > "$TEAMSERVER_APPSETTINGS.tmp"
	jq --arg key "$SERVER_KEY" '.ServerKey = $key' "$TEAMSERVER_APPSETTINGS.tmp" > "$TEAMSERVER_APPSETTINGS.tmp2"
	cp "$TEAMSERVER_APPSETTINGS.tmp2" "$TEAMSERVER_APPSETTINGS"
	rm "$TEAMSERVER_APPSETTINGS.tmp"
	rm "$TEAMSERVER_APPSETTINGS.tmp2"
	
	install_part "PayloadTemplates"  "https://github.com/Fropops/FractalC2/raw/refs/heads/main/Install/Agent.zip"
	
    install_Tools

	# Cloner les outils
	git clone https://github.com/TheWover/donut.git
	cd donut && make && cd ..
}


install_WebCommander() {
	# Installer dotnet runtime si nécessaire
	sudo apt-get update
	sudo apt-get install -y aspnetcore-runtime-8.0
	
	
	install_part "WebCommander" "https://github.com/Fropops/FractalC2/raw/refs/heads/main/Install/WebCommander.zip"
    chmod +x "$BASE_DIR/WebCommander/WebCommanderHost"
}

install_Tools() {
    echo "Installing Tools..."

    TOOLS_DIR="$BASE_DIR/Tools"
    mkdir -p "$TOOLS_DIR"

    # Clone uniquement le dossier Install/Tools du repo (sparse checkout)
    git clone --depth 1 --filter=blob:none \
        --sparse https://github.com/Fropops/FractalC2.git temp_tools_repo

    cd temp_tools_repo
    git sparse-checkout set Install/Tools

    # Copier les fichiers
    cp -r Install/Tools/* "$TOOLS_DIR/"

    cd ..
    rm -rf temp_tools_repo

    echo "Tools installed in $TOOLS_DIR"
}

install_Commander() {
	# Installer dotnet runtime si nécessaire
	sudo apt-get update
	sudo apt-get install -y dotnet-runtime-7.0
	
	
	install_part "Commander"  "https://github.com/Fropops/FractalC2/raw/refs/heads/main/Install/Commander.zip"
	chmod +x "$BASE_DIR/Commander/Commander"

	# Mise à jour du Commander appsettings.json
	COMMANDER_SETTINGS="$BASE_DIR/Commander/appsettings.json"
	jq --arg key "$USER_API_KEY" '.Api.ApiKey = $key' "$COMMANDER_SETTINGS" > "$COMMANDER_SETTINGS.tmp" && mv "$COMMANDER_SETTINGS.tmp" "$COMMANDER_SETTINGS"
}

# Installer selon le choix
case "$INSTALL_PART" in
    All)
        install_TeamServer
		install_WebCommander
        ;;
    TeamServer)
        install_TeamServer
        ;;
    WebCommander)
        install_WebCommander
		;;
	Commander)
        install_Commander
        ;;
    *)
        echo "Invalid option: $INSTALL_PART"
        echo "Usage: $0 [All|TeamServer|Commander]"
        exit 1
        ;;
esac


run_TeamServerCommander() {
	cd "$BASE_DIR/TeamServer"
	sudo ./TeamServer &
	echo "TeamServer started."
}

run_WebCommander() {
	cd "$BASE_DIR/TeamServer"
	sudo ./TeamServer &
	echo "TeamServer started."
}

# Lancer TeamServer si présent et si noRun n'est pas précisé
if [[ "$NO_RUN" != "noRun" ]]; then
	case "$INSTALL_PART" in
		All)
			run_TeamServer
			run_WebCommander
			;;
		TeamServer)
			run_TeamServer
			;;
		WebCommander)
			run_WebCommander
			;;
		Commander)
			;;
		*)
		echo "Invalid option: $INSTALL_PART"
		echo "Usage: $0 [All|TeamServer|Commander]"
		exit 1
		;;
	esac
else
    echo "Skipping run (noRun flag detected)"
fi

show_WebCommander() {
	echo -e "\e[32m[?]\e[0m Web Commander"
	echo -e "\e[36m[*]\e[0m Running at http://127.0.0.1:5001"
}

show_TeamServer() {
	echo -e "\e[32m[?]\e[0m Team Server"
	echo -e "\e[36m[*]\e[0m Running at http://127.0.0.1:5000"
	echo -e "\e[36m[*]\e[0m User : Admin"
	echo -e "\e[36m[*]\e[0m API Key : $USER_API_KEY"
}

case "$INSTALL_PART" in
	All)
		show_TeamServer
		show_WebCommander
		;;
	TeamServer)
		show_TeamServer
		;;
	WebCommander)
		show_WebCommander
		;;
	Commander)
		;;
	*)
	echo "Invalid option: $INSTALL_PART"
	echo "Usage: $0 [All|TeamServer|Commander]"
	exit 1
	;;
esac

echo "Installation completed."




