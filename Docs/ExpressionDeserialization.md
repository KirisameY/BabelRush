# 表达式对象与 Tomlyn 反序列化设计计划

> 状态：设计草案（**未施工**）
> 范围：`BabelRushR.Core`，Tomlyn 2.10.1
> 对应路线图：`WorkPlan.md` →「表达式类型 & 复杂反序列化支持」
> 本文只解决**反序列化**；表达式的业务语义（求值、目标选择、修饰器接入等）由各派生类型自己实现。
> 本轮不运行测试：Core 测试目前有另一处在修，设计里的测试只列计划，落地时再执行。

---

## 0. 结论摘要

一句话：**`Expression` 抽象 record + 以 `RegKey` 为键的全局注册表 + 每个被声明类型一个 `TomlConverter<T>`（由 `TomlConverterFactory` 创建）+ 自带的 `name(arg, ...)` 参数 DSL。**

具体决定：

1. 基类 `public abstract record Expression;` 放在 `BabelRushR.Core.Expressions`。
2. 用 `ExpressionTomlConverterFactory : TomlConverterFactory` 接管所有 `Expression` 及其派生类型。Tomlyn 会按**属性被声明的类型**创建对应的 `ExpressionTomlConverter<T>`。
3. 注册表接受两种东西：
   - **命名反序列化器**：`RegKey -> (ExpressionArgs -> Expression)`；
   - **每类型一个 fallback**：`Type -> (ExpressionFallbackArgs -> Expression)`，用于没有括号的裸字符串。
4. 注册分两层：
   - **手动注册**（基础能力，永远可用）；
   - **特性声明 + 自动扫描**（便利层，派生类最多写 `[ExpressionDefinition]` / `[ExpressionFallback]`）。
5. 参数由 `ExpressionArgs.Read<T>()` 按位置读取，内置 `Expression` 递归、`RegKey`、`string`、`bool`、数值、枚举；游戏类型通过 `RegisterArgConverter<T>` 扩展。
6. 反序列化只负责“字符串 → 表达式对象”。`Write` 暂不支持，抛 `NotSupportedException`，避免写出错误数据。
7. 接入点是 `SerializingOptions.CollectOptions()`；表达式转换器放在 `Converters` **最后**，让显式的 `[TomlConverterFor]` 可以覆盖它。
8. `DefaultBeforeInitialize` **不**加入表达式转换器：`pack.toml` 的读取早于包程序集加载，此时注册表还不完整。

---

## 1. 需求拆解

把原始需求逐条落到设计元素上，避免后文跑偏：

| 需求 | 设计落点 |
| --- | --- |
| 从 `expression` 或 `expression(arg1, arg2, ...)` 字符串反序列化 | `ExpressionParser` + `ExpressionTomlConverter<T>` |
| args 是其他类型的字段或另一个表达式 | `ExpressionArgs.Read<T>()`，`T` 可以是 `Expression` 派生类型，也可以是标量/`RegKey`/自定义类型 |
| .NET 侧基类是 `abstract record`，允许自身和外部程序集派生 | `public abstract record Expression;`；工厂用 `typeof(Expression).IsAssignableFrom(type)` 判定 |
| 不考虑参数多态、可选参数、默认值、按参数名传参 | 只做**按位置**读取；一个 expression 名对应一个固定反序列化器/构造器 |
| Tomlyn 根据属性声明的类型分发给具体反序列化器 | `TomlConverterFactory` + `ExpressionTomlConverter<T>`（见 §3.2 的原因） |
| 可以注册一个无括号 fallback | `RegisterFallback<T>()` / `[ExpressionFallback]` |
| 一系列 expression 名 → 反序列化器 | `Register<T>(RegKey, ...)` / `[ExpressionDefinition]` |
| expression 名是 `RegKey`，避免程序集间冲突 | 注册表键就是 `RegKey`；代码注册必须写全限定名，TOML 里裸名走包默认命名空间 |
| 派生类尽量少写代码 | 特性 + 构造器绑定（`ExpressionConstructorBinder`）覆盖常见 record；特殊情况再手动注册 |
| 只解决反序列化 | `Write` 明确不支持；不求值、不做业务校验、不接注册表 |

### 1.1 非目标

明确不做，避免实现时膨胀：

- **不求值**。`Expression` 只是数据；`Evaluate(...)` 之类由派生类型自己定义。
- **不做参数多态/重载**。同一个 expression 名不会按参数个数或类型尝试多个候选。
- **不支持可选参数、默认值、命名参数**（`foo(x = 1)`）。以后要加时，`ParameterInfo.HasDefaultValue` / 参数名匹配可以接在构造器绑定层，不影响语法主干。
- **不支持表达式序列化/round-trip**。`Write` 抛异常。
- **不支持运行时热注册**。注册必须在 `CollectOptions()` 之前完成；构建后冻结。
- **不保证 NativeAOT/trimming**。现有 `AttributedTypeScanner` 也是反射方案，保持一致。
- **不改变现有 `[TomlPolymorphic]` 机制**。表达式走字符串，不走 `$type` 表。

---

## 2. 现有基础设施与已核对事实

### 2.1 相关代码位置

| 关注点 | 文件 |
| --- | --- |
| TOML 选项构建 | `BabelRushR.Core/Common/Serializing/SerializingOptions.cs` |
| `RegKey` 转换 | `BabelRushR.Core/Common/Serializing/RegKeyConverter.cs` |
| 特性扫描 | `BabelRushR.Core/Common/AttributedTypeScanner.cs` |
| 数据注册生命周期 | `BabelRushR.Core/Data/DataRegistrationManager.cs`、`DataRegistrant.cs` |
| 包加载顺序 | `BabelRushR.Core/Contents/ContentsLoaderManager.cs` |
| 测试复位 | `BabelRushR.Core.Test/SerializingOptionsTestAccessor.cs` |

关键调用顺序（`ContentsLoaderManager.LoadPacks`）：

```text
ResolveMetaData（读 pack.toml，使用 DefaultBeforeInitialize）
  → LoadPackAssemblies（包 DLL 进入 PackAssemblyLoadContext）
  → RegisterAttributedLoaders
       → DataLoader.Create()
            → DataRegistrationManager.InitializeRegisters()
                 → 逐个 RunClassConstructor([DataRegisterInitialize] 的静态类)
  → SerializingOptions.CollectOptions()
       → 收集多态映射 / 转换器 / TypeInfo
  → 逐包、逐 DataRegistrant 加载 /data 下的 TOML
```

这个顺序决定了：

- 表达式注册的静态初始化（手动注册）必须在 `CollectOptions()` 之前完成；
- `CollectOptions()` 时包程序集已经全部加载，`AttributedTypeScanner` 能看到包里的 `Expression` 派生类型和特性；
- 任何 `Expression` 字段的 TOML 反序列化发生在 `CollectOptions()` 之后，注册表已经冻结。

### 2.2 Tomlyn 2.10.1 的实测事实

以下不是从文档推测，是在本机 `Tomlyn 2.10.1` 上跑探针确认的（探针放在 `.tmp/tomlprobe`，不计入仓库）：

