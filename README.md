# Skillcade Multiplayer SDK

Made for creating different multiplayer games with similar architecture.

It's a baseline for different netcode frameworks. This package doesn't have any bootstrapping or scene setup - look for specific framework-related extension of this package.

You can extend this package by your netcode framework.

## Extensions

Here are list of existing framework-related extensions:

1 - FishNet: https://github.com/Skillcade/SkillcadeSDK-FishNet

## Installation

### Install VContainer:

Just follow any preferred way in instructions: https://vcontainer.hadashikick.jp/getting-started/installation

### Install Amazon GameLift SDK

1 - Create new Scoped Registry in Package Manager.
  
    Name: Unity NuGeT
    URL: https://unitynuget-registry.openupm.com
    Scope: org.nuget
    
2 - Download file com.amazonaws.gameliftserver.sdk-5.4.0.tgz from the root of this repository

3 - Open Package Manager inside your project and click 'install package from tarball'

4 - Select downloaded .tgz archive

### Install Skillcade SKD

1 - Open Package Manager inside your project and click 'install package from git URL'

2 - Put https://github.com/Skillcade/SkillcadeSDK.git?path=Assets/Source/SkillcadeSDK into input field and press OK
