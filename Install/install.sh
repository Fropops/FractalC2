#!/bin/bash

# Usage: ./install-c3po.sh [All|TeamServer|Commander]
INSTALL_PART=${1:-All}  # Par défaut tout installer
NO_TOOLS=${2:-""}             # Si "noTools", ne pas télécharger Tools.zip
NO_RUN=${3:-""}   # Si "noRun", ne pas lancer le TeamServer à la fin

BASE_DIR="$PWD/C3PO"
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
	sudo apt-get install -y aspnetcore-runtime-7.0
	
	
	install_part "TeamServer" "https://github.com/Fropops/C3PO/raw/refs/heads/master/Install/TeamServer.zip"
    chmod +x "$BASE_DIR/TeamServer/TeamServer"
	
	
	# Mise à jour du appsettings.json
	TEAMSERVER_APPSETTINGS="TeamServer/appsettings.json"
	jq --arg key "$USER_API_KEY" '.Users[0].Key = $key' "$TEAMSERVER_APPSETTINGS" > "$TEAMSERVER_APPSETTINGS.tmp"
	jq --arg key "$SERVER_KEY" '.ServerKey = $key' "$TEAMSERVER_APPSETTINGS.tmp" > "$TEAMSERVER_APPSETTINGS.tmp2"
	cp "$TEAMSERVER_APPSETTINGS.tmp2" "$TEAMSERVER_APPSETTINGS"
	rm "$TEAMSERVER_APPSETTINGS.tmp"
	rm "$TEAMSERVER_APPSETTINGS.tmp2"
}

install_Tools() {
    echo "Installing Tools..."

    TOOLS_DIR="$BASE_DIR/Tools"
    mkdir -p "$TOOLS_DIR"

    # Clone uniquement le dossier Install/Tools du repo (sparse checkout)
    git clone --depth 1 --filter=blob:none \
        --sparse https://github.com/Fropops/C3PO.git temp_tools_repo

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
	
	
	install_part "Commander"  "https://github.com/Fropops/C3PO/raw/refs/heads/master/Install/Commander.zip"
	chmod +x "$BASE_DIR/Commander/Commander"
	
	install_part "PayloadTemplates"  "https://github.com/Fropops/C3PO/raw/refs/heads/master/Install/Agent.zip"
	
	if [[ "$NO_TOOLS" != "noTools" ]]; then
        echo "Installing Tools..."
        install_Tools
    else
        echo "Skipping Tools installation (noTools flag detected)"
    fi
	
	# Installer Rust pour cross-compilation
	curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs -o rust-install.sh
	chmod +x rust-install.sh
	./rust-install.sh -y
	rm rust-install.sh
	~/.cargo/bin/rustup target add i686-pc-windows-gnu
	~/.cargo/bin/rustup target add x86_64-pc-windows-gnu

	# Cloner les outils
	git clone https://github.com/TheWover/donut.git
	cd donut && make && cd ..
	git clone https://github.com/Fropops/incrust.git
	
	# Mise à jour du Commander appsettings.json
	COMMANDER_SETTINGS="$BASE_DIR/Commander/appsettings.json"
	jq --arg key "$USER_API_KEY" '.Api.ApiKey = $key' "$COMMANDER_SETTINGS" > "$COMMANDER_SETTINGS.tmp" && mv "$COMMANDER_SETTINGS.tmp" "$COMMANDER_SETTINGS"
}

# Installer selon le choix
case "$INSTALL_PART" in
    All)
        install_TeamServer
		install_Commander
        ;;
    TeamServer)
        install_TeamServer
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

# Lancer TeamServer si présent et si noRun n'est pas précisé
if [[ "$NO_RUN" != "noRun" ]]; then
    if [ -f "$BASE_DIR/TeamServer/TeamServer" ]; then
        cd "$BASE_DIR/TeamServer"
        sudo ./TeamServer &
        echo "TeamServer started."
    fi
else
    echo "Skipping TeamServer run (noRun flag detected)"
fi


echo "Installation completed."