| 事实 | 影响 |
| --- | --- |
| `TomlConverter<T>.CanConvert(Type)` 是 `sealed`，只对**精确类型**返回 true | 只注册 `TomlConverter<Expression>` **不能**处理属性声明为 `EntityExpression` / `AddExpression` 的情况；基类转换器只对声明为 `Expression` 的属性生效 |
| `TomlConverterFactory.CanConvert(Type)` 可重写，返回 true 后 `CreateConverter` 会为实际声明类型创建一个转换器 | 这是同时覆盖 `Expression` 抽象基类和所有派生类型的正确入口 |
| 工厂对 `Expr`、`Num`、`List<Expr>`、`Dictionary<string, Expr>` 的元素都会被调用 | 集合、字典、嵌套 record 里的表达式字段不需要额外处理 |
| record/class 的**构造器参数**也会走工厂（实测） | `RecordCardType` 这类主构造器映射的 record 不需要改成可写属性 |
| 声明为抽象 `Expr` 的属性可以正常走工厂，不需要 `[TomlPolymorphic]` | 不要给 `Expression` 标 `[TomlPolymorphic]`；字符串不是 TOML 表 |
| 在 `options.Converters` 里，**顺序靠前**的转换器先匹配 | 表达式工厂放最后，让显式 `[TomlConverterFor]` 覆盖 |
| 转换器里 `reader.TokenType` 可读；非字符串时 `GetString()` 会抛 `TomlException`（`Expected token String but was Integer`） | 可以自己先判 `TokenType` 给出更贴切的错误，也可以交给 `GetString()` |
| `reader.Options` 是当前反序列化使用的选项（含 `WithDefaultNamespace` 替换后的 `RegKeyConverter`） | 表达式名解析默认命名空间时，从这里拿 `RegKeyConverter` |
| 转换器里抛出的异常（包括 `TomlException`）会被 Tomlyn 再包一层：`Exception while trying to convert ... Reason: ...` | 错误信息会偏长；表达式自己的消息必须自包含，不能依赖外层上下文 |
| `TomlConverterFactory.CreateConverter` 对同一类型可能被调用多次 | 工厂要无状态；可以按类型缓存转换器实例 |

参考资料：

- Tomlyn 仓库：<https://github.com/xoofx/Tomlyn>
- 官方文档：<https://xoofx.github.io/Tomlyn/>
- 序列化文档（转换器、工厂、多态）：<https://xoofx.github.io/Tomlyn/docs/serialization/>
- 低层 API：<https://xoofx.github.io/Tomlyn/docs/low-level/>

### 2.3 `RegKey` 与 `RegKeyConverter` 的现状

`RegKey`（`KirisameY.Registration`）：

- 值语义，有 `NameSpace` / `Key` / `FullName`；
- `RegKey.From(ns, key)`；
- `RegKey.FromFull(full, separator = ":")`：**不 trim、不校验空段、没有冒号或多个冒号时返回 `null`**（实测），不适合直接拿来解析用户输入；
- 相等性可用于字典键。

`RegKeyConverter` 目前：

```csharp
public override RegKey Read(TomlReader reader) => reader.GetString().Split(':') switch
{
    [var ns, var k]                           => RegKey.From(ns, k),
    [var k] when defaultNameSpace is not null => RegKey.From(defaultNameSpace, k),
    var somethingElse                         => throw reader.CreateException(...)
};
```

表达式名需要同一套“默认命名空间”语义，但**校验要更严**（不能接受空命名空间、空 key、空白、括号/逗号）。计划：

- 给 `RegKeyConverter` 加一个只读属性 `public string? DefaultNameSpace => defaultNameSpace;`；
- 表达式名用独立的严格解析器（`ExpressionName.TryParse`），不复用 `RegKeyConverter.Read` 的宽松 `Split`；
- 是否把两者统一到 `RegKeyText.TryParse` 是可选的后续整理；**本轮不改变 `RegKeyConverter` 对现有 TOML 字段的行为**，避免无关回归。

---

## 3. 总体设计

### 3.1 数据流

```text
TOML: value = "core:add(1, goblin)"
  │
  ▼
Tomlyn 根据属性声明类型 T（Expression / EntityExpression / AddExpression ...）
  │  选择 ExpressionTomlConverter<T>
  ▼
reader.GetString()
  │
  ▼
ExpressionRegistry.Read<T>(text, context)
  │
  ├─ ExpressionParser.Parse
  │     ├─ 无括号 → fallback
  │     │     → ExpressionFallbackArgs("goblin") → T
  │     └─ 有括号 → name = "core:add"，args = ["1", "goblin"]
  │           → 查 RegKey 注册表 → Registration
  │           → ExpressionArgs
  │                 ├─ Read<Expression>() → 递归 ExpressionRegistry
  │                 ├─ Read<RegKey>()     → 默认命名空间解析
  │                 └─ Read<int>()        → 标量解析
  │           → Registration.Deserialize(args) → Expression
  │
  ├─ 检查结果是否可赋给 T
  └─ 检查参数是否恰好消费完
  │
  ▼
T 实例
```

### 3.2 为什么是 `TomlConverterFactory`，而不是“一个基类转换器”或“每个类型一个特性”

| 方案 | 问题 |
| --- | --- |
| 只注册 `TomlConverter<Expression>` | `CanConvert` 被 `TomlConverter<T>` 封死为精确匹配；属性声明为派生类型时不会用它，Tomlyn 会按 record 结构去映射字符串，直接失败（实测） |
| 给每个表达式类型手写/扫描一个 `TomlConverter<T>` | 需要先枚举所有派生类型；抽象基类仍要特判；包程序集动态加载后新增的类型不一定在枚举里；注册代码多 |
| 用 `[TomlPolymorphic]` + `TomlDerivedType` | 输入是字符串，不是带 `$type` 的表；参数是位置参数，不是 TOML 属性；判别值也不是 `RegKey` 语义 |
| **`TomlConverterFactory`** | 一次注册覆盖整条继承链；Tomlyn 把**声明类型**作为 `CreateConverter` 参数传进来，正好用于选择 fallback 和做结果类型检查；派生类型零额外注册 |

因此：`ExpressionTomlConverterFactory` 是唯一的 Tomlyn 侧入口，内部为每个声明类型创建 `ExpressionTomlConverter<T>`。

### 3.3 核心类型清单

公开 API（`BabelRushR.Core.Expressions`）：

| 类型 | 职责 |
| --- | --- |
| `abstract record Expression` | 所有表达式的基类；无成员 |
| `static class ExpressionDeserialization` | 手动注册入口；`Register` / `RegisterFallback` / `RegisterArgConverter` |
| `sealed class ExpressionArgs` | 按位置读取参数；`Read<T>()` / `ReadRegKey()` / `ReadString()` / `EnsureEnd()` |
| `sealed class ExpressionFallbackArgs` | 裸字符串 fallback 的输入；`Text` / `Read<T>()` / `ReadRegKey()` |
| `sealed class ExpressionReadContext` | 当前读取上下文（内部持有 `TomlReader`）、选项、默认命名空间、深度；`CreateException` / `ParseRegKey` |
| `sealed class ExpressionArgContext` | 自定义参数转换器拿到的当前参数文本 + 上下文 |
| `interface IExpressionArgConverter` | 类型族参数转换器的扩展点 |
| `[ExpressionDefinition("ns:name")]` | 标在派生 record 上，声明命名形式 |
| `[ExpressionFallback]` | 标在派生 record 上，声明裸字符串 fallback（单参数构造器） |
| `[ExpressionConstructor]` | 标在构造器上，消歧义 |

内部实现：

| 类型 | 职责 |
| --- | --- |
| `ExpressionParser` | 解析 `name(args...)`，只做顶层切分；参数类型解释交给 `ExpressionArgs` |
| `ExpressionRegistry` | 冻结后的查找表：`RegKey -> Registration`、`Type -> Fallback` |
| `ExpressionRegistryBuilder` | 收集描述符、校验、冻结；测试可直接构造 |
| `ExpressionAttributeScanner` | 扫描 `[ExpressionDefinition]` / `[ExpressionFallback]`，生成描述符 |
| `ExpressionConstructorBinder` | 根据构造器参数类型生成反序列化委托 |
| `ExpressionArgConverters` | 内置参数类型 + 自定义转换器解析 |
| `ExpressionTomlConverterFactory` | `TomlConverterFactory` 实现 |
| `ExpressionTomlConverter<T>` | 每个声明类型一个的 `TomlConverter<T>` |

---

## 4. 语法与语义

### 4.1 文法

```text
expression-string := ws expression ws
expression        := name [ ws "(" [ arg-list ] ")" ]
arg-list          := arg { ws "," ws arg }
arg               := quoted-string | expression | bare-text
name              := regkey-text
regkey-text       := key | namespace ":" key
```

注意：**Parser 不负责判断 `arg` 是哪种类型**。它只按顶层逗号切分，保留每段原文和偏移；`ExpressionArgs.Read<T>()` 再根据期望类型解释：

