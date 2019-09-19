# Grid-Anchor-based-Image-Cropping-Pytorch
This code includes several extensions we have made to our conference version. Please read the [paper](https://drive.google.com/open?id=1Bd1VaqYVycB7Npv5OdXKl-znKs_APl4n) for details.


### Requirements
python 3.5 or later, pytorch 1.0 or later, numpy, cv2, scipy. 

### Usage
1. Download the source code, the [dataset](https://drive.google.com/open?id=1X9xK5O9cx4_MvDkWAs5wVuM-mPWINaqa) and the [pretrained model](https://drive.google.com/open?id=1kaNWvfIdtbh2GIPNSWXdxqyS-d2DR1F3).

2. Make sure your device is CUDA enabled. Build and install source code of roi_align_api and rod_align_api.

3. Run ``TrainModel.py`` to train a new model on our dataset or Run ``demo_eval.py`` to test the pretrained model on any images.

### Notes of compilation
1. Before you start to build the source code and install the packages, please specify the architecture of your GPU card and CUDA_HOME path in both /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/roi_align/make.sh and /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/rod_align/make.sh

2. Build and install by running ``sudo bash /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/make_all.sh''.

3. Test environment: A spare workstation with dual NV TITAN RTX GPU cards, Intel Core i9-7900X, DDR4 2400MHz 16Gx6 Memory, Samsung 970 EVO 1T nvme m.2 SSD, 1200W PSU; Ubuntu 16.04, GCC 5.4.0, NV CUDA-10.0 (with CUDA Patch Version 10.0.130.1), NV cuDNN 7.6 (compatible with CUDA-10.0), and PyTorch 1.2.0.

4. There is no plan to offer pre-built bundles across various environments due to our limited time. You can download the pre-built bundle in the test environment at /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/prebuilt_bundles for evaluation.

### Other notes
1. This repository is developed to be compatible with Python3 and PyTorch 1.0+. Please refer to the [official implementation](https://github.com/HuiZeng/Grid-Anchor-based-Image-Cropping-Pytorch) for any performance comparison.

### Official PyTorch implementation
1. [PyTorch 0.4.2 + Python 2.7](https://github.com/HuiZeng/Grid-Anchor-based-Image-Cropping-Pytorch).