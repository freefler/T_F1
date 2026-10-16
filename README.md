# 工业上位机通信系统（Modbus RTU）

基于 C# WinForms 的 Modbus RTU 上位机通信程序，支持串口参数配置、主从站联调、周期采集、历史数据存储与查询。

## ✨ 功能特性

- 串口参数可视化配置（波特率 / 校验位 / 数据位 / 停止位）
- Modbus RTU 主站周期轮询采集，支持单次读取
- 内置 Modbus 从站模拟器，**无需硬件即可完成联调**
- 采集数据写入 SQL Server，支持历史数据查询
- 通信全程异步执行，界面不卡顿
- 超时自动报错、断线可恢复，资源正确释放

## 🛠 技术栈

| 类别 | 技术 |
|------|------|
| 语言 / 框架 | C# 、.NET Framework 4.7.2 、WinForms |
| 通信协议 | Modbus RTU（NModbus4） |
| 数据库 | SQL Server 2022 Express + ADO.NET |
| 开发工具 | Visual Studio |

## 📁 项目结构

```
THE_Ftry_project/
├─ Form1.cs                界面层：只负责显示与转发用户操作
├─ ModbusRtuService.cs     主站通信服务：连接 / 读写 / 周期轮询
├─ ModbusSlaveService.cs   从站模拟服务（仅用于自测）
├─ DBhelper.cs             数据库访问：参数化 SQL
└─ pic/                    状态灯图片资源
```

## 🏗 架构设计

界面与通信解耦：串口与协议全部封装在独立的服务类中，通过**事件**向界面推送数据，
界面层不直接操作串口，也不关心协议细节。

```
 ┌──────────────┐
 │    Form1     │  界面层
 └──────┬───────┘
  方法调用│ ▲ 事件通知（DataReceived / ErrorOccurred）
        ↓ │
 ┌──────────────┐
 │ ModbusRtu    │  通信层（后台线程轮询）
 │ Service      │
 └──────┬───────┘
        │ 串口报文
 ┌──────▼───────┐
 │ ModbusSlave  │  从站模拟器（自测用）
 │ Service      │
 └──────────────┘
```

## 💡 关键技术点

- **通信不阻塞 UI**：`Task.Run` + `CancellationToken` 后台轮询，避免同步读写卡死界面
- **串口独占保护**：串口是独占资源，用 `lock` 串行化读写，防止并发导致数据撞车
- **跨线程安全刷新**：事件在后台线程触发，用 `InvokeRequired` + `BeginInvoke` 切回 UI 线程
- **资源正确释放**：先 `Dispose` 上层 Modbus 对象，再关闭底层串口，顺序不可颠倒
- **参数化 SQL**：所有数据库操作使用参数占位符，杜绝 SQL 注入

## 🚀 快速开始

**1. 创建数据库与表**

```sql
CREATE DATABASE SCADA_Demo;
GO

USE SCADA_Demo;
GO

CREATE TABLE HistoryData
(
    Id          INT IDENTITY(1,1) PRIMARY KEY,   -- 自增主键
    TagName     NVARCHAR(50)  NOT NULL,          -- 数据点名称
    TagValue    FLOAT         NOT NULL,          -- 数值
    CollectTime DATETIME      NOT NULL           -- 采集时间
);
```

**2. 修改连接字符串**

在 `DBhelper.cs` 中改成你自己的服务器名：

```csharp
Data Source=你的服务器名\SQLEXPRESS;Initial Catalog=SCADA_Demo;Integrated Security=True;TrustServerCertificate=True;
```

> `TrustServerCertificate=True` 用于信任本地自签名证书，新版 SQL Server 驱动必须加。

**3. 编译运行**

用 Visual Studio 打开 `THE_Ftry_project.slnx`。

**4. 无硬件联调**

程序内置从站模拟器。用虚拟串口工具（如 com0com）造一对互通串口，
主站、从站各选一个即可完成通信测试。

## 📌 后续计划

- [ ] 报警模块（上下限判断 + 报警确认 + 报警记录）
- [ ] 历史数据曲线展示
- [ ] Modbus TCP 支持
- [ ] 采集数据预处理（滤波、异常值剔除、缺失插值）
- [ ] 数据导出 Excel

## 📝 提交规范

提交信息遵循 Conventional Commits：`<type>(<scope>): <subject>`

常用 type：`feat` / `fix` / `docs` / `style` / `refactor` / `perf` / `test` / `chore`
