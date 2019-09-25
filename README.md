# Grid-Anchor-based-Image-Cropping-Pytorch
This repository includes several extensions we have made to our conference version. Please read the [paper](https://drive.google.com/open?id=1Bd1VaqYVycB7Npv5OdXKl-znKs_APl4n) for details.

Some codes, including roi align and rod align, are written as PyTorch extensions in C++ with or without CUDA. We notice that PyTorch API changes a lot after v1.0 so that our official implementation is NOT fully compatible with PyTorch 1.0 or later version. In this repository, we upgrade the interface to make our code smooth again. 

Now, we offer prebuilt bundles compatible with PyTorch 1.2 for both Python2.7 and Python3.5 users.

### Official PyTorch implementation
1. [PyTorch 0.4.1 + Python 2.7](https://github.com/HuiZeng/Grid-Anchor-based-Image-Cropping-Pytorch)
2. [Matlab (conference version)](https://github.com/HuiZeng/Grid-Anchor-based-Image-Cropping)

### Requirements
PyTorch 1.0 or later, numpy, cv2, scipy. 

### Usage
1. Download the source code, the [dataset](https://drive.google.com/open?id=1X9xK5O9cx4_MvDkWAs5wVuM-mPWINaqa) and the [pretrained model](https://drive.google.com/open?id=1kaNWvfIdtbh2GIPNSWXdxqyS-d2DR1F3).

2. Make sure your device is CUDA enabled. Build and install source code of roi_align_api and rod_align_api.

3. Run ``TrainModel.py`` to train a new model on our dataset or Run ``demo_eval.py`` to test the pretrained model on any images.

### Notes of compilation
1. For Python3 users, before you start to build the source code and install the packages, please specify the architecture of your GPU card and CUDA_HOME path in both /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/roi_align/make.sh and /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/rod_align/make.sh

For Python2 users, before you start to build the source code and install the packages, please specify the architecture of your GPU card and CUDA_HOME path in both /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/roi_align/make_python2.sh and /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/rod_align/make_python2.sh

2. Change your directory to the root of this folder,

cd /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/

For Python3 users, please build and install by running 

sudo bash make_all.sh

For Python2 users, please build and install by running

sudo bash make_all_python2.sh

3. Test environment: A spare workstation with dual NV TITAN RTX GPU cards, Intel Core i9-7900X, DDR4 2400MHz 16Gx6 Memory, Samsung 970 EVO 1T nvme m.2 SSD, 1200W PSU; Ubuntu 16.04, GCC 5.4.0, Python 2.7&3.5, NV CUDA-10.0 (with CUDA Patch Version 10.0.130.1), NV cuDNN 7.6 (compatible with CUDA-10.0), and PyTorch 1.2.0.

4. There is no plan to offer pre-built bundles across various environments due to our limited time. You can download the pre-built bundles in the test environment at /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/prebuilt_bundles for evaluation.

### Annotation software
We release binary executable annotation software at /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/annotation. The software is developed and is only available on Windows with dot Net 4.5 and WPF. At the moment, there is no plan to release the ENTIRE source code of the software. Luckily, the key code are released in $root/annotation/ref_code. Before you start, please kindly read the notes below.

1. We only support image files with the following extensions, including jpg, jpeg, png, and tiff. Please put all your images at /root/to/Grid-Anchor-based-Image-Cropping-Pytorch/annotation/images.

2. Double click ImageCrop2.exe to start the annotation software. Click the checkbox named 1-5 to give a cropped image a mark. Toggle your mouse wheel to go to the previous/next page. At each page, at most 4 cropped images are presented for marking. They are organized as ImageSource1 (top left), ImageSource2 (bottom left), ImageSource3 (top right) and ImageSource4 (bottom right). Please check $root/annotation/ref_code/MainWindowViewModel.cs to see how we crop an image and group the cropped ones by aspect ratio. 

3. The software makes a "scores" directory and scores of the cropped images are saved with their corresponding image name in $root/annotation/scores.
		
Please note that a negative value, i.e. -1 in a score file means there is no need to mark the cropped image. The scores of cropped images are arranged in the text file according to how they are generated. For details, please check the function called ComputeCategories() in $root/annotation/ref_code/MainWindowViewModel.cs.

4. You can close the software at any time. A text file called "progress.txt" is automatically created at $root/annotation. Once you launch the software again, the scores will be reloaded.

### Other notes
1. This repository is developed to be compatible with PyTorch 1.0+. Please refer to the [official implementation](https://github.com/HuiZeng/Grid-Anchor-based-Image-Cropping-Pytorch) for any performance comparison.
