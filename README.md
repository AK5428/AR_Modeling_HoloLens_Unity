# AR空中实时建模系统
一个基于HoloLens的AR空中实时建模系统，由手势进行控制，可支持多人协作建模。使用Unity进行开发。以下按照双人系统进行说明。

# 硬件架构
![硬件架构](https://github.com/AK5428/AR_Modeling_HoloLens_Unity/blob/main/Describe/Fig.5.%20System%20implementation..pdf)
如图所示，该系统的硬件由两台HoloLens及两台装有Rhino本地服务器和Unity的电脑组成，每台HoloLens由一台对应的电脑进行控制。电脑与HoloLens之间通过Holographic Remoting实现无线同步。

# 主要功能说明
本系统包含三块主要的功能。
1. HoloLens的开发与控制。使用微软的MRTK包实现，包含用户的手势识别、空中绘制、对按钮的响应以及对物体的操控；
2. 模型的创建与显示。使用Rhino中的compute-rhino3d库实现，包括基于点、线的模型生成，以及模型到Unity中mesh的转换；
3. 多人视野同步。通过Photon实现多人系统中数据的同步，通过Azure Spatial Anchor实现虚拟物品在真实空间下的位置同步。