- 期望 `Expression` 派生类型 → 把整段当表达式递归解析；
- 期望 `RegKey` → 当 RegKey 文本；
- 期望 `string` → 引号字符串或裸文本；
- 期望数值/布尔/枚举 → 标量解析；
- 期望自定义类型 → 自定义 `IExpressionArgConverter`。

这样同一段文本 `goblin` 在不同参数位置上可以有不同含义，且不需要语法层做类型推断。

### 4.2 裸表达式 vs 括号表达式（关键决议）

推荐规则，简单且无歧义：

- **有括号** → 一定走“命名反序列化器”查找。零参数也要写 `name()`。
- **没有括号** → 一定走“声明类型的 fallback”。不会去查命名注册表。

理由：

1. 和“无参（没有括号）的回退”这个描述一致；
2. 裸文本通常是数据（实体 id、数字、字符串），不应被同名的表达式注册意外截胡；
3. 零参数命名表达式写成 `name()` 很自然，且能明确表达“这是一个操作，不是字面量”。

备选规则（如果确实需要 `"owner"` 直接当命名表达式）：

- 先查“零参数命名注册”，找到就用；否则再 fallback。
- 代价：实体/字段数据与表达式名冲突时会被静默截胡；需要给注册表存 arity 并处理优先级。
- 本设计**不推荐**，但实现上留了余地：`Registration` 可以带 `ParameterCount`，以后要改规则不需要动语法。

### 4.3 名称与命名空间

**代码注册**：

- 必须全限定：`RegKey.From("core", "add")` 或字符串 `"core:add"`；
- 字符串重载在注册时解析；没有 `:`、多个 `:`、空段、空白 → 直接抛 `ArgumentException`；
- 不允许用当前包的默认命名空间（注册发生在程序集加载期，没有“当前包”这个概念）。

**TOML 引用**：

- `"core:add(1, 2)"` → 显式命名空间 `core`；
- `"add(1, 2)"` → 使用当前反序列化选项的默认命名空间（`SerializingOptions.WithDefaultNamespace(packId)` 注入的 pack id）；
- 没有默认命名空间又写了裸名 → `TomlException`，提示写全限定名或用带命名空间的选项反序列化；
- 大小写敏感（`RegKey` 的相等性），文档里明确。

**为什么代码注册必须全限定**：不同程序集都可以定义 `add`，只有命名空间能区分。TOML 里的裸名是“当前包自己的名字”，这是使用侧约定，不是注册侧的默认值。

### 4.4 空白与字符串

- 名称、括号、逗号两侧允许空白和换行；解析前 trim。
- 名称内部不允许空白；`core : add` 视为非法。
- 参数段首尾 trim；空段（`(1,,2)`、`(1,)`）报错。
- 双引号字符串按 TOML basic string 处理（至少支持 `\"`、`\\`、`\n`、`\t`、`\r`、`\uXXXX`）；
- 单引号字符串按 literal string 处理（不转义），方便写 `'a,b(c)'`；
- 引号内的逗号、括号不参与切分；
- 多行 TOML 字符串里的换行按普通空白处理（引号内除外）。

实现上可以手写一个小型扫描器；不建议为每个参数拼一个 TOML 文档再交给 Tomlyn——裸 token（`goblin`）不是合法 TOML 值，且错误位置会失真。

### 4.5 参数、递归与限制

- 参数**按位置**读取；一个名字对应一个固定模式。
- 读取完由框架自动调用 `EnsureEnd()`；多传参数报错，不需要每个 delegate 自己记得检查。
- `Expression` 参数递归解析；深度上限建议 32（或复用 `TomlSerializerOptions.MaxDepth` 的有效值，取较小者）。
- 单表达式参数个数上限建议 64；表达式字符串长度上限建议 64 KiB。两者都是防御性限制，不是业务约束。
- 参数读取失败时错误信息带 `argument #N`；特性绑定的构造器再补上参数名。

### 4.6 示例

| TOML 值（字段声明类型） | 结果 |
| --- | --- |
| `"core:add(1, 2)"`（`Expression`） | `AddExpression(ConstantExpression(1), ConstantExpression(2))`（前提：`Expression` 根 fallback 负责数字） |
| `"goblin"`（`EntityExpression`） | `EntityExpression` 的 fallback 把 `goblin` 解析成实体引用 |
| `"core:entity(goblin)"`（`EntityExpression`） | 命名注册 `core:entity`，参数 `EntityType` 由自定义转换器读取 |
| `"core:add(1, core:mul(2, 3))"` | 嵌套表达式 |
| `"core:say(\"a,b\", 'c(d)')"` | 两个字符串参数，逗号/括号在引号内不切分 |
| `"3"`（`Expression`） | 根 fallback 解析成常量表达式 |
| `"3"`（`EntityExpression`） | 走实体 fallback，通常会报“找不到实体 3”，而不是变成常量 |

---

## 5. 注册模型

### 5.1 手动注册 API

```csharp
namespace BabelRushR.Core.Expressions;

public abstract record Expression;

public static class ExpressionDeserialization
{
    // 命名形式：TOML 里写 "core:damage(...)"
    public static void Register<TExpression>(
        RegKey name,
        Func<ExpressionArgs, TExpression> deserializer)
        where TExpression : Expression;

    public static void Register<TExpression>(
        string name,
        Func<ExpressionArgs, TExpression> deserializer)
        where TExpression : Expression;

    // 裸形式：TOML 里写 "goblin"
    public static void RegisterFallback<TExpression>(
        Func<ExpressionFallbackArgs, TExpression> fallback)
        where TExpression : Expression;

    // 自定义参数类型
    public static void RegisterArgConverter<T>(
        Func<ExpressionArgContext, T> converter);

    public static void RegisterArgConverter(IExpressionArgConverter converter);
}
```

用法：

```csharp
[DataRegisterInitialize]
public static class CoreExpressions
{
    static CoreExpressions()
    {
        // 根类型：裸数字 -> 常量表达式
        ExpressionDeserialization.RegisterFallback<Expression>(
            args => new ConstantExpression(args.Read<double>()));

        // 命名表达式
        ExpressionDeserialization.Register<AddExpression>(
            "core:add",
            args => new AddExpression(args.Read<Expression>(), args.Read<Expression>()));
    }
}
```

要点：

- `Register<T>` 的 `T` 是**声明结果类型**；delegate 可以返回子类型，运行时仍按实际类型做可赋值检查。
- **expression 名是注册表键，不是 `Expression` 对象的属性**：同一类型可以有多个名字（别名），对象不需要知道自己是哪个名字构造的。业务若需要名字，派生类型自己加字段；框架不往基类塞 `Name`，避免每个派生类都要实现。
- **同一个 `RegKey` 不能按声明类型重载**：如果两个表达式类型都想要 `"core:const"` 但参数模式不同，必须用不同名字（`core:int_const` / `core:float_const`），或者合并成一个类型。这是“本轮不做参数多态”的直接后果。
- 同名注册全局唯一；重复直接抛（构建期错误，不是读 TOML 时才炸）。
- delegate 返回 `null`、返回不可赋给声明类型的结果、或抛非 `TomlException` 异常，都由框架转成带上下文的 `TomlException`。
- 框架在 delegate 返回后自动 `EnsureEnd()`；variadic 表达式以后要支持时再加显式开关。

### 5.2 特性声明（便利层）

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ExpressionDefinitionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ExpressionFallbackAttribute : Attribute;

[AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
public sealed class ExpressionConstructorAttribute : Attribute;
```

用法：

```csharp
[ExpressionDefinition("core:add")]
public sealed record AddExpression(Expression Left, Expression Right) : Expression;

[ExpressionDefinition("core:entity")]
[ExpressionFallback]
public sealed record EntityExpression(EntityType Entity) : Expression;

[ExpressionDefinition("core:const")]
[ExpressionFallback]
public sealed record ConstantExpression(double Value) : Expression;
```

- `[ExpressionDefinition]` 可标多个：同一类型可以有多个名字（别名），共用同一个构造器模式。
- 特性实参只能是编译期常量，所以 `ExpressionDefinitionAttribute` 收 `string`（必须是 `ns:key`），构建期再转成 `RegKey`；手动注册的 `Register` 直接收 `RegKey`。
- `[ExpressionFallback]` 表示：裸字符串按**唯一构造参数的类型**读取，然后调用构造器。要求恰好一个构造参数，且参数类型不是 `Expression` 派生类型（否则会递归，构建期直接拒绝）。零参数、多参数或需要自定义解析逻辑时，用 `RegisterFallback` 手动注册。
- 构造器选择规则：
  1. 有 `[ExpressionConstructor]` → 用它（允许多个时只有它能消歧义）；
  2. 否则恰好一个 public 实例构造器 → 用它；
  3. 否则抛，错误信息列出候选并要求标注。
- 参数按位置读取；本轮不处理 `HasDefaultValue`、参数名传参、重载选择。
- 特性绑定**只走构造器**：`required` / `init` / 可写属性不会被填充；如果类型需要这些，改用手动 `Register`，或在构造器里完成全部初始化。

等价关系：

```csharp
[ExpressionDefinition("core:add")]
public sealed record AddExpression(Expression Left, Expression Right) : Expression;
```

在构建期会被展开成类似：

```csharp
ExpressionDeserialization.Register<AddExpression>(
    "core:add",
    args => new AddExpression(args.Read<Expression>(), args.Read<Expression>()));
```

只是参数读取和构造由反射 binder 完成，派生类不用自己写。

### 5.3 fallback 规则

- **每个类型最多一个 fallback**；重复注册（特性 + 手动）时：
  - 手动 fallback **覆盖**特性声明的 `[ExpressionFallback]`，并记一条 debug/info 日志；
  - 两个手动 fallback → 抛。
- fallback 只对**精确声明类型**生效，不沿基类链继承。理由：`Expression` 根 fallback 可能是“数字 → 常量”，`EntityExpression` 的 fallback 是“裸名 → 实体”；沿继承链回退会让实体字段意外接受数字。
- 如果确实想让一组类型共享 fallback，用泛型 helper 或手动为每个类型注册同一个 delegate。
- 手动注册 `RegisterFallback<Expression>` 是允许的，用于给根类型一个“常量/数字”默认解释。

### 5.4 注册表生命周期

```text
声明阶段：
  [DataRegisterInitialize] 静态构造 / 手动 Register
  [ExpressionDefinition] / [ExpressionFallback] 由扫描器在 Build 时读取
    ↓
构建阶段（SerializingOptions.CollectOptions）：
  ExpressionDeserialization.Build()
    - 扫描特性
    - 合并手动描述符
    - 校验（类型、构造器、参数转换器、重名、fallback 冲突）
    - 冻结为 ExpressionRegistry
    ↓
读取阶段：
  ExpressionTomlConverter<T> 只读 ExpressionRegistry
```

纪律：

- **注册类建议标 `[DataRegisterInitialize]`**（复用现有 `InitializeRegisters` 管线），或在程序集 module initializer 里注册；不要依赖“普通静态构造器恰好在 `CollectOptions` 前被首次触达”——那是不可控的。
- `Register` 在构建后调用 → 抛 `InvalidOperationException`（“registrations must be completed before serialization starts”）。
- 构建后注册表不可变、可并发读；构建阶段用 `Lock` 保护。
- `Build()` 是幂等的：已构建则直接返回；`ResetForTests()` 只清冻结结果，不清手动描述符，保证测试重复初始化时手动注册不丢。
- 构建失败时聚合错误（列出所有非法项），不要只报第一条；与 `CollectOptions` 现有“配置错误直接抛”的风格一致。

### 5.5 校验清单（构建期）

对每个特性/手动描述符：

- 类型是具体 class/record、非 abstract、非 open generic、可赋给 `Expression`；
- expression 名是合法的全限定 `RegKey`；
- 名字全局唯一（跨程序集、跨 Core/包程序集）；
- 构造器能唯一确定；参数类型都有对应的参数转换器；
- `[ExpressionFallback]` 恰好一个构造参数，且参数不是 `Expression` 派生类型；
- 手动注册的 delegate 非 null；
- 手动注册的 `RegKey` 不是默认值/空段；
- 结果类型可赋给声明类型（泛型在编译期保证；运行时再兜底）。

错误示例：

```text
Expression definition 'core:add' on 'My.Asm.AddExpression' is invalid:
  - constructor parameter 'left' has type 'System.Linq.Expressions.Expression', which is not a BabelRush Expression.
Duplicate expression name 'core:add': attribute on 'My.Asm.AddExpression' and manual registration.
Expression type 'My.Asm.WeirdExpression' has 3 public constructors; annotate one with [ExpressionConstructor].
Expression fallback on 'My.Asm.WrapperExpression' requires exactly one constructor parameter, but found 2.
No argument converter is registered for parameter 'entity' of type 'My.Asm.EntityType' on expression 'core:entity'.
```

---

## 6. 参数读取

### 6.1 内置类型

| 声明类型 | 读取规则 |
| --- | --- |
| `Expression` 及派生 | 递归 `ExpressionRegistry.Read(type, text)`；结果必须可赋给该类型 |
| `RegKey` | 严格 RegKey 文本；无 `:` 时用当前默认命名空间；引号包裹时先去掉引号 |
| `string` | 双引号字符串（解码 TOML 转义）或裸文本；裸文本取 trim 后的整段 |
| `bool` | `true` / `false`，大小写不敏感 |
| 整数（`sbyte`…`ulong`、`nint`/`nuint`、`Int128`/`UInt128`） | InvariantCulture；允许前导 `+`/`-`；越界报错 |
| 浮点（`float`/`double`/`decimal`/`Half`） | InvariantCulture；支持下述形式；暂不保证 `inf`/`nan`（以后可加） |
| 枚举 | 先按名字（忽略大小写）解析；失败再按整数解析；建议校验 `Enum.IsDefined` |
| `char` | 引号字符串长度为 1，或裸文本长度为 1 |
| 自定义类型 | 查 `RegisterArgConverter<T>` / `IExpressionArgConverter` |

**不做**：`object`、`Nullable<T>`、数组/字典参数、可选参数、默认值。这些以后按需加，不要一开始铺开。

### 6.2 自定义参数转换器

游戏类型示例（`EntityType`）：

```csharp
ExpressionDeserialization.RegisterArgConverter<EntityType>((arg, _) =>
{
    var key = arg.ParseRegKey();
    return EntityType.Registry.TryGetValue(key, out var type)
        ? type
        : throw arg.CreateException($"unknown entity type '{key}'.");
});
```

注意：

- 这里用 `TryGetValue` 而不是 `EntityType.Registry[key]`，因为注册表的 indexer 有 placeholder fallback；表达式里引用错实体应该在加载期报错，而不是静默变成 404。
- 这个转换器会引入**数据加载顺序依赖**：`EntityType` 数据必须先于引用它的卡牌/表达式数据加载。详见 §9.1。
- 自定义转换器在构建期也会被查询（特性绑定的参数类型需要能解析）；因此所有 `RegisterArgConverter` 必须在 `CollectOptions()` 前完成。

### 6.3 泛型读取的内部结构

`ExpressionArgs` 对外是泛型 `Read<T>()`，内部需要非泛型的 `Read(Type)` 供反射 binder 使用：

```csharp
public sealed class ExpressionArgs
{
    public T Read<T>() => (T)Read(typeof(T))!;
    internal object? Read(Type type) => ExpressionArgConverters.Read(type, ...);
}
```

`ExpressionArgConverters.Read(Type, ...)` 的分支顺序：

1. `Expression` 派生 → 递归注册表（`allowExpression = false` 时拒绝）；
2. `RegKey` / `string` / `bool` / 数值 / 枚举 / `char`；
3. 自定义 `IExpressionArgConverter`（按注册顺序，或按精确类型缓存）；
4. 找不到 → `TomlException`，消息里带类型和参数位置。

### 6.4 参数读取的错误上下文

`ExpressionArgs` 维护 `Position` 和当前参数的偏移；读取失败时消息形如：

```text
Argument #2 of expression 'core:add' could not be read as Expression: no fallback is registered for type Expression and the text 'goblin' has no parentheses.
```

特性绑定的构造器再补参数名：

```text
Argument 'right' of expression 'core:add' could not be read as Expression: ...
```

---

## 7. Tomlyn 集成

### 7.1 `ExpressionTomlConverterFactory`

```csharp
using Tomlyn;
using Tomlyn.Serialization;

namespace BabelRushR.Core.Expressions;

internal sealed class ExpressionTomlConverterFactory(ExpressionRegistry registry) : TomlConverterFactory
{
    public override bool CanConvert(Type typeToConvert) =>
        typeof(Expression).IsAssignableFrom(typeToConvert);

    public override TomlConverter CreateConverter(Type typeToConvert, TomlSerializerOptions options)
    {
        if (typeToConvert.ContainsGenericParameters)
            throw new InvalidOperationException(
                $"Expression type '{typeToConvert}' must not be an open generic type.");

        return (TomlConverter)Activator.CreateInstance(
            typeof(ExpressionTomlConverter<>).MakeGenericType(typeToConvert),
            [registry])!;
    }
}
```

可选优化：用 `ConcurrentDictionary<Type, TomlConverter>` 缓存；转换器只持有不可变 registry，按类型复用是安全的。

### 7.2 `ExpressionTomlConverter<T>`

```csharp
internal sealed class ExpressionTomlConverter<TExpression>(ExpressionRegistry registry)
    : TomlConverter<TExpression> where TExpression : Expression
{
    public override TExpression? Read(TomlReader reader)
    {
        if (reader.TokenType != TomlTokenType.String)
        {
            throw reader.CreateException(
                $"Expression value must be a TOML string, but was {reader.TokenType}.");
        }

        var context = ExpressionReadContext.From(reader);
        return registry.Read<TExpression>(reader.GetString(), context);
    }

    public override void Write(TomlWriter writer, TExpression value) =>
        throw new NotSupportedException(
            "Expression serialization is not supported yet; only deserialization is implemented.");
}
```

- `Read` 返回 `TExpression?`（Tomlyn 的签名），但实际永远返回非 null；registry 里对 null 做兜底。
- `Write` 明确抛；以后要 round-trip 时再加一个对称的 formatter 注册表，不在本轮。
- `reader.TokenType` 的判断是防御性的；即使不判，`GetString()` 也会抛，只是消息不如上面贴切。

### 7.3 `SerializingOptions` 的改动

在 `CollectOptions()` 里：

```csharp
var polymorphismMappings = CollectPolymorphismMappings();
var converters = CollectConverters();
var typeInfos = CollectTypeInfos();
var expressionRegistry = ExpressionDeserialization.Build(); // 新增

Default = TomlSerializerOptions.Default with
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    IndentSize = 4,
    PolymorphismOptions = new()
    {
        DerivedTypeMappings           = polymorphismMappings,
        TypeDiscriminatorPropertyName = "$type",
        UnknownDerivedTypeHandling    = TomlUnknownDerivedTypeHandling.Fail
    },
    // 表达式工厂放最后：显式 [TomlConverterFor] 先匹配，可以覆盖表达式类型
    Converters = [RegKeyConverter.Default, ..converters, new ExpressionTomlConverterFactory(expressionRegistry)],
    TypeInfoResolver = new TypeInfoResolver(typeInfos),
};
```

- **不要**改 `DefaultBeforeInitialize`。`pack.toml` 在包程序集加载之前读取，此时表达式注册表还不完整；如果 `PackMetaData` 里出现 `Expression` 字段，应该直接报错，而不是半初始化。
- `WithDefaultNamespace(ns)` 复制 `Default.Converters` 并替换 `RegKeyConverter.Default`，表达式工厂会被原样带过去；工厂持有的 registry 是不可变的，安全。
- 测试复位时，`SerializingOptionsTestAccessor.Reset()` 除了清 `_initialized` / `Default` / 命名空间缓存，还要调用 `ExpressionDeserialization.ResetForTests()`。

### 7.4 `RegKeyConverter` 的改动

最小改动：

```csharp
public class RegKeyConverter(string? defaultNameSpace = null) : TomlConverter<RegKey>
{
    public string? DefaultNameSpace => defaultNameSpace;
    // Read / Write 保持现有行为
}
```

表达式侧通过：

```csharp
var regKeyConverter = reader.Options.Converters.OfType<RegKeyConverter>().FirstOrDefault();
var defaultNamespace = regKeyConverter?.DefaultNameSpace;
```

拿到默认命名空间。`WithDefaultNamespace` 每次都会创建新的 `RegKeyConverter(ns)`，所以列表里通常恰好一个；如果有多个，取第一个并在文档里说明。

不建议本轮把 `RegKeyConverter.Read` 改成严格解析：现有 TOML 数据可能依赖宽松行为（例如空段），而表达式名需要的是另一套更严的规则。两者分开，避免无关回归。

### 7.5 转换器顺序与覆盖

`options.Converters` 的匹配是**顺序第一个命中**（实测）：

- `RegKeyConverter` 在最前：`RegKey` 字段照旧；
- `[TomlConverterFor]` 收集到的转换器在中间：显式覆盖优先；
- `ExpressionTomlConverterFactory` 在最后：兜住所有没被显式覆盖的 `Expression` 类型。

这样：

- 普通 `Expression` 字段走工厂；
- 如果某个表达式类型有特殊的 TOML 表示，可以用 `[TomlConverterFor]` 提供专用转换器，工厂不会抢；
- 不会影响任何非 `Expression` 类型（工厂 `CanConvert` 返回 false）。

### 7.6 与 `[TomlPolymorphic]` 的关系

不要给 `Expression` 或其派生类型标 `[TomlPolymorphic]` / `[TomlDerivedType]`：

- 输入是字符串，多态机制期望的是带 `$type` 的 TOML 表；
- 表达式名是 `RegKey`，不是 Tomlyn 的 string/int discriminator；
- 参数是位置参数，不是表属性。

如果用户给某个表达式类型同时标了 `[TomlPolymorphic]`，转换器会优先（工厂在 `Converters` 里），行为仍以本文为准；但这是无意义的配置，文档里不要推荐。

---

## 8. 错误处理与诊断

### 8.1 两层错误

| 阶段 | 异常 | 处理 |
| --- | --- | --- |
| 注册/构建（`CollectOptions`） | `InvalidOperationException` / `ArgumentException` | 直接让包加载失败；这是配置错误，不应带病运行 |
| TOML 读取（转换器） | `TomlException` | `TomlDataRegistrant` 捕获并记录文件/包，跳过该文件 |
| 用户 delegate 抛非 `TomlException` | 包成 `TomlException`，带表达式名和 inner exception | 同上；避免用户代码的 `NullReferenceException` 直接炸掉整个加载 |

但要注意：Tomlyn 会把转换器里抛出的异常再包一层，最终消息类似：

```text
(12,8) : error : Exception while trying to convert TOML value to type 'EntityExpression'
using converter 'ExpressionTomlConverter`1[...]'. Reason:
(12,8) : error : Unknown expression 'babelrush:damge'.
```

所以表达式自己的消息要**自包含**：包含表达式文本、出错位置、期望/实际类型。不要写“见上文”这类依赖外层上下文的话。

### 8.2 错误消息约定

```text
Expression text is empty.
Expression 'core:add' is missing a closing ')'.
Unexpected text after expression: '...' at offset 17.
Unknown expression 'babelrush:damge'. Registered names in namespace 'babelrush': damage, damage_all, block.
Expression 'core:add' produced AddExpression, but the declared type is EntityExpression.
Expression 'core:add' expects 2 arguments, but 3 were provided.
Argument #2 of 'core:add' could not be read as Expression: ...
Bare expression 'goblin' cannot be read as EntityExpression: no fallback is registered for that type.
Expression nesting exceeds the maximum depth of 32.
No argument converter is registered for type 'My.Asm.EntityType'.
```

建议：

- 未知名字最多列 5 个同命名空间的候选，避免消息爆炸；
- 位置用**表达式字符串内的字符偏移**；`TomlException` 本身带 TOML 文件的行列，两者互补；
- 构建期错误聚合，读期错误逐条抛（一个文件一条）。

### 8.3 日志

- 构建期：不需要逐条日志，抛异常即可。
- 读期：`TomlDataRegistrant` 已经记录 `e.Message`；表达式错误会带文件名和包名。
- 可选：在 `CollectOptions` 完成后 debug 日志输出“注册了 N 个表达式名 / M 个 fallback”，方便排查命名空间问题。

---

## 9. 关键风险与待解决问题

### 9.1 数据加载顺序（最重要）

`EntityExpression` 的 eager fallback（查 `EntityType.Registry` 并构造 `EntityType`）有一个隐藏前提：**实体类型数据必须先于引用它的卡牌/表达式数据加载**。

当前 `DataRegistrationManager.InitializeRegisters()` 的顺序是：

```csharp
AppDomain.CurrentDomain.GetAssemblies()
    .Where(...)
    .SelectMany(assembly => assembly.DefinedTypes)   // 没有排序
    .Where(type => type.GetCustomAttribute<DataRegisterInitializeAttribute>() is not null)
    .ForEach(type => RuntimeHelpers.RunClassConstructor(type.TypeHandle));
