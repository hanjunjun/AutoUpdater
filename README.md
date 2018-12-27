# AutoUpdater
一个自动化更新组件，很容易嵌入自己的程序

这个组件支持超大文件下载，文件下载用的是分段下载具体原理见下图。

Demo 演示：

![Logo](https://github.com/HanJunJun/AutoUpdater/blob/master/Client/AutoUpdater.Client.Test/demo3.png)
![Logo](https://github.com/HanJunJun/AutoUpdater/blob/master/Client/AutoUpdater.Client.Test/demo1.png)
![Logo](https://github.com/HanJunJun/AutoUpdater/blob/master/Client/AutoUpdater.Client.Test/demo2.png)
![Logo](https://github.com/HanJunJun/AutoUpdater/blob/master/Client/AutoUpdater.Client.Test/客户端主动请求服务器文件逻辑流程.png)

# Tips：

    后端技术：
	
      * C# Winform
	  
	  * 多线程编程
	  
	  * 策略模式，反射，面向接口编程
      
      * MsgPack 一个开源库能将数据流byte[]转化成对象
	  
      * TCP协议基础概念

      * TCP封包，粘包，拆包处理