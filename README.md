# T_git

C# 上位机项目

## 提交规范（Conventional Commits）

提交信息格式：`<type>(<scope>): <subject>`

常用 type：`feat`（新功能）、`fix`（修复）、`docs`（文档）、`style`（格式）、`refactor`（重构）、`perf`（性能）、`test`（测试）、`build`（构建）、`chore`（杂项）

示例：
```
feat(serial): 新增串口自动扫描功能
fix(ui): 修复波形显示在高 DPI 下模糊的问题
```

---

## 目录说明（开发前必看）

### ✅ 工作区（唯一开发位置，所有修改都在这里做）

```
C:\Users\tgy\Desktop\github\T_git\
├─ THE_Ftry_project.slnx      ← 解决方案入口，VS 双击打开这个
├─ packages\                     NuGet 依赖包（被 .gitignore 忽略，不用管）
├─ README.md                     本文件
└─ THE_Ftry_project\             项目源码
   ├─ Form1.cs                   ★ 界面逻辑（主要修改对象）
   ├─ Form1.Designer.cs          ★ 界面控件定义（VS 自动生成，一般不手改）
   ├─ ModbusRtuService.cs        ★ 主站通信服务（已完成，暂不改）
   ├─ ModbusSlaveService.cs      ★ 从站模拟服务（自测用，上线时删除）
   ├─ Program.cs                   程序入口（不用改）
   ├─ App.config                   配置文件（第二期会用）
   ├─ Properties\                  程序集信息（不用改）
   ├─ pic\                         状态灯图片 G绿 / R红
   ├─ packages.config              NuGet 包清单
   └─ bin\  obj\                   编译产物（不提交）
```

> 图例：★ = 需要关注的代码文件

### 📦 备份区（只读存档，不要在这里改代码）

```
C:\Users\tgy\Desktop\上位机\_备份\
└─ THE_Ftry_project_初始版_20261009\     第一期完成时的完整快照
```

### 🔖 Git 还原点

| 标识 | 说明 |
|---|---|
| 提交 `0cb91e5` | 初始版（待修改） |
| 标签 `v0.1-初始版备份` | 大重构前的还原点 |

回退命令：`git reset --hard v0.1-初始版备份`

---

## 当前进度

- [x] 第一期：Modbus RTU 通信（主从站联调跑通）
- [ ] 从站服务接入 Form1（进行中）
- [ ] 串口枚举改用 `SerialPort.GetPortNames()`
- [ ] 清理死代码（port1 / master1 / Star_Pol）
- [ ] 控件统一改名
- [ ] 第二期：SQL Server 数据存储

