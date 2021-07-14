$pythonVersion = &{python -V} 2>&1

if ($pythonVersion -Match "Python 3.9*") {
  echo "$pythonVersion found!"
} else {
  echo "$pythonVersion is invalid! Use 3.9 or greater."
  exit 1
}

echo "Clonning ml-agents..."
git clone https://github.com/Unity-Technologies/ml-agents.git Vendor/ml-agents
git checkout release_18

echo "Setting up environment..."
python -m pip install virtualenv
virtualenv -p python3.9 venv
.\venv\Scripts\Activate.ps1
python -m pip install --upgrade pip
python -m pip install torch==1.7.1 -f https://download.pytorch.org/whl/torch_stable.html
python -m pip install -r requirements.txt
deactivate
echo "Setup complete."