```

`assembly.DefinedTypes` 的顺序是元数据顺序，不保证 `EntityType` 在 `CardType` 前面；`DataLoader.Load` 又按 `_dataRegistrants` 的 attach 顺序遍历。因此：

- 如果 `CardType` 的 `/cards` 注册项先加载，而某张卡引用了 `EntityExpression(goblin)`，此时 `EntityType.Registry` 可能还没有 `goblin`；
- eager lookup 会失败（或者拿到 placeholder，取决于实现）。

可选方案：

| 方案 | 优点 | 缺点 |
| --- | --- | --- |
| **A. 表达式存 `RegKey`，求值时再查** | 彻底消除加载顺序依赖；反序列化只解析文本 | 表达式不直接持有 `EntityType`；每次求值要查表（可缓存） |
| **B. 给 `DataRegistrant` 加加载阶段/优先级** | 保持 eager；通用解决其他“类型先于内容”的依赖 | 需要改数据加载框架；优先级/阶段是额外约定 |
| **C. 两遍加载：先类型、后内容** | 语义清晰 | 改动最大；所有 registrant 要区分阶段 |
| **D. 不管，靠当前顺序** | 零成本 | 不可靠；换程序集/改文件就可能坏 |

建议：

- **v1 优先 A**：`EntityExpression` 存 `RegKey`（或存一个可延迟解析的引用），求值时通过 `EntityType.Registry` 解析；fallback 只做 RegKey 解析，不碰注册表。
- 如果产品上确实要求表达式对象里直接是 `EntityType`，则把 **B** 作为前置任务，单独设计“类型数据先于内容数据”的加载阶段；不要把这个依赖藏进表达式框架。
- 这个决定不影响表达式反序列化框架本身：fallback/arg converter 是用户代码，框架只负责调用。

### 9.2 包程序集重复加载

`PackAssemblyLoadContext` 为每个包创建独立 ALC。如果两个包各自带了一份相同的程序集（含 `[ExpressionDefinition]` 的类），会加载出两份 `Type`，扫描后产生**同名注册冲突**。这是现有 `[AutoContentsLoader]` / `[TomlConverterFor]` 也有的问题。

缓解：

- 表达式注册应放在 Core 或共享程序集，不要复制进多个包；
- 包内自定义表达式用包自己的 `RegKey` 命名空间（约定等于 pack id），从命名上避免冲突；
- 构建期冲突错误要列出两个来源（程序集/类型），方便定位。

### 9.3 全局注册表与测试隔离

表达式注册是进程级全局状态，和 `CardType.Registry` 类似。测试反复 `CollectOptions` 时：

- `ResetForTests()` 只清冻结的 registry，不清手动描述符；否则 `[DataRegisterInitialize]` 只跑一次，重新初始化后手动注册会丢；
- 测试程序集里带 `[ExpressionDefinition]` 的类型会被每次扫描到，所以测试名要用独立命名空间（例如 `test:`），且不要和手动注册重名；
- 需要隔离测试时，用内部的 `ExpressionRegistryBuilder` 直接构造一个 registry + 自定义工厂，不碰全局单例；
- `SerializingOptionsTestAccessor.Reset()` 要同时复位表达式 registry 和命名空间缓存，避免旧 options 持有旧 registry。

### 9.4 特性扫描时机

`AttributedTypeScanner` 扫描的是 `AppDomain.CurrentDomain.GetAssemblies()`，只包含**已加载**程序集：

- 包程序集在 `LoadPackAssemblies` 后已加载，`CollectOptions` 能看到；
- 如果某个程序集在 `CollectOptions` 之后才加载，它的表达式特性不会被发现；
- 运行时热加载不支持；文档明确“注册必须在启动/包加载阶段完成”。

### 9.5 错误包装噪音

Tomlyn 会包一层转换器异常，消息里带内部转换器类型名。这是库行为，无法通过配置关掉。缓解：

- 内层消息短而完整；
- 不要依赖外层消息做测试断言，测试断言内层关键字；
- 如果以后觉得太吵，可以在 `TomlDataRegistrant` 里对 `TomlException` 做一次清理/截断，但那是展示层问题，不是本设计的一部分。

### 9.6 性能与 AOT

- 反序列化只发生在内容加载期，不在每帧；解析器不需要极致优化。
- 构造器 binder 用 `ConstructorInfo.Invoke` 足够；以后热点明显再换 compiled expression/IL。
- 反射扫描 + `Activator` 不是 NativeAOT/trimming 友好的；项目当前没有 AOT 目标，和现有 `AttributedTypeScanner` 保持一致。
- 如果未来要 AOT，需要源生成器或显式注册表，届时表达式注册的手动层可以保留，特性扫描层替换掉。

### 9.7 其他待定

- 表达式名允许的字符集：建议 `[A-Za-z0-9_.-]` + 一个 `:`；如果现有 `RegKey` 用了别的字符，需要放宽。
- `Expression` 这个类型名与 `System.Linq.Expressions.Expression` 冲突：本仓库目前不用 LINQ 表达式；如果担心，可改名 `GameExpression`/`Expr`，或统一用别名。**推荐先用 `Expression`**，命名空间 `BabelRushR.Core.Expressions` 足以区分。
- 一个类型多个命名但不同参数模式：特性层不支持（共用构造器）；需要时用手动 delegate 注册多个名字。
- 失败重试：`SerializingOptions.CollectOptions` 目前一次性，构建失败后同进程不能重试；表达式注册表继承这个现状。

---

## 10. 接入点清单

### 10.1 新增文件

```text
BabelRushR.Core/Expressions/
├── Expression.cs
├── ExpressionDeserialization.cs
├── ExpressionAttributes.cs
├── ExpressionArgs.cs
├── ExpressionFallbackArgs.cs
├── ExpressionReadContext.cs
├── ExpressionArgContext.cs
├── IExpressionArgConverter.cs
├── ExpressionArgConverters.cs
├── ExpressionParser.cs
├── ExpressionRegistry.cs
├── ExpressionRegistryBuilder.cs
├── ExpressionAttributeScanner.cs
├── ExpressionConstructorBinder.cs
├── ExpressionTomlConverterFactory.cs
└── ExpressionTomlConverter.cs
```

文件数量可以合并（例如 attributes + contexts 各一个文件），上面的拆分只是职责说明。

### 10.2 修改文件

| 文件 | 改动 |
| --- | --- |
| `Common/Serializing/SerializingOptions.cs` | `CollectOptions()` 里 `Build()` + 工厂加入 `Converters` 末尾 |
| `Common/Serializing/RegKeyConverter.cs` | 加 `DefaultNameSpace` 只读属性（不改 Read 行为） |
| `BabelRushR.Core.Test/SerializingOptionsTestAccessor.cs` | `Reset()` 里复位表达式 registry |
| `WorkPlan.md` | 落地后再勾选「表达式类型 & 复杂反序列化支持」；本轮不动 |

不需要改：

- `TomlDataRegistrant`：它已经捕获 `TomlException`；
- `AttributedTypeScanner`：类级特性扫描已经够用；
- `DataRegistrationManager`：手动注册走现有 `[DataRegisterInitialize]`；
- `EntityType` / `CardType`：等 Phase 3 再接入表达式。

### 10.3 `CollectOptions` 中的精确顺序

```csharp
var polymorphismMappings = CollectPolymorphismMappings();
var converters = CollectConverters();
var typeInfos = CollectTypeInfos();
var expressionRegistry = ExpressionDeserialization.Build();

