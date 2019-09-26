## RTPPFormatterWithCSharpScript
---

### 简介
这是一个依赖于插件[RealTimePPDisplayer](https://github.com/OsuSync/RealTimePPDisplayer)以及[ConfigGUI](https://github.com/OsuSync/SyncPlugin/tree/master/ConfigGUI)的Sync小插件.它可以给前者提供一个Formatter，通过编写C#代码来给Displayer提供文本格式化.

### 用法
0. 若没有安装ConfigGUI，请在Sync输入`plugins install config`来安装此插件；若没有安装RealTimePPDisplayer,请输入`plugins install displayer`来安装.
1. 去[Release页面](https://github.com/MikiraSora/RTPPFormatterWithCSharpScript/releases)下载最新的zip文件，并解压到Sync根目录.
2. 打开Sync,输入`config`.
3. 找到RealtimePPDisplayer的配置页面，将Formatter项更改成`rtppfmt-csharp-script`.点击Save按钮，关闭配置窗口，Sync输入`restart`来重启.
![](https://puu.sh/ElrFm/07610e19e5.png)
4. 重启后，再输入`config`打开配置页面,打开PPFormat的编辑窗口，写入指定格式的C#代码即可看到Sync控制台输出的实际C#动态代码,以及wpf窗口和编辑器底下preview窗口显示的结果.[代码格式指导]()，[脚本示例1](),[脚本示例2]()
![](https://puu.sh/ElrRU/622c263eb5.png)
5. 若对Formatter代码满意，则关闭Formatter编辑器窗口，并点击Save按钮。


### 脚本部分格式说明

[**ScriptTemplate**](https://github.com/MikiraSora/RTPPFormatterWithCSharpScript/blob/master/ScriptTemplate.cs)<br>
虽然是以C#形式来写脚本，但为了便利，有一些内容是和前者有所差异的 :
* 直接填写C#代码语句，插件会把直接写的代码替代到ScriptTemplate里的`/*@@script@@*/`部分，自带一个参数[display(查看定义)](https://github.com/OsuSync/RealTimePPDisplayer/blob/master/Displayer/DisplayerBase.cs),必须要返回一个字符串.
```csharp
return "pp:"+display.Pp.RealtimePP;
```

* 可以在任意地方使用`using xxxx;`来使用钦定命名空间的类型.若需要添加dll引用也能使用`//c:\your.dll`来表示 :
```csharp   
using MyNameSpace;
//e:\mylib.dll

return "{display.Pp.RealtimePP:F2}pp";
```

* 一般直接编写C#代码会整合替换在ScriptTemplate里的`/*@@script@@*/`部分.若想声明类成员字段以及方法等，则需要用`@@...多行代码...@@`或者`@单行代码;`来表示，后者两个会替换`/*@@addition@@*/`部分 :
```csharp
@int current = 0;

@@
    int break_time=0;
    int prev_combo=0;
    
    //若定义了Clear，则每次Formatter复位时,都会自动调用此方法
    public void Clear()
    {
        break_time=0;
        prev_combo=0;
    }
@@

var n100 = display.HitCount.Count100;
var n50 = display.HitCount.Count50;
var nmiss = display.HitCount.CountMiss;

var cb = display.HitCount.Combo;

if (cb<prev_combo)
    break_time++;

prev_combo = cb;

return $"{n100}x100 {n50}x50 {nmiss}xMoe {break_time}xBrk";
```