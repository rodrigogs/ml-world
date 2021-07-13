# ml-world

Base Unity project ready to use with [ml-agents](https://github.com/Unity-Technologies/ml-agents).

## Instalation
```ps1
./Scripts/setup.ps1
```

## Usage
### First initialize the virtualenv
```ps1
./Scripts/dev.ps1
```
### Then you can train your agents
```ps1
mlagents-learn ./Assets/Configs/sample_config.yml --run-id MyBehavior
```
### Or improve existent ones
```ps1
mlagents-learn ./Assets/Configs/sample_config.yml --initialize-from MyBehavior1 --run-id MyBehavior2
```