Default = TomlSerializerOptions.Default with
{
    ...
    Converters = [RegKeyConverter.Default, ..converters, new ExpressionTomlConverterFactory(expressionRegistry)],
    ...
};
```

`Build()` 放在收集完 converters/typeInfos 之后、构造 `Default` 之前；它不依赖前两者，但这样错误顺序更直观（先收集所有注册，再统一构建）。

---

## 11. API 草案（完整签名）

```csharp
namespace BabelRushR.Core.Expressions;

public abstract record Expression;

public static class ExpressionDeserialization
{
    public static void Register<TExpression>(
        RegKey name,
        Func<ExpressionArgs, TExpression> deserializer)
        where TExpression : Expression;

    public static void Register<TExpression>(
        string name,
        Func<ExpressionArgs, TExpression> deserializer)
        where TExpression : Expression;

    public static void RegisterFallback<TExpression>(
        Func<ExpressionFallbackArgs, TExpression> fallback)
        where TExpression : Expression;

    public static void RegisterArgConverter<T>(
        Func<ExpressionArgContext, T> converter);

    public static void RegisterArgConverter(IExpressionArgConverter converter);
}

public sealed class ExpressionArgs
{
    public int Count { get; }
    public int Position { get; }
    public bool HasMore { get; }

    public T Read<T>();
    public RegKey ReadRegKey();
    public string ReadString();
    public void EnsureEnd();
}

