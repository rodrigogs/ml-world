# #!/bin/bash
$VENV_PATH=".venv"
$PYTHON_VERSION="3.9"

echo "Clonning ml-agents..."
git clone https://github.com/Unity-Technologies/ml-agents.git Vendor/ml-agents
cd Vendor/ml-agents
git checkout release_21
cd ..\..

echo "Setting up environment..."
python -m pip install virtualenv
python -m virtualenv -p python$($PYTHON_VERSION) $($VENV_PATH)
.".\$($VENV_PATH)\Scripts\Activate.ps1"

if (-not (python --version).StartsWith("Python $PYTHON_VERSION")) {
    Write-Host "Python version($(python --version)) is not correct. Please check the version and try again."
    exit 1
}

python -m pip install --upgrade pip
python -m pip install -r requirements-win.txt

deactivate
echo "Setup complete."
