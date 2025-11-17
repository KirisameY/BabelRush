这个项目算是寄了，初期开发模式就不对还有意拖延了很多问题，到今天这步已经不能囿于沉没成本不敢切割了。
我整理一下它尚未挪到库里的遗产写在这儿吧：

- addons(GD编辑器插件，不加入lib，回头可以直接复制用)
    - Exportation(打包时另行导出部分资源作为zip数据包)
    - Importation(Lua脚本作为字符串导入资源)
- Data
    - Model/Set定义
    - 相关检查及生成器（有待改进，并且需要给生成器做代码检查）
- I18n(I18n项的延迟更新实现部分)
- Numerics(尚不成熟先不加入lib)
- Registering
    - RegKey(改个名字能直接整合进lib)
    - 加载系统(得先改良结构或者重构掉)
    - I18n支持(同↑)
- Scripting(Lua沙箱相关，尚不成熟先不加入lib，回头复制了用)
- Utils.RandomUtils(加权平均)