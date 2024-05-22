# #!/bin/bash

# VENV_PATH=".venv"
# PYTHON_VERSION="3.10.12"
# LOCAL_PYTHON_PATH="${PWD}/.localPython"

# echo "Installing local python from source..."
# wget http://www.python.org/ftp/python/${PYTHON_VERSION}/Python-${PYTHON_VERSION}.tar.xz
# mkdir -p ${LOCAL_PYTHON_PATH}
# tar -xf Python-${PYTHON_VERSION}.tar.xz -C ${LOCAL_PYTHON_PATH} --strip-components=1
# rm -rf Python-${PYTHON_VERSION}.tar.xz
# cd ${LOCAL_PYTHON_PATH}
# ./configure --prefix=${LOCAL_PYTHON_PATH} --enable-optimizations
# make -j 8
# make install
# bin/python setup.py install
# cd ..
# echo "Python installed."

# echo "Clonning ml-agents..."
# git clone https://github.com/Unity-Technologies/ml-agents.git Vendor/ml-agents
# cd Vendor/ml-agents
# git checkout release_21
# cd ../..

# echo "Setting up environment..."
# python -m pip install --user virtualenv
# python -m virtualenv -p $PWD/.localPython/bin/python ${VENV_PATH}
# source ./${VENV_PATH}/bin/activate

# if [[ $(python --version) != "Python ${PYTHON_VERSION}"* ]]; then
#     echo "Python version($(python --version)) is not correct. Please check the version and try again."
#     exit 1
# fi

# python -m pip install --upgrade pip
# python -m pip install -r requirements-macos.txt

# source deactivate
# echo "Setup complete."

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
python -m pip install --upgrade pip
python -m pip install -r requirements-win.txt
deactivate
echo "Setup complete."
