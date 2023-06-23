#!/bin/bash

echo "Clonning ml-agents..."
git clone https://github.com/Unity-Technologies/ml-agents.git Vendor/ml-agents
cd Vendor/ml-agents
git checkout release_20
cd ../..

echo "Setting up environment..."
python -m pip install virtualenv
virtualenv -p python3.9 venv
source ./venv/bin/activate
python -m pip install --upgrade pip
python -m pip install grpcio --only-binary :all:
python -m pip install torch torchvision torchaudio mlagents mlagents-envs
python -m pip install -r requirements-macos.txt
source deactivate
echo "Setup complete."
