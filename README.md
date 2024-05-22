# ml-world

Base Unity project ready to use with [ml-agents](https://github.com/Unity-Technologies/ml-agents).

## Instalation
#### Clone
```ps1
git clone --recurse-submodules https://github.com/rodrigogs/ml-world.git
```
Powershell
```ps1
./Scripts/setup.ps1
```
Bash
```bash
./Scripts/setup.sh
```

## Usage
#### First initialize the virtualenv
Powershell
```ps1
./.venv/Scripts/Activate.ps1
```
Bash
```bash
source ./.venv/bin/activate
```
#### Then you can train your agents
Powershell
```ps1
mlagents-learn ./Assets/Configs/sample_config.yml --run-id MyBehavior
```
Bash
```bash
mlagents-learn ./Assets/Configs/sample_config.yml --run-id MyBehavior
```
#### Or improve existent ones
Powershell
```ps1
mlagents-learn ./Assets/Configs/sample_config.yml --initialize-from MyBehavior1 --run-id MyBehavior2
```
Bash
```bash
mlagents-learn ./Assets/Configs/sample_config.yml --initialize-from MyBehavior1 --run-id MyBehavior2
```
#### Using tensorboard
```bash
tensorboard --logdir ./results
```
