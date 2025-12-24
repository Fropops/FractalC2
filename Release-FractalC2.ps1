function Update-ProjectVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectPath,

        [ValidateSet("major","minor","patch","build")]
        [string]$IncrementPart = "patch"
    )

    if (-not (Test-Path $ProjectPath)) {
        Write-Error "? Le fichier '$ProjectPath' est introuvable."
        return
    }

    Write-Host "?? Mise à jour de la version pour le projet : $ProjectPath"

    [xml]$xml = Get-Content $ProjectPath
    $updatedCsproj = $false

    $tags = @("Version","AssemblyVersion","FileVersion","InformationalVersion")

    foreach ($tag in $tags) {
		$node = $xml.SelectSingleNode("//Project/PropertyGroup/$tag")
		
		if (-not $node) {
		  Write-Host "? Impossible de trouver $tag"
		  continue;
		}
		
		$currentVersion = $node.InnerText

		$versionParts = $currentVersion -split '\.' | ForEach-Object { [int]$_ }
		while ($versionParts.Count -lt 4) { $versionParts += 0 }

		switch ($IncrementPart) {
			"major" { $versionParts[0]++; $versionParts[1] = 0; $versionParts[2] = 0; $versionParts[3] = 0 }
			"minor" { $versionParts[1]++; $versionParts[2] = 0; $versionParts[3] = 0 }
			"patch" { $versionParts[2]++; $versionParts[3] = 0 }
			"build" { $versionParts[3]++ }
		}

		$newVersion = ($versionParts -join '.')
		Write-Host "? $tag : $currentVersion ? $newVersion"
		
		$node.InnerText = "$newVersion"
		$updatedCsproj = $true
	}

    if ($updatedCsproj) {
        $xml.Save($ProjectPath)
        Write-Host "?? Version mise à jour dans le .csproj"
    }

    # AssemblyInfo.cs
    $projectDir = Split-Path $ProjectPath
    $assemblyInfo = Get-ChildItem -Path $projectDir -Recurse -Filter "AssemblyInfo.cs" | Select-Object -First 1

    if (-not $assemblyInfo) {
        Write-Warning "?? Aucun fichier AssemblyInfo.cs trouvé."
        return
    }

    Write-Host "?? Fichier AssemblyInfo détecté : $($assemblyInfo.FullName)"
    $content = Get-Content $assemblyInfo.FullName
    $newContent = @()
    $updatedAsm = $false

    $regex = '\[assembly:\s*(AssemblyVersion|AssemblyFileVersion|InformationalVersion)\("([\d\.]+)"\)\]'
    foreach ($line in $content) {
        if ($line -match $regex) {
            $versionParts = $matches[2] -split '\.' | ForEach-Object { [int]$_ }
            while ($versionParts.Count -lt 4) { $versionParts += 0 }

            switch ($IncrementPart) {
                "major" { $versionParts[0]++; $versionParts[1] = 0; $versionParts[2] = 0; $versionParts[3] = 0 }
                "minor" { $versionParts[1]++; $versionParts[2] = 0; $versionParts[3] = 0 }
                "patch" { $versionParts[2]++; $versionParts[3] = 0 }
                "build" { $versionParts[3]++ }
            }

            $newVersion = ($versionParts -join '.')
            $newline = "[assembly: $($matches[1])(`"$newVersion`")]"
            Write-Host "? Mise à jour $($matches[1]): $($matches[2]) ? $newVersion"
            $newContent += $newline
            $updatedAsm = $true
        } else {
            $newContent += $line
        }
    }

    if ($updatedAsm) {
        Set-Content -Path $assemblyInfo.FullName -Value $newContent -Encoding UTF8
        Write-Host "?? Version mise à jour dans AssemblyInfo.cs"
    } elseif (-not $updatedCsproj) {
        Write-Host "?? Aucune version trouvée à mettre à jour."
    }
}


function Release-FractalC2 {
    param(
        [ValidateSet("All","TeamServer","Commander","Agent", "DebugAgent", "WebCommander")]
        [string]$Target = "All",
        [ValidateSet("major","minor","patch","revision")]
        [string]$IncrementPart = "patch"
    )
	
	clear

    $msbuild = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
    $buildDir = "E:\Share\Projects\FractalC2\tmpbuild"
    $baseDir = "E:\Share\Projects\FractalC2\"

    # Créer dossier temporaire
    if (-not (Test-Path $buildDir)) { New-Item -ItemType Directory -Path $buildDir | Out-Null }

	# Dossiers de source
	$scriptDir    = "$baseDir\Payload\Scripts"
	$peDir    = "$baseDir\Payload\PE"
	
    # Dossiers de destination
    $destx86Dir   = "$baseDir\PayloadTemplates\x86"
    $destx64Dir   = "$baseDir\PayloadTemplates\x64"
    $destdebugDir = "$baseDir\PayloadTemplates\debug"
    

    $dirs = @($destx86Dir, $destx64Dir, $destdebugDir)
    foreach ($d in $dirs) { if (-not (Test-Path $d)) { New-Item -ItemType Directory -Path $d | Out-Null } }

    # Fonction pour builder et copier un projet
    function Build-And-Copy {
        param (
            [string]$proj,
            [string]$outputName,
            [string[]]$platforms = @("x86", "x64", "ReleaseButDebug"),
            [string[]]$destDirs
        )

        foreach ($i in 0..($platforms.Count-1)) {
			$platform = $platforms[$i]
			$destdir = $destDirs[$i]
            $cfg = if ($platform  -eq "ReleaseButDebug") { "ReleaseButDebug" } else { "release" }
			$platform = if ($platform  -eq "x86") { "x86" } else { "x64" }
			Write-Host "Buld Command : $msbuild $proj /p:configuration=$cfg /p:platform=$platform  /p:outputpath=$buildDir"
            & $msbuild $proj /p:configuration=$cfg /p:platform=$platform  /p:outputpath=$buildDir
			#Write-Host  "After build"
            $outputFile = Join-Path $buildDir $outputName
			#Write-Host  "OutFiles : $outputFile $destdir"
            Copy-Item $outputFile $destdir -Force
            Remove-Item "$buildDir\*" -Force -Recurse
        }
    }
	

    # Fonction pour créer un zip par cible
    function Zip-Target {
        param (
            [string]$targetName,
            [string]$sourceDir
        )
        $zipPath = "$baseDir\Install\$targetName.zip"
        if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
        Compress-Archive -Path "$sourceDir\*" -DestinationPath $zipPath -Force
        Write-Host "ZIP créé : $zipPath"
    }
	
	# --- Partie Debug Agent ---
    if ($Target -in @("DebugAgent")) {
		$destDirs = @($destx86Dir, $destx64Dir, $destdebugDir)
		$platforms = @("x86","x64","ReleaseButDebug")
	
		Write-Host "Building Linux Agent..."
		dotnet publish $baseDir/AgentLinux/AgentLinux.csproj `
		  -c Debug `
		  -f net8.0 `
		  -r linux-x64 `
		  --self-contained `
		  -p:PublishSingleFile=true `
		  -p:PublishTrimmed=true `
		  -o $destdebugDir
		  
		dotnet publish $baseDir/AgentLinux/AgentLinux.csproj `
		  -c Release `
		  -f net8.0 `
		  -r linux-x64 `
		  --self-contained `
		  -p:PublishSingleFile=true `
		  -p:PublishTrimmed=true `
		  -p:DebugType=none `
		  -p:DebugSymbols=false `
		  -p:StripSymbols=true `
		  -o $destx64Dir
		  
		  
        Write-Host "Building Agent..."
		Build-And-Copy -proj "$baseDir\Agent\Agent.csproj" -outputName "Agent.exe" -platforms $platforms -destDirs $destDirs
        Build-And-Copy -proj "$baseDir\Agent\Agent.csproj" -outputName "Agent.exe" -platforms $platforms -destDirs $destDirs
		Build-And-Copy -proj "$baseDir\Payload\PatcherDll\PatcherDll.csproj" -outputName "Patcher.dll" -platforms $platforms -destDirs $destDirs
		Build-And-Copy -proj "$baseDir\Payload\InjectDll\InjectDll.csproj" -outputName "Inject.dll" -platforms $platforms -destDirs $destDirs
		Build-And-Copy -proj "$baseDir\Payload\Starter\Starter.csproj" -outputName "Starter.exe" -platforms $platforms -destDirs $destDirs
		Build-And-Copy -proj "$baseDir\Payload\Service\Service.csproj" -outputName "Service.exe" -platforms $platforms -destDirs $destDirs
		
		$destDirs = @($destx86Dir, $destx64Dir, $destdebugDir)
		foreach ($dest in $destDirs) {
			Copy-Item "$scriptDir\*.ps1" $dest -Recurse -Force
			Copy-Item "$scriptDir\*.py" $dest -Recurse -Force
		}
		Copy-Item "$peDir\debug\*" $destdebugDir -Recurse -Force
		Copy-Item "$peDir\release\x64\*" $destx64Dir -Recurse -Force
		Copy-Item "$peDir\release\x86\*" $destx86Dir -Recurse -Force
    }

    # --- Partie Agent ---
    if ($Target -in @("All","Agent")) {
		$destDirs = @($destx86Dir, $destx64Dir, $destdebugDir)
		$platforms = @("x86","x64","ReleaseButDebug")
	
        Write-Host "Mise à jour de la version de l'Agent Linux..."
        Update-ProjectVersion -ProjectPath "$baseDir\AgentLinux\AgentLinux.csproj" -IncrementPart $IncrementPart

		Write-Host "Building Linux Agent..."
		dotnet publish $baseDir/AgentLinux/AgentLinux.csproj `
		  -c Debug `
		  -f net8.0 `
		  -r linux-x64 `
		  --self-contained `
		  -p:PublishSingleFile=true `
		  -p:PublishTrimmed=true `
		  -o $destdebugDir
		  
		dotnet publish $baseDir/AgentLinux/AgentLinux.csproj `
		  -c Release `
		  -f net8.0 `
		  -r linux-x64 `
		  --self-contained `
		  -p:PublishSingleFile=true `
		  -p:PublishTrimmed=true `
		  -p:DebugType=none `
		  -p:DebugSymbols=false `
		  -p:StripSymbols=true `
		  -o $destx64Dir
		  
		Write-Host "Mise à jour de la version de l'Agent..."
        Update-ProjectVersion -ProjectPath "$baseDir\Agent\Agent.csproj" -IncrementPart $IncrementPart
		
        Write-Host "Building Agent..."
        Build-And-Copy -proj "$baseDir\Agent\Agent.csproj" -outputName "Agent.exe" -platforms $platforms -destDirs $destDirs
		Build-And-Copy -proj "$baseDir\Payload\PatcherDll\PatcherDll.csproj" -outputName "Patcher.dll" -platforms $platforms -destDirs $destDirs
		Build-And-Copy -proj "$baseDir\Payload\InjectDll\InjectDll.csproj" -outputName "Inject.dll" -platforms $platforms -destDirs $destDirs
		Build-And-Copy -proj "$baseDir\Payload\Starter\Starter.csproj" -outputName "Starter.exe" -platforms $platforms -destDirs $destDirs
		Build-And-Copy -proj "$baseDir\Payload\Service\Service.csproj" -outputName "Service.exe" -platforms $platforms -destDirs $destDirs
		
		$destDirs = @($destx86Dir, $destx64Dir, $destdebugDir)
		foreach ($dest in $destDirs) {
			Copy-Item "$scriptDir\*" $dest -Recurse -Force
			Copy-Item "$scriptDir\*.py" $dest -Recurse -Force
		}
		
		Copy-Item "$peDir\debug\*" $destdebugDir -Recurse -Force
		Copy-Item "$peDir\release\x64\*" $destx64Dir -Recurse -Force
		Copy-Item "$peDir\release\x86\*" $destx86Dir -Recurse -Force
		
        Zip-Target -targetName "Agent" -sourceDir "$baseDir\PayloadTemplates"
    }

    # --- Partie TeamServer ---
    if ($Target -in @("All","TeamServer")) {
        Write-Host "Mise à jour de la version de TeamServer..."
        Update-ProjectVersion -ProjectPath "$baseDir\TeamServer\TeamServer.csproj" -IncrementPart $IncrementPart

        Write-Host "Building TeamServer..."
        Remove-Item "$baseDir\Release\TeamServer" -Force -Recurse -ErrorAction SilentlyContinue
        dotnet publish "$baseDir\TeamServer\TeamServer.csproj" `
            -c Release `
            -r linux-x64 `
            --self-contained false `
            /p:Platform="Any CPU" `
            /p:DeleteExistingFiles=true `
            /p:ExcludeApp_Data=false `
            /p:WebPublishMethod=FileSystem `
            /p:PublishProvider=FileSystem `
            /p:PublishDir="$baseDir\Release\TeamServer" `
            /p:TargetFramework=net8.0

        Remove-Item "$baseDir\Release\TeamServer\appsettings.*" -Force -ErrorAction SilentlyContinue
        Copy-Item "$baseDir\TeamServer\appsettings.release.json" "$baseDir\Release\TeamServer\appsettings.json" -Force
        Zip-Target -targetName "TeamServer" -sourceDir "$baseDir\Release\TeamServer"
    }

    # --- Partie Commander ---
    if ($Target -in @("All","Commander")) {
        Write-Host "Mise à jour de la version de Commander..."
        Update-ProjectVersion -ProjectPath "$baseDir\Commander\Commander.csproj" -IncrementPart $IncrementPart

        Write-Host "Building Commander..."
        Remove-Item "$baseDir\Release\Commander" -Force -Recurse -ErrorAction SilentlyContinue
        dotnet publish "$baseDir\Commander\Commander.csproj" `
            -c Release `
            -r linux-x64 `
            --self-contained false `
            /p:Platform="Any CPU" `
            /p:PublishProtocol=FileSystem `
            /p:PublishProvider=FileSystem `
            /p:PublishDir="$baseDir\Release\Commander" `
            /p:TargetFramework=net8.0 `
            /p:PublishSingleFile=false `
            /p:PublishTrimmed=false

        Remove-Item "$baseDir\Release\Commander\appsettings.*" -Force -ErrorAction SilentlyContinue
        Copy-Item "$baseDir\Commander\appsettings.release.json" "$baseDir\Release\Commander\appsettings.json" -Force
        Zip-Target -targetName "Commander" -sourceDir "$baseDir\Release\Commander"
    }
	
	# --- Partie Commander ---
    if ($Target -in @("All","WebCommander")) {
        Write-Host "Mise à jour de la version de WebCommander..."
        Update-ProjectVersion -ProjectPath "$baseDir\WebCommander\WebCommander.csproj" -IncrementPart $IncrementPart

        Write-Host "Building WebCommander..."
        Remove-Item "$baseDir\Release\WebCommander" -Force -Recurse -ErrorAction SilentlyContinue
        dotnet publish "$baseDir\WebCommander\WebCommander.csproj" `
            -c Release `
            -r linux-x64 `
            --self-contained false `
            /p:Platform="Any CPU" `
            /p:PublishProtocol=FileSystem `
            /p:PublishProvider=FileSystem `
            /p:PublishDir="$baseDir\Release\WebCommander" `
            /p:TargetFramework=net8.0 `
            /p:PublishSingleFile=false `
            /p:PublishTrimmed=false
			
		Write-Host "Building WebCommanderHost..."
		Remove-Item "$baseDir\Release\WebCommanderHost" -Force -Recurse -ErrorAction SilentlyContinue
        dotnet publish "$baseDir\WebCommanderHost\WebCommanderHost.csproj" `
            -c Release `
            -r linux-x64 `
            --self-contained false `
            /p:Platform="Any CPU" `
            /p:DeleteExistingFiles=true `
            /p:ExcludeApp_Data=false `
            /p:WebPublishMethod=FileSystem `
            /p:PublishProvider=FileSystem `
            /p:PublishDir="$baseDir\Release\WebCommanderHost" `
            /p:TargetFramework=net8.0

		Copy-Item "$baseDir\Release\WebCommander\*" "$baseDir\Release\WebCommanderHost\" -Force -Recurse
        Zip-Target -targetName "WebCommander" -sourceDir "$baseDir\Release\WebCommanderHost"
    }

	if (Test-Path $buildDir) {
		Remove-Item $buildDir -Recurse -Force
	}
	if (Test-Path $baseDir\Release) {
		Remove-Item $baseDir\Release -Recurse -Force
	}

    Write-Host "Build terminé pour $Target."
}
