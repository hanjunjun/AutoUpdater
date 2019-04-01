# 觉得还不错的给个*Star，谢谢了
# AutoUpdater
一个自动化更新组件，很容易嵌入自己的程序

这个组件支持超大文件下载，文件下载用的是分段下载具体原理见下图。
这个项目分为两个部分：
1.服务端：服务端载体可以是winform窗体可以注册为Windows服务
2.客户端：客户端是个winform窗体。

# 服务端：

![Logo](https://github.com/HanJunJun/AutoUpdater/blob/master/Client/AutoUpdater.Client.Test/demo3.png)

# 客户端：

![Logo](https://github.com/HanJunJun/AutoUpdater/blob/master/Client/AutoUpdater.Client.Test/demo1.png)
![Logo](https://github.com/HanJunJun/AutoUpdater/blob/master/Client/AutoUpdater.Client.Test/demo2.png)
![Logo](https://github.com/HanJunJun/AutoUpdater/blob/master/Client/AutoUpdater.Client.Test/客户端主动请求服务器文件逻辑流程.png)
```xml
<appSettings>
    <add key="ServerIP" value="127.0.0.1" /><!--服务端IP-->
    <!--<add key="ServerIP" value="127.0.0.1" />-->
    <add key="ServerPort" value="8100" /><!--服务端监听端口-->
    <add key="CallbackExeName" value="AutoUpdater.Client.Test.exe" /><!--客户端更新完成之后要启动的你的主程序-->
    <add key="Title" value="自动更新" />
    <!--<add key="PageSize" value="11534336" />-->
    <add key="PageSize" value="1048576" />
    <add key="ReceiveTimeOutSeconds" value="30" />
    <add key="ConnectTimeOutSeconds" value="30" />
    <add key="ClientSettingsProvider.ServiceUri" value="" />
  </appSettings>
```
# Tips：

    后端技术：
	
      * C# Winform
	  
	  * 多线程编程
	  
	  * 策略模式，反射，面向接口编程
      
      * MsgPack 一个开源库能将数据流byte[]转化成对象
	  
      * TCP协议基础概念

      * TCP封包，粘包，拆包处理