public sealed class ExpressionFallbackArgs
{
    public string Text { get; }
    public ExpressionReadContext Context { get; }

    public T Read<T>();
    public RegKey ReadRegKey();
    public string ReadString();
}

public sealed class ExpressionReadContext
{
    public TomlSerializerOptions Options { get; }
    public string? DefaultNamespace { get; }
    public int Depth { get; }

    public TomlException CreateException(string message);
    public RegKey ParseRegKey(string text);
}

public sealed class ExpressionArgContext
{
    public string Text { get; }
    public ExpressionReadContext Context { get; }

    public RegKey ParseRegKey();
    public TomlException CreateException(string message);
}

public interface IExpressionArgConverter
{
    bool CanConvert(Type type);
    object? Read(ExpressionArgContext context);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class ExpressionDefinitionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ExpressionFallbackAttribute : Attribute;

[AttributeUsage(AttributeTargets.Constructor, Inherited = false)]
public sealed class ExpressionConstructorAttribute : Attribute;
```

说明：

- `ExpressionRegistry` / 工厂 / parser / binder 都是 `internal`；公开面只保留注册、参数读取、上下文、特性和基类。
- `ExpressionReadContext.Options` 暴露 `TomlSerializerOptions`，方便自定义参数转换器复用 Tomlyn；不暴露 `TomlReader`，避免公共契约绑死 Tomlyn 的低层类型。
- `ExpressionArgs.Read<T>()` 是主入口；`ReadRegKey()` / `ReadString()` 是常用快捷方式。

---

## 12. 分阶段实施计划

依赖关系（先下后上）：

```text
Expression 基类
  ├─ ExpressionParser
  ├─ ExpressionReadContext / ExpressionArgContext
  ├─ ExpressionArgConverters（内置 + 自定义）
  ├─ ExpressionArgs / ExpressionFallbackArgs
  └─ ExpressionRegistryBuilder → ExpressionRegistry
        ├─ 手动注册 API（ExpressionDeserialization）
        └─ ExpressionAttributeScanner → ExpressionConstructorBinder
              └─ ExpressionTomlConverterFactory → ExpressionTomlConverter<T>
                    └─ SerializingOptions.CollectOptions
```

### Phase 0：拍板与前置确认

- 确认 §4.2 的“裸 = fallback、括号 = 命名”规则；
- 确认 §9.1 的实体引用策略（eager `EntityType` vs lazy `RegKey`）；
- 确认 §7.6 不给 `Expression` 标 `[TomlPolymorphic]`；
- 确认 `Expression` 类型名和命名空间。

**退出条件**：上述四点有明确结论，后续不需要返工。

### Phase 1：最小可用核心（只支持手动注册）

交付：

- `Expression` 基类；
- `ExpressionParser`（名称/参数切分、引号、空白、错误位置）；
- `ExpressionArgs` + 内置参数转换器（`Expression`、`RegKey`、`string`、`bool`、数值、枚举）；
- `ExpressionRegistry` / `ExpressionRegistryBuilder`（手动 `Register` / `RegisterFallback` / `RegisterArgConverter`）；
- `ExpressionTomlConverterFactory` / `ExpressionTomlConverter<T>`；
- `SerializingOptions` 接入 + `SerializingOptionsTestAccessor` 复位。

测试：手动注册的解析、嵌套、命名空间、类型检查、错误路径（见 §13 的 Parser/Registry/Arg 部分）。

**退出条件**：一个声明为 `Expression` 的 TOML 字段可以读 `"core:add(1, 2)"`，其中 `AddExpression` / `ConstantExpression` 全部手动注册，不依赖任何特性。

### Phase 2：特性与自动扫描

交付：

- `[ExpressionDefinition]` / `[ExpressionFallback]` / `[ExpressionConstructor]`；
- `ExpressionAttributeScanner` + `ExpressionConstructorBinder`；
- 构建期校验（类型、构造器、参数转换器、重名、fallback 冲突）；
- 冲突/覆盖规则（手动 fallback 覆盖特性 fallback；命名重复报错）。

测试：特性扫描、构造器选择、fallback、非法类型、重复名、缺少参数转换器。

**退出条件**：`[ExpressionDefinition("core:add")] record AddExpression(Expression Left, Expression Right)` 零手动注册即可工作；`EntityExpression` 的 `[ExpressionFallback]` 在注册了 `EntityType` 参数转换器后工作。

### Phase 3：游戏侧接入

交付：

- `ConstantExpression` / `AddExpression` / 其他算术表达式；
- `EntityExpression`（按 §9.1 的决议选择 eager 或 lazy）；
- `EntityType` 参数转换器 + fallback；
- 把某个卡牌字段从 `int` / `RegKey` 改成 `Expression`（或新增字段），在 `Contents/babelrush/data/cards/cards.toml` 里加一条示例；
- 数据加载顺序的前置处理（如果选 eager）。

测试：端到端 TOML → 卡牌类型 → 表达式对象；实体引用；命名空间；错误数据。

**退出条件**：现有卡牌数据仍能加载；新示例卡使用表达式计算伤害/数值；错误实体名在加载期报错。

### Phase 4：打磨

- 错误消息加表达式内偏移；
- 未知名字候选列表；
- 深度/参数个数/长度限制可配置；
- 文档（`Docs/ExpressionDeserialization.md` 更新为最终版）；
- 可选：表达式写回（如果确实需要 round-trip）。

**退出条件**：错误信息足够定位数据问题；没有待补的 TODO。

---

## 13. 测试矩阵

> 测试设计按 xunit.v3 / MTP 的现有风格；Core 测试当前由另一处在修，本轮不执行。

### 13.1 Parser

| 用例 | 期望 |
| --- | --- |
| `"goblin"` | 无括号，name 为空，裸文本 `goblin` |
| `"core:entity(goblin)"` | name `core:entity`，1 个参数 |
| `"core:add(1, 2)"` | 2 个参数，trim 后 `1` / `2` |
| `"core:add(1, core:mul(2, 3))"` | 2 个参数，第二个是嵌套表达式原文 |
| `"core:say(\"a,b\", 'c(d)')"` | 2 个参数，引号内的逗号/括号不切分 |
| `" core:add ( 1 , 2 ) "` | 空白容忍 |
| `"core:add(1,)"` | 空参数错误 |
| `"core:add(1"` | 缺少 `)` |
| `"core:add(1) trailing"` | 尾随文本错误 |
| `"()"` | 空名称错误 |
| `""` | 空表达式（有 fallback 时交给 fallback，否则错误） |
| 深度超过上限 | 深度错误 |

### 13.2 Registry

| 用例 | 期望 |
| --- | --- |
| 同名重复注册 | 构建期抛，消息含两个来源 |
| 未全限定名 | `Register` 立即抛 |
| 未知名字 | 读期 `TomlException`，列同命名空间候选 |
| 结果类型不可赋给声明类型 | 读期 `TomlException`，带期望/实际类型 |
| 没有 fallback 的裸文本 | 读期 `TomlException` |
| fallback 返回 null | 读期 `TomlException` |
| 参数多传 | 自动 `EnsureEnd` 报错 |
| 参数少传 | `Read` 越界报错 |
| 构建后 `Register` | `InvalidOperationException` |
| 手动 fallback 覆盖特性 fallback | 手动生效，不抛 |
| 两个手动 fallback | 构建期抛 |

### 13.3 参数类型

| 用例 | 期望 |
| --- | --- |
| `int` / `long` / `double` / `decimal` | 正确解析；越界报错 |
| `bool` | `true` / `false`；其他报错 |
| `string` 引号 / 裸文本 | 正确；逗号/括号需引号 |
| `RegKey` 显式 / 默认命名空间 | 正确；无默认命名空间时报错 |
| 枚举名字 / 数字 | 正确；未定义值按策略 |
| `Expression` 派生 | 递归解析；类型检查 |
| 自定义 `EntityType` | 走注册的转换器；未知 key 报错 |
| 未注册的自定义类型 | 构建期（特性）或读期（手动）报错 |
| 非字符串 TOML 值 | `TomlException`，提示必须是字符串 |

### 13.4 Tomlyn / 集成

| 用例 | 期望 |
| --- | --- |
| 属性声明 `Expression` | 工厂创建 `ExpressionTomlConverter<Expression>` |
| 属性声明具体表达式类型 | 工厂创建对应泛型转换器 |
| `List<Expression>` / `Dictionary<string, Expression>` | 元素走表达式转换器 |
| 显式 `[TomlConverterFor]` + 表达式工厂 | 显式转换器优先（工厂在最后） |
| `WithDefaultNamespace("pack")` 裸名 | 解析为 `pack:name` |
| 显式 `"core:name"` | 不被默认命名空间覆盖 |
| `DefaultBeforeInitialize` 含 `Expression` 字段 | 不保证支持；应报错（文档明确） |
| 没有表达式的现有 TOML | 行为不变 |

### 13.5 特性

| 用例 | 期望 |
| --- | --- |
| `[ExpressionDefinition]` 单/多名字 | 自动注册；同一构造器模式 |
| `[ExpressionFallback]` 单参数 | 裸文本可用 |
| `[ExpressionConstructor]` 多构造器消歧 | 选中标注的 |
| 多 public 构造器未标注 | 构建期抛 |
| abstract / open generic / 非 `Expression` | 构建期抛 |
| fallback 参数是 `Expression` 派生 | 构建期抛（递归风险） |
| fallback 参数无转换器 | 构建期抛 |
| 跨程序集类型（测试程序集） | 扫描到 |

### 13.6 生命周期 / 测试隔离

| 用例 | 期望 |
| --- | --- |
| `CollectOptions` 重复（测试复位） | registry 重建，手动描述符不丢 |
| 复位后命名空间缓存 | 清空，不会拿到旧 factory |
| 测试特性类型重复扫描 | 不重复注册（每次 build 从描述符重建） |
| 包加载失败（非法注册） | 异常含类型/名字，包清理逻辑不变 |

---

## 14. 验收标准

- [ ] `Expression` 字段能从 `"name"` 和 `"name(arg, ...)"` 两种形式反序列化。
- [ ] 声明为具体派生类型的字段只接受兼容结果；不兼容时报清晰错误。
- [ ] 表达式名是 `RegKey`；裸名使用包默认命名空间，显式名不被覆盖。
- [ ] 手动注册和特性声明都可用；派生类在常见 record 情况下只需特性。
- [ ] 外部程序集（含包 ALC）里的表达式类型能被发现。
- [ ] 参数支持表达式递归、`RegKey`、字符串、布尔、数值、枚举和自定义类型。
- [ ] 构建期校验覆盖重名、非法类型、构造器歧义、缺失参数转换器、fallback 冲突。
- [ ] 读期错误都是 `TomlException`，能被 `TomlDataRegistrant` 捕获并记录文件。
- [ ] `SerializingOptionsTestAccessor` 能复位 registry，测试可重复初始化。
- [ ] 不含表达式的现有 TOML 行为不变。
- [ ] 框架不含任何求值/业务逻辑；`Write` 明确不支持。
- [ ] 现有 Core/Mvvm 测试在另一处修复完成后保持通过（本轮不执行）。

---

## 15. 需要拍板的点

按影响从大到小：

1. **实体引用策略**：`EntityExpression` 直接持有 `EntityType`（eager，需要数据加载阶段）还是持有 `RegKey`（lazy，无顺序依赖）？
   - 推荐：v1 用 `RegKey`；如果必须 eager，先单独做“类型数据先于内容数据”的加载阶段。
2. **裸文本规则**：无括号只走 fallback（推荐），还是先查零参数命名注册再 fallback？
3. **特性形态**：`[ExpressionDefinition]` + `[ExpressionFallback]` 两个特性（推荐），还是一个合并特性？
4. **字符串参数**：允许裸文本 + 引号（推荐，方便实体名），还是只允许引号？
5. **写支持**：`Write` 抛异常（推荐，明确不支持），还是现在就用一个 formatter 注册表做 round-trip？
6. **基类名字**：`Expression`（推荐）还是 `GameExpression`/`Expr`（规避 `System.Linq.Expressions` 重名）？
7. **命名空间/字符集**：表达式名限制为 ASCII + `_.-:`，还是完全沿用 `RegKey` 的宽松规则？

---

## 16. 示例：卡牌动作接入（Phase 3 预览）

现状（`RecordCardType`）：

```csharp
public record RecordCardType(RegKey ID, int Cost, ImmutableArray<(IEntityActionType Type, int Value)> Actions) : ICardType;
```

接入表达式后可能变成：

```csharp
public record RecordCardType(
    RegKey ID,
    int Cost,
    ImmutableArray<(IEntityActionType Type, Expression Value)> Actions) : ICardType;
```

TOML：

```toml
[[card]]
id = "strike"
cost = 2

[[card.actions]]
type = "core:attack"
value = "6"                         # 根 Expression fallback -> 常量 6

[[card]]
id = "heavy_strike"
cost = 3

[[card.actions]]
type = "core:attack"
value = "core:add(6, core:mul(2, core:field(\"strength\")))"
```

这要求：

- `Expression` 根 fallback 注册（数字 → 常量表达式）；
- `core:add` / `core:mul` 注册，参数为 `Expression`；
- `core:field("strength")` 这类“字段引用”可以是一个命名表达式，也可以由具体表达式类型自己实现；裸 `strength` 会不会被解释成字段，取决于根 fallback 的实现；
- `RecordCardType` 的 `int Value` 改成 `Expression` 会影响 `RecordTypedCard`、`EntityActionType.CreateAction(int)` 等，Phase 3 再处理，不在本设计范围。

---

## 17. 参考

- Tomlyn 仓库：<https://github.com/xoofx/Tomlyn>
- Tomlyn 文档：<https://xoofx.github.io/Tomlyn/>
- 序列化（转换器/工厂/多态）：<https://xoofx.github.io/Tomlyn/docs/serialization/>
- 低层 API：<https://xoofx.github.io/Tomlyn/docs/low-level/>
- `KirisameY.Registration` 的 `RegKey` / 注册表 API（本机包，无公开文档）
- 现有 TOML 收集逻辑：`BabelRushR.Core/Common/Serializing/SerializingOptions.cs`
- 现有数据加载顺序：`BabelRushR.Core/Contents/ContentsLoaderManager.cs`、`BabelRushR.Core/Data/DataRegistrationManager.cs`